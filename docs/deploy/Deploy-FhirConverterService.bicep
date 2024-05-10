/*
This template deploys the following:
* A container app running the FHIR-Converter
* Role assignment for the container app to read custom templates from the storage container (if the template storage account and container names are specified)
*/

@description('Location where the resources are deployed. For list of Azure regions where Container Apps is available, see [Products available by region](https://azure.microsoft.com/en-us/explore/global-infrastructure/products-by-region/?products=container-apps)')
@allowed([
  'australiaeast'
  'brazilsouth'
  'canadacentral'
  'canadaeast'
  'centralindia'
  'centralus'
  'chinanorth3'
  'eastasia'
  'eastus'
  'eastus2'
  'francecentral'
  'germanywestcentral'
  'japaneast'
  'koreacentral'
  'northcentralus'
  'northeurope'
  'norwayeast'
  'southafricanorth'
  'southcentralus'
  'southeastasia'
  'swedencentral'
  'switzerlandnorth'
  'uaenorth'
  'uksouth'
  'ukwest'
  'westcentralus'
  'westeurope'
  'westus'
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

@description('Name of the key vault containing the application insights connection string secret.')
param keyVaultName string = ''

@description('Name of the user-assigned managed identity to be used by the container app to access key vault secrets.')
param keyVaultUAMIName string = ''

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

@description('List of audiences that the authentication token is intended for.')
param securityAuthenticationAudiences array = []

@description('Issuing authority of the JWT token.')
param securityAuthenticationAuthority string = ''

@description('Tag of the image to deploy. To see available image versions, visit the [FHIR Converter MCR page](https://mcr.microsoft.com/en-us/product/healthcareapis/fhir-converter/tags)')
param imageTag string

@description('Timestamp to append to container name. Defaults to time of deployment.')
param timestamp string = utcNow('yyyyMMddHHmmss')

@description('The ID of the user-assigned managed identity to be used by the container app to access application insights.')
param applicationInsightsUAMIName string = ''

param applicationInsightsConnectionStringSecretName string = ''

var configureApplicationInsights = !empty(applicationInsightsUAMIName)

// Get the UAMI with access to application insights
resource applicationInsightsUAMI 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = if (configureApplicationInsights) {
  name: applicationInsightsUAMIName
}

// Get the container apps environment
resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2023-05-01' existing = {
  name: envName
}

// Security configuration
var securityEnabledConfigName = 'ConvertService__Security__Enabled'
var securityAuthenticationAudiencesConfigNamePrefix = 'ConvertService__Security__Authentication__Audiences__'
var securityAuthenticationAuthorityConfigName = 'ConvertService__Security__Authentication__Authority'

var securityEnabledConfiguration = [
  {
    name: securityEnabledConfigName
    value: string(securityEnabled)
  }
]

var securityAuthenticationAuthorityConfig = [
  {
    name: securityAuthenticationAuthorityConfigName
    value: securityAuthenticationAuthority
  }
]

var securityAuthenticationAudiencesConfig = [for (audience, i) in securityAuthenticationAudiences: {
    name: '${securityAuthenticationAudiencesConfigNamePrefix}${i}'
    value: audience
}]

var securityConfiguration = concat(securityEnabledConfiguration, securityEnabled ? concat(securityAuthenticationAuthorityConfig, securityAuthenticationAudiencesConfig) : [])

var integrateTemplateStore = !empty(templateStorageAccountName) && !empty(templateStorageAccountContainerName)

// Template hosting configuration
var storageEnvironmentSuffix = az.environment().suffixes.storage
var blobTemplateHostingConfigurationName = 'TemplateHosting__StorageAccountConfiguration__ContainerUrl'
var blobTemplateHostingConfigurationValue = 'https://${templateStorageAccountName}.blob.${storageEnvironmentSuffix}/${templateStorageAccountContainerName}'
var blobTemplateHostingConfiguration = integrateTemplateStore ? [
  {
    name: blobTemplateHostingConfigurationName
    value: blobTemplateHostingConfigurationValue
  }
] : []

// Application insights configuration
var applicationInsightsConnectionStringConfigurationName = 'ConvertService__Telemetry__AzureMonitor__ApplicationInsightsConnectionString'
var applicationInsightsUAMIClientIdConfigurationName = 'ConvertService__Telemetry__AzureMonitor__ManagedIdentityClientId'
var telemetryConfiguration = configureApplicationInsights ? [
    {
        name: applicationInsightsConnectionStringConfigurationName
        secretRef: applicationInsightsConnectionStringSecretName
    }
    {
        name: applicationInsightsUAMIClientIdConfigurationName
        value: applicationInsightsUAMI.properties.clientId
    }
] : []

// Environment Variables for Container App
var envConfiguration =  concat(securityConfiguration, telemetryConfiguration, blobTemplateHostingConfiguration)

var imageName = 'healthcareapis/fhir-converter'

// Configure identities
var applicationInsightsUAMIResourceId = configureApplicationInsights ? applicationInsightsUAMI.id : ''
var keyVaultUAMIResourceId = resourceId('Microsoft.ManagedIdentity/userAssignedIdentities', keyVaultUAMIName)
var userAssignedIdentities = configureApplicationInsights ? {
  '${applicationInsightsUAMIResourceId}' : {}
  '${keyVaultUAMIResourceId}' : {}
} : {}

var akvEnvironmentSuffix = az.environment().suffixes.keyvaultDns
var applicationInsightsConnStringAKVSecretUrl = 'https://${keyVaultName}${akvEnvironmentSuffix}/secrets/${applicationInsightsConnectionStringSecretName}'

resource containerApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: appName
  location: location
  identity: (configureApplicationInsights) ? {
    type: 'SystemAssigned, UserAssigned'
    userAssignedIdentities: userAssignedIdentities
  } : {
    type: 'SystemAssigned'
  }
  properties:{
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      ingress: {
        targetPort: 8080
        external: true
      }
      secrets: configureApplicationInsights ? [
        {
          name: applicationInsightsConnectionStringSecretName
          keyVaultUrl: applicationInsightsConnStringAKVSecretUrl
          identity: keyVaultUAMIResourceId
        }
      ] : []
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
  tags: {
    fhirConverterEnvName: envName
    fhirConverterAppName: appName
    fhirConverterImageName: imageName
    fhirConverterImageVersion: imageTag
  }
}

// Reference the existing storage account
resource templateStorageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' existing = if (integrateTemplateStore) {
  name: templateStorageAccountName
}

// Reference the existing blob service
resource templateBlobService 'Microsoft.Storage/storageAccounts/blobServices@2022-09-01' existing = if (integrateTemplateStore) {
  name: 'default'
  parent: templateStorageAccount
}

// Reference the existing container
resource templateStorageAccountContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2022-09-01' existing = if (integrateTemplateStore) {
  name: templateStorageAccountContainerName
  parent: templateBlobService
}

var roleAssignmentName = guid(templateStorageAccountContainer.id, appName, storageBlobDataReaderRoleDefinitionId)
var storageBlobDataReaderRoleDefinitionId = resourceId('Microsoft.Authorization/roleDefinitions', '2a2b9908-6ea1-4ae2-8e65-a410df84e7d1')
resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (integrateTemplateStore) {
  name: guid(roleAssignmentName)
  scope: templateStorageAccountContainer
  properties: {
    principalId: containerApp.identity.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: storageBlobDataReaderRoleDefinitionId
  }
}

// Output
output containerAppFQDN string = containerApp.properties.configuration.ingress.fqdn
output containerAppLatestRevisionName string = containerApp.properties.latestRevisionName
