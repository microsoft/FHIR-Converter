/*
This template deploys the following:
* Azure Log Analytics workspace
* Azure Application Insights (if deployApplicationInsights is set to true)
* A Key Vault secret containing the connection string to the Application Insights instance (if deployApplicationInsights is set to true)
* A user-assigned managed identity granted the "Monitoring Metrics Publisher" role to authenticate with Application Insights (if deployApplicationInsights is set to true)
* Azure Container Apps environment
*/

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

@description('The name of the Key Vault to store the Application Insights connection string secret.')
param keyVaultName string = 'default'

@description('If set to true, the Container Apps Environment will be linked to the Virtual Network.')
param linkToVnet bool = false

@description('The name of the Virtual Network linked to the Container Apps Environment. Only applicable if linkToVnet is set to true.')
param cAppEnvVnetName string = '${envName}-vnet'

@description('The name of the subnet in the virtual network. Only applicable if linkToVnet is set to true.')
param cAppEnvSubnetName string = 'default'

@description('IP range in CIDR notation that can be reserved for environment infrastructure IP addresses. Must be within the VNet address space, but not overlapping with any subnets within the VNet. Only applicable when linkToVnet is set to true.')
param cAppEnvVnetPlatformReservedCidr string = '10.0.16.0/24'

@description('IP address from the IP range defined by platformReservedCidr that will be reserved for the internal DNS server. Only applicable when linkToVnet is set to true.')
param cAppEnvVnetPlatformReservedDnsIP string = '10.0.16.4'

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
resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = if (deployApplicationInsights) {
  name: deployApplicationInsights ? applicationInsightsName : 'default'
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspace.id
    DisableLocalAuth: true
  }
}

resource keyVault 'Microsoft.KeyVault/vaults@2021-04-01-preview' existing = if (deployApplicationInsights) {
  name: keyVaultName
}

var applicationInsightsConnectionStringSecretName = '${applicationInsightsName}-connection-string'
resource applicationInsightsConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2021-04-01-preview' = if (deployApplicationInsights) {
  parent: keyVault
  name: applicationInsightsConnectionStringSecretName
  properties: {
    value: deployApplicationInsights ? applicationInsights.properties.ConnectionString : 'default'
  }
}

// Create user-assigned managed identity to authenticate with Application Insights
var applicationInsightsUAMIName = '${applicationInsightsName}-mi'
resource applicationInsightsUAMI 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = if (deployApplicationInsights) {
  name: applicationInsightsUAMIName
  location: location
}

// Grant Monitoring Metrics Publisher role to applicationInsightsUAMI on applicationInsights
var monitoringMetricsPublisherRoleDefinition = '3913510d-42f4-4e42-8a64-420c390055eb'
resource monitoringMetricsPublisherRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (deployApplicationInsights) {
  name: guid(resourceGroup().id, applicationInsights.id, monitoringMetricsPublisherRoleDefinition)
  scope: deployApplicationInsights ? applicationInsights : resourceGroup()
  properties: {
    principalId: deployApplicationInsights ? applicationInsightsUAMI.properties.principalId : 'default'
    principalType: 'ServicePrincipal'
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', monitoringMetricsPublisherRoleDefinition)
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
    vnetConfiguration: linkToVnet ? {
      internal: false
      infrastructureSubnetId: resourceId('Microsoft.Network/virtualNetworks/subnets', cAppEnvVnetName, cAppEnvSubnetName)
      dockerBridgeCidr: null
      platformReservedCidr: cAppEnvVnetPlatformReservedCidr
      platformReservedDnsIP: cAppEnvVnetPlatformReservedDnsIP
    } : null
  }
}

output containerAppEnvironmentName string = containerAppEnvironment.name
output containerAppEnvironmentId string = containerAppEnvironment.id
output logAnalyticsWorkspaceName string = logAnalyticsWorkspace.name
output applicationInsightsUAMIName string = deployApplicationInsights ? applicationInsightsUAMI.name : ''
output applicationInsightsConnStringSecretName string = deployApplicationInsights ? applicationInsightsConnectionStringSecret.name : ''
