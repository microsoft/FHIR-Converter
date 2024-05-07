@description('Location where the resources are deployed.')
@allowed([
  'australiaeast'
  'canadacentral'
  'centralindia'
  'centralus'
  'eastus'
  'eastus2'
  'francecentral'
  'germanywestcentral'
  'japaneast'
  'koreacentral'
  'northcentralus'
  'northeurope'
  'southcentralus'
  'southeastasia'
  'swedencentral'
  'switzerlandnorth'
  'uksouth'
  'westeurope'
  'westus2'
  'westus3'
])
param location string

@description('The name of the container app running the FHIR-Converter service.')
param appName string

@description('The name of the container apps environment where the app will run.')
param envName string

@description('Name of storage account containing custom templates. Leave blank if using default templates.')
param templateStorageAccountName string = ''

@description('Name of the container in the storage account containing custom templates. Leave blank if using default templates.')
param templateStorageAccountContainerName string = ''

@description('Minimum possible number of replicas per revision as the container app scales.')
param minReplicas int = 0

@description('Maximum possible number of replicas per revision as the container app scales.')
param maxReplicas int = 30

@description('CPU usage limit in cores.')
param cpuLimit string = '1.0'

@description('Memory usage limit in Gi.')
param memoryLimit string = '2Gi'

@description('If set to true, security will be enabled on the API endpoint.')
param securityEnabled bool = false

@description('List of audiences that the security token is intended for.')
param securityAuthenticationAudiences array = []

@description('Issuing authority of the JWT token.')
param securityAuthenticationAuthority string = ''

@description('Tag of the image to deploy.')
param imageTag string

@description('Timestamp to append to container name. Defaults to time of deployment.')
param timestamp string = utcNow('yyyyMMddHHmmss')

// Security configuration
var securityEnabledConfigName = 'ConvertService__Security__Enabled'
var securityAuthenticationAudiencesConfigNamePrefix = 'ConvertService__Security__Authentication__Audiences__'
var securityAuthenticationAuthorityConfigName = 'ConvertService__Security__Authentication__Authority'
var securityConfiguration = [
  {
    name: securityEnabledConfigName
    value: string(securityEnabled)
  }
  {
    name: securityAuthenticationAuthorityConfigName
    value: securityAuthenticationAuthority
  }
]

var securityAuthenticationAudiencesConfig = [for (audience, i) in securityAuthenticationAudiences: {
    name: '${securityAuthenticationAudiencesConfigNamePrefix}${i}'
    value: audience
}]

// Template hosting configuration
var storageEnvironmentSuffix = az.environment().suffixes.storage
var blobTemplateHostingConfigurationName = 'TemplateHosting__StorageAccountConfiguration__ContainerUrl'
var blobTemplateHostingConfigurationValue = 'https://${templateStorageAccountName}.blob.${storageEnvironmentSuffix}/${templateStorageAccountContainerName}'
var blobTemplateHostingConfiguration = [
  {
    name: blobTemplateHostingConfigurationName
    value: blobTemplateHostingConfigurationValue
  }
]

// Telemetry configuration
var applicationInsightsName = '${envName}-ai'
resource applicationInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: applicationInsightsName
}

resource applicationInsightsUAMI 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name: '${applicationInsightsName}-mi'
}

var appInsightsConnectionStringConfigurationName = 'ConvertService__Telemetry__AzureMonitor__ApplicationInsightsConnectionString'
var appInsightsConnectionString = applicationInsights.properties.ConnectionString
var appInsightsUAMIClientIdConfigurationName = 'ConvertService__Telemetry__AzureMonitor__ManagedIdentityClientId'
var appInsightsUAMIClientId = applicationInsightsUAMI.properties.clientId
var telemetryConfiguration = [
    {
        name: appInsightsConnectionStringConfigurationName
        value: appInsightsConnectionString
    }
    {
        name: appInsightsUAMIClientIdConfigurationName
        value: appInsightsUAMIClientId
    }
]

// Get Container Apps Environment
resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2023-05-01' existing = {
  name: envName
}

// Environment Variables for Container App
var envConfiguration =  concat(securityConfiguration, securityAuthenticationAudiencesConfig, telemetryConfiguration, empty(templateStorageAccountName) ? [] : blobTemplateHostingConfiguration)

var microsoftCR = ''
var imageName = 'healthcareapis/fhir-converter'

resource containerApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: appName
  location: location
  identity: {
    type: 'SystemAssigned, UserAssigned'
    userAssignedIdentities: {
      '${applicationInsightsUAMI.id}': {}
    }
  }
  properties:{
    managedEnvironmentId: containerAppEnvironment.id
    configuration: {
      ingress: {
        targetPort: 8080
        external: true
      }
    }
    template: {
      containers: [
        {
          image: 'mcr.microsoft.com/${imageName}:${imageTag}'
          name: 'fhir-converter-${timestamp}'
          env: envConfiguration
          resources: {
            cpu: json(cpuLimit)
            memory: memoryLimit
          }
        }
      ]
      scale: {
        minReplicas: minReplicas
        maxReplicas: maxReplicas
      }
    }
  }
}

// Grant container app's system MI to read from storage account
resource templateStorageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' existing = if (!empty(templateStorageAccountName)) {
  name: templateStorageAccountName
}

var roleAssignmentName = guid(templateStorageAccount.id, appName, storageBlobDataReaderRoleDefinitionId)
var storageBlobDataReaderRoleDefinitionId = resourceId('Microsoft.Authorization/roleDefinitions', '2a2b9908-6ea1-4ae2-8e65-a410df84e7d1')
resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (!empty(templateStorageAccountName)) {
  name: guid(roleAssignmentName)
  scope: templateStorageAccount
  properties: {
    principalId: containerApp.identity.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: storageBlobDataReaderRoleDefinitionId
  }
}

// Output
output containerAppFQDN string = containerApp.properties.configuration.ingress.fqdn
output containerAppLatestRevisionName string = containerApp.properties.latestRevisionName
