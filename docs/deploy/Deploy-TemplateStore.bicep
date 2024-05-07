@description('Location where the storage account is deployed.')
@allowed([
  'australiaeast'
  'canadacentral'
  'centralindia'
  'centralus'
  'eastus'
  'eastus2'
  'francecentral'
  'germanywestcentral'
  'japaneast'
  'koreacentral'
  'northcentralus'
  'northeurope'
  'southcentralus'
  'southeastasia'
  'swedencentral'
  'switzerlandnorth'
  'uksouth'
  'westeurope'
  'westus2'
  'westus3'
])
param location string

@description('Name of the storage account.')
param templateStorageAccountName string

@description('Name of the storage account container.')
param templateStorageAccountContainerName string

resource templateStorageAccountCreated 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: templateStorageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {}
}

resource templateStorageAccount 'Microsoft.Storage/storageAccounts/blobServices@2021-06-01' = {
  name: 'default'
  parent: templateStorageAccountCreated
}

resource templateStorageAccountContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-04-01' = {
  name: templateStorageAccountContainerName
  parent: templateStorageAccount
}

output templateStorageAccountName string = templateStorageAccountCreated.name
output templateStorageAccountContainerName string = templateStorageAccountContainer.name
