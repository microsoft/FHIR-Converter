/*
This template deploys the following:
* Storage account (if enableTemplateStoreIntegration is set to true)
* Storage account container (if enableTemplateStoreIntegration is set to true)
* Key vault (if deployKeyVault is set to true)
* User assigned identity with Key Vault Secrets User role on the Key Vault (if deployKeyVault is set to true)
* Role assignment for the user assigned identity to access the Key Vault (if deployKeyVault is set to true)
*/

@description('Location where the storage account is deployed. For list of Azure regions where Blob Storage is available, see [Products available by region](https://azure.microsoft.com/en-us/explore/global-infrastructure/products-by-region/?products=key-vault,storage).')
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
  'chinanorth2'
  'chinanorth3'
  'eastasia'
  'eastus'
  'eastus2'
  'francecentral'
  'germanywestcentral'
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
  'westcentralus'
  'westeurope'
  'westus'
  'westus2'
  'westus3'
])
param location string

@description('If set to true, a storage account and blob container will be deployed with the specified names for storing custom templates.')
param deployTemplateStore bool

@description('Name of the storage account to be deployed.')
param templateStorageAccountName string = ''

@description('Name of the storage account container to be deployed.')
param templateStorageAccountContainerName string = ''

@description('If set to true, a key vault and user assigned managed identity will be deployed with the specified names.')
param deployKeyVault bool

@description('Name of the key vault to be deployed.')
param keyVaultName string = ''

@description('Name of the user-assigned managed identity to be deployed for accessing the key vault.')
param keyVaultUserAssignedIdentityName string = ''

resource templateStorageAccountCreated 'Microsoft.Storage/storageAccounts@2022-09-01' = if (deployTemplateStore) {
  name: deployTemplateStore ? templateStorageAccountName : 'default'
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    allowSharedKeyAccess: false
  }
}

resource templateStorageAccount 'Microsoft.Storage/storageAccounts/blobServices@2022-09-01' = if (deployTemplateStore) {
  name: 'default'
  parent: templateStorageAccountCreated
}

resource templateStorageAccountContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2022-09-01' = if (deployTemplateStore) {
  name: deployTemplateStore ? templateStorageAccountContainerName : 'default'
  parent: templateStorageAccount
}

resource keyVault 'Microsoft.KeyVault/vaults@2021-04-01-preview' = if (deployKeyVault) {
  name: deployKeyVault ? keyVaultName : 'default'
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
	name: deployKeyVault ? keyVaultUserAssignedIdentityName : 'default'
	location: location
}

var kvSecretUserRole = '4633458b-17de-408a-b874-0445c86b69e6' // Key Vault Secrets User role
resource keyVaultSecretsUserRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (deployKeyVault) {
  name: guid(resourceGroup().id, keyVaultUserAssignedIdentity.id, kvSecretUserRole)
  scope: keyVault
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', kvSecretUserRole)
    principalId: deployKeyVault ? keyVaultUserAssignedIdentity.properties.principalId : 'default'
    principalType: 'ServicePrincipal'
  }
}

output templateStorageAccountName string = deployTemplateStore ? templateStorageAccountCreated.name : ''
output templateStorageAccountContainerName string = deployTemplateStore ? templateStorageAccountContainer.name : ''
output keyVaultName string = deployKeyVault ? keyVault.name : ''
output keyVaultUAMIName string = deployKeyVault ? keyVaultUserAssignedIdentity.name : ''
