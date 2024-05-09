@description('Location where the resources are deployed. For list of Azure regions where the below resources are available, see [Products available by region](https://azure.microsoft.com/en-us/explore/global-infrastructure/products-by-region/?products=monitor,container-apps).')
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
  'westeurope'
  'westus'
  'westus2'
  'westus3'
])
param location string

@description('Name of the container apps environment.')
param envName string

@description('If set to true, Application Insights logs and metrics collection will be enabled for the container app.')
param deployApplicationInsights bool

@description('The name of the application insights instance to be deployed for collecting logs and metrics from the container app.')
param applicationInsightsName string

@description('The name of the key vault to store secrets to be accessed by the container app.')
param keyVaultName string

// Deploy log analytics workspace
var logAnalyticsWorkspaceName = '${envName}-logsws'
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2020-03-01-preview' = {
  name: logAnalyticsWorkspaceName
  location: location
  properties: any({
    retentionInDays: 30
    features: {
      searchVersion: 1
    }
    sku: {
      name: 'PerGB2018'
    }
  })
}

// Deploy application insights for receiving application telemetry
resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = if (deployApplicationInsights) {
  name: applicationInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspace.id
    DisableLocalAuth: true
  }
}

// Get the already created key vault based on the provided name
resource keyVault 'Microsoft.KeyVault/vaults@2021-04-01-preview' existing = if (deployApplicationInsights) {
  name: keyVaultName
}

// Create secret in key vault to hold the application insights connection string
var applicationInsightsConnectionStringSecretName = '${applicationInsightsName}-conn-string'
resource applicationInsightsConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2021-06-01-preview' = if (deployApplicationInsights) {
  parent: keyVault
  name: applicationInsightsConnectionStringSecretName
  properties: {
	value: applicationInsights.properties.ConnectionString
  }
}

// Create user-assigned managed identity to authenticate with Application Insights
var applicationInsightsUAMIName = '${applicationInsightsName}-mi'
resource applicationInsightsUAMI 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = if (deployApplicationInsights) {
  name: applicationInsightsUAMIName
  location: location
}

// Grant Monitoring Metrics Publisher role to applicationInsightsUAMI on applicationInsights
var monitoringMetricPublisherRole = '3913510d-42f4-4e42-8a64-420c390055eb'

resource monitoringMetricsPublisherRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (deployApplicationInsights) {
  name: guid(resourceGroup().id, applicationInsightsUAMI.id, monitoringMetricPublisherRole)
  scope: applicationInsights
  properties: {
    principalId: applicationInsightsUAMI.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', monitoringMetricPublisherRole)
  }
}

// Deploy the container app environment
// https://github.com/Azure/azure-rest-api-specs/blob/Microsoft.App-2022-03-01/specification/app/resource-manager/Microsoft.App/preview/2022-01-01-preview/ManagedEnvironments.json
var containerAppEnvironmentName = envName
resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2023-05-01' = {
  name: containerAppEnvironmentName
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalyticsWorkspace.properties.customerId
        sharedKey: logAnalyticsWorkspace.listKeys().primarySharedKey
      }
    }
  }
}

output containerAppEnvironmentName string = containerAppEnvironment.name
output containerAppEnvironmentId string = containerAppEnvironment.id
output logAnalyticsWorkspaceName string = logAnalyticsWorkspace.name
output applicationInsightsUAMIName string = deployApplicationInsights ? applicationInsightsUAMI.name : ''
output appInsightsConnStringSecretName string = deployApplicationInsights ? applicationInsightsConnectionStringSecret.name : ''
