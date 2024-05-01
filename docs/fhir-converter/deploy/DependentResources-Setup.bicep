param location string = resourceGroup().location

// TODO: remove once image is public
param imagePullSecretValue string

// Storage Account parameters
param templateStorageAccountName string
param templateStorageAccountContainerName string
param deployTemplateStore bool

// Key Vault parameters
param keyVaultName string
param keyVaultUserAssignedIdentityName string
param deployKeyVault bool

// Deploy storage account to host convert templates if 'deployTemplateStore' is set.
resource templateStorageAccountCreated 'Microsoft.Storage/storageAccounts@2022-09-01' = if (deployTemplateStore) {
  name: templateStorageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {}
}

resource templateStorageAccount 'Microsoft.Storage/storageAccounts/blobServices@2021-06-01' = if (deployTemplateStore) {
  name: 'default'
  parent: templateStorageAccountCreated
}

resource templateStorageAccountContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-04-01' = if (deployTemplateStore) {
  name: templateStorageAccountContainerName
  parent: templateStorageAccount
}

resource keyVault 'Microsoft.KeyVault/vaults@2021-04-01-preview' = if (deployKeyVault) {
  name: keyVaultName
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    enableRbacAuthorization: true
  }
}

resource keyVaultUserAssignedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = if (deployKeyVault) {
  name: keyVaultUserAssignedIdentityName
  location: location
}

var kvSecretUserRole = '4633458b-17de-408a-b874-0445c86b69e6'
resource userKvAccessRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (deployKeyVault) {
  name: guid(resourceGroup().id, keyVaultUserAssignedIdentity.id, kvSecretUserRole)
  scope: keyVault
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', kvSecretUserRole)
    principalId: keyVaultUserAssignedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// TODO: remove once image is public
resource imagePullSecret 'Microsoft.KeyVault/vaults/secrets@2019-09-01' = {
  parent: keyVault
  name: 'ConvertImagePullSecret'
  properties: {
    value: imagePullSecretValue
    attributes: {
      enabled: true
    }
  }
  }
}

output templateStorageAccountName string = templateStorageAccountCreated.name
output templateStorageAccountContainerName string = templateStorageAccountContainer.name
output keyVaultName string = keyVault.name
output keyVaultUserAssignedIdentityName string = keyVaultUserAssignedIdentity.name
