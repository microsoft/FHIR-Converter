@description('Location where the resources are deployed. For list of Azure regions where application insights is available, see [Products available by region](https://azure.microsoft.com/en-us/explore/global-infrastructure/products-by-region/?products=monitor).')
@allowed([
  'australiacentral'
  'australiaeast'
  'australiasoutheast'
  'brazilsouth'
  'canadacentral'
  'canadaeast'
  'centralindia'
  'centralus'
  'chinaeast2'
  'chinanorth3'
  'eastasia'
  'eastus'
  'eastus2'
  'francecentral'
  'germanywestcentral'
  'israelcentral'
  'italynorth'
  'japaneast'
  'japanwest'
  'koreacentral'
  'northcentralus'
  'northeurope'
  'norwayeast'
  'polandcentral'
  'qatarcentral'
  'southafricanorth'
  'southcentralus'
  'southeastasia'
  'southindia'
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

@description('The resource ID of the log analytics workspace to link to the application insights instance.')
param logAnalyticsWorkspaceId string

// Deploy application insights for receiving azure monitor telemetry
var applicationInsightsName = '${envName}-ai'
resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: applicationInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspaceId
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
var monitoringMetricsPublisherRoleAssignment = guid(applicationInsightsUAMIName, applicationInsightsName)
var monitoringMetricsPublisherRoleDefinitionId = resourceId('Microsoft.Authorization/roleDefinitions', '3913510d-42f4-4e42-8a64-420c390055eb') // Monitoring Metrics Publisher role
resource monitoringMetricsPublisherRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: monitoringMetricsPublisherRoleAssignment
  scope: applicationInsights
  properties: {
    principalId: applicationInsightsUAMI.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: monitoringMetricsPublisherRoleDefinitionId
  }
}

output applicationInsightsConnectionString string = applicationInsights.properties.ConnectionString
output applicationInsightsUAMIClientId string = applicationInsightsUAMI.properties.clientId
output applicationInsightsUAMIResourceId string = applicationInsightsUAMI.id