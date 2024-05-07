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

@description('For deployment tracking only. Leave blank if referencing this template directly.')
param appName string = ''

@description('For deployment tracking only. Leave blank if referencing this template directly.')
param appImageName string = ''

@description('For deployment tracking only. Leave blank if referencing this template directly.')
param appImageTag string = ''

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

// Deploy application insights for receiving azure monitor telemetry
var applicationInsightsName = '${envName}-ai'
resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: applicationInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspace.id
    DisableLocalAuth: true
  }
}

// Create user-assigned managed identity to authenticate with Application Insights
var applicationInsightsUAMIName = '${applicationInsightsName}-mi'
resource applicationInsightsUAMI 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: applicationInsightsUAMIName
  location: location
}

// Grant Monitoring Metrics Publisher role to applicationInsightsUAMI on applicationInsights
var monitoringMetricsPublisherRoleAssignmentName = guid(applicationInsightsUAMIName, applicationInsightsName)
var monitoringMetricsPublisherRoleDefinitionId = resourceId('Microsoft.Authorization/roleDefinitions', '3913510d-42f4-4e42-8a64-420c390055eb') // Monitoring Metrics Publisher role
resource monitoringMetricsPublisherRole 'Microsoft.Authorization/roleAssignments@2020-08-01-preview' = {
  name: monitoringMetricsPublisherRoleAssignmentName
  scope: applicationInsights
  properties: {
    principalId: applicationInsightsUAMI.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: monitoringMetricsPublisherRoleDefinitionId
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
  tags: {
	fhirConverterEnvName: envName
    fhirConverterAppName: appName
    fhirConverterImageName: appImageName
    fhirConverterImageVersion: appImageTag
  }
}

output containerAppEnvironmentName string = containerAppEnvironment.name
output logAnalyticsWorkspaceName string = logAnalyticsWorkspace.name
output applicationInsightsName string = applicationInsights.name
