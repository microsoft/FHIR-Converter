param location string = resourceGroup().location
param appName string
param envName string

// Storage Account configuration
param templateStorageAccountName string = ''
param templateStorageAccountContainerName string = ''

// Container configuration
param minReplicas int = 0
param maxReplicas int = 30
param cpuLimit string = '1.0'
param memoryLimit string = '2Gi'

// Security configuration
param securityEnabled bool = false
param securityAuthenticationAudiences array = []
param securityAuthenticationAuthority string = ''

// KV details containing image pull password token - TODO remove once image is public
param keyVaultName string
param imagePullTokenName string
param imagePullPwdSecretName string
param keyVaultUAMIName string

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

param timestamp string = utcNow('yyyyMMddHHmmss')

var microsoftCR = 'hlsacaplatformtestacr' // TODO replace with MCR once image is public
var imageName = 'convertservice' // TODO replace with MCR image name once it is public
param imageTag string = 'latest'

// Secret reference for image pull password - TODO remove once image is public in MCR
var akvEnvironmentSuffix = az.environment().suffixes.keyvaultDns
var imagePullAKVSecretUrl = 'https://${keyVaultName}${akvEnvironmentSuffix}/secrets/${imagePullPwdSecretName}'
var imagePullSecretName = 'imagepullsecret'
resource keyVaultUAMI 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name: keyVaultUAMIName
}

resource containerApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: appName
  location: location
  identity: {
    type: 'SystemAssigned, UserAssigned'
    userAssignedIdentities: {
      '${keyVaultUAMI.id}': {}
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
      secrets: [ // TODO: remove - image pull will not require MI once pulling from public MCR
        {
          name: imagePullSecretName 
          keyVaultUrl: imagePullAKVSecretUrl
          identity: keyVaultUAMI.id 
        }
      ]
      registries: [ // TODO: remove - image pull will not require MI once pulling from public MCR
        {
          server: '${microsoftCR}.azurecr.io'
          username: imagePullTokenName 
          passwordSecretRef: imagePullSecretName
        }
      ]
    }
    template: {
      containers: [
        {
          image: '${microsoftCR}.azurecr.io/${imageName}:${imageTag}'
          name: 'fhirconverter-${timestamp}'
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
