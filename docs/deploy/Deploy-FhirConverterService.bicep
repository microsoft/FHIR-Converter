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

@description('Timestamp to append to container name. Defaults to time of deployment.')
param timestamp string = utcNow('yyyyMMddHHmmss')

@description('The name of the container app running the FHIR-Converter service.')
param appName string

@description('The name of the container apps environment where the app will run.')
param envName string

@description('Tag of the image to deploy. To see available image versions, visit the [FHIR Converter MCR page](https://mcr.microsoft.com/en-us/product/healthcareapis/fhir-converter/tags)')
param imageTag string

@description('If set to true, the converter will pull custom templates from the specified storage account.')
param useCustomTemplates bool = false

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

@description('If set to true, application logs and metrics will be sent to the specified application insights instance.')
param useApplicationInsights bool

@description('The resource ID of the user-assigned managed identity used to access the application insights instance.')
param applicationInsightsUAMIName string = ''

@description('Name of the secret in the key vault containing the connection string to the application insights instance.')
param applicationInsightsConnStringSecretName string = ''

@description('The ID of the container apps environment where the container app should be deployed to.')
param containerAppEnvironmentId string

// Get the user-assigned identity that has Monitoring Metric Publisher access to the application insights instance
resource applicationInsightsUAMI 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = if (!empty(applicationInsightsUAMIName)) {
    name: applicationInsightsUAMIName
}

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

// Application insights configuration
var applicationInsightsConnectionStringConfigurationName = 'ConvertService__Telemetry__AzureMonitor__ApplicationInsightsConnectionString'
var applicationInsightsUAMIClientIdConfigurationName = 'ConvertService__Telemetry__AzureMonitor__ManagedIdentityClientId'
var telemetryConfiguration = [
    {
        name: applicationInsightsConnectionStringConfigurationName
        secretRef: applicationInsightsConnStringSecretName
    }
    {
        name: applicationInsightsUAMIClientIdConfigurationName
        value: applicationInsightsUAMI.properties.clientId
    }
]

// Environment Variables for Container App
var envConfiguration =  concat(securityConfiguration, securityAuthenticationAudiencesConfig, telemetryConfiguration, empty(templateStorageAccountName) ? [] : blobTemplateHostingConfiguration)

var imageName = 'healthcareapis/fhir-converter'

var keyVaultUAMIResourceId = resourceId('Microsoft.ManagedIdentity/userAssignedIdentities', keyVaultUAMIName)

var applicationInsightsUAMIResourceId = useApplicationInsights ? applicationInsightsUAMI.id : ''

var userAssignedIdentities = useApplicationInsights ? {
  '${applicationInsightsUAMIResourceId}': {}
  '${keyVaultUAMIResourceId}': {}
} : {}

var akvEnvironmentSuffix = az.environment().suffixes.keyvaultDns
var appInsightsConnStringAKVSecretUrl = 'https://${keyVaultName}${akvEnvironmentSuffix}/secrets/${applicationInsightsConnStringSecretName}'

resource containerApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: appName
  location: location
  identity: (useApplicationInsights) ? {
	type: 'SystemAssigned, UserAssigned'
	userAssignedIdentities: userAssignedIdentities
  } : {
    type: 'SystemAssigned'
  }
  properties:{
    managedEnvironmentId: containerAppEnvironmentId
    configuration: {
      ingress: {
        targetPort: 8080
        external: true
      }
      secrets: [
          {
              name: applicationInsightsConnStringSecretName
              keyVaultUrl: appInsightsConnStringAKVSecretUrl
              identity: keyVaultUAMIResourceId
          }
      ]
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

// Grant container app's system MI to read from storage account
resource templateStorageAccount'Microsoft.Storage/storageAccounts/blobServices/containers@2022-09-01' existing = if (!empty(templateStorageAccountName)) {
  name: templateStorageAccountName
}

var roleAssignmentName = guid(templateStorageAccount.id, appName, storageBlobDataReaderRoleDefinitionId)
var storageBlobDataReaderRoleDefinitionId = resourceId('Microsoft.Authorization/roleDefinitions', '2a2b9908-6ea1-4ae2-8e65-a410df84e7d1')
resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (!empty(templateStorageAccountName)) {
  name: roleAssignmentName
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
