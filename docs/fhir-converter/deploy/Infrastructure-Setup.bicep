param envName string
param location string = resourceGroup().location

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

// Deploy Application Insights
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
}

output containerAppEnvironmentName string = containerAppEnvironment.name
output logAnalyticsWorkspaceName string = logAnalyticsWorkspace.name
output applicationInsightsName string = applicationInsights.name
