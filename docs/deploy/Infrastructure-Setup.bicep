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
param enableApplicationInsights bool

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

// Deploy application insights for collection application logs and metrics
module applicationInsightsDeploy 'Deploy-AppInsights.bicep' = if (enableApplicationInsights) {
  name: 'applicationInsightsDeploy'
  scope: resourceGroup()
  params: {
    location: location
    envName: envName
    logAnalyticsWorkspaceId: logAnalyticsWorkspace.id
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
output applicationInsightsName string = enableApplicationInsights ? applicationInsightsDeploy.outputs.applicationInsightsName : ''
output applicationInsightsConnectionString string = enableApplicationInsights ? applicationInsightsDeploy.outputs.applicationInsightsConnectionString : ''
output applicationInsightsUAMIClientId string = enableApplicationInsights ? applicationInsightsDeploy.outputs.applicationInsightsUAMIClientId : ''
output applicationInsightsUAMIResourceId string = enableApplicationInsights ? applicationInsightsDeploy.outputs.applicationInsightsUAMIResourceId : ''
