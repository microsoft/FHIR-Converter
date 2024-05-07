@description('Location where the storage account is deployed. For list of Azure regions where Blob Storage is available, see [Products available by region](https://azure.microsoft.com/en-us/explore/global-infrastructure/products-by-region/?products=storage).')
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
