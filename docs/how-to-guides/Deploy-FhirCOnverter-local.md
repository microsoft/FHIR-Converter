# Fhir app container deployment for local changes

If you plan to deploy and test your local changes, you'll need to make some modifications to run the deployment using Deploy-FhirConverterService.bicep.

Since the local changes will be included in the image pushed to your private ACR, you'll need to add authentication to your ACR using a managed identity. To do this, create a user-assigned managed identity with ACR pull permissions. This managed identity will be added to your app container and used for authentication to your private ACR.

Note: We cannot use a system-assigned managed identity in this case, as it will be created during the app container's creation and cannot be used for authentication.

The following changes need to be added to the mentioned Bicep file.

```
resource identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2022-01-31-preview' = {
  name: 'appcontianerMI'
  location: location 
}

resource roleAssignmentAcr 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid('AcrPullTestUserAssigned')
  scope: containerRegistry
  properties: {
    principalId: identity.properties.principalId  
    principalType: 'ServicePrincipal'
    // acrPullDefinitionId has a value of 7f951dda-4ed3-4680-a7ca-43fe172d538d
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
  }
}

resource containerApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: containerAppName
  location: location
   identity: {
    type: 'SystemAssigned,UserAssigned'
   userAssignedIdentities: {
      '${identity.id}': {}
    }
  } 
  properties:{
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      registries: [
        {
          server: 'hlsinteropconvertinframarywus3acr.azurecr.io'
          identity: identity.id
        }
      ]  
      ....
```
Accordingly, you'll need to update the image and ACR URL with your ACR details.