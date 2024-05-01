targetScope = 'subscription'

@minLength(3)
@maxLength(16)
@description('Base name used to name provisioned resources where a custom name is not provided. Should be alphanumeric, at least 3 characters and no more than 16 characters.')
param baseName string

@description('Location where the resources are deployed.')
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

@description('Name of the resource group to deploy the resources to. Will provision new resource group with given name if one does not already exist.')
param resourceGroupName string = '${baseName}-rg'

resource resourceGroup 'Microsoft.Resources/resourceGroups@2020-06-01' = {
  name: resourceGroupName
  location: location
}

// TODO: remove this param when image is public
param imagePullSecretValue string

@description('Set to true to deploy a storage account and key vault.')
param deployDependentResources bool

@description('Name of storage account containing custom templates.')
param templateStorageAccountName string = deployDependentResources ? '${baseName}templatestorage' : '' 

@description('Name of storage account container containing custom templates.')
param templateStorageAccountContainerName string = deployDependentResources ? '${baseName}templatecontainer' : ''

@description('Name of the Key Vault to store the image pull secret.')
param keyVaultName string = deployDependentResources ? '${baseName}-keyvault' : ''

@description('Name of the Managed Identity used to access Key Vault secrets.')
param keyVaultUserAssignedIdentityName string = deployDependentResources ? '${baseName}-kv-mi' : ''

module dependentResourcesSetup 'DependentResources-Setup.bicep' = if (deployDependentResources) {
  name: 'dependentResourcesSetup'
  scope: resourceGroup
  params: {
    location: location
    imagePullSecretValue: imagePullSecretValue // TODO remove when image is public
    templateStorageAccountName: templateStorageAccountName
    templateStorageAccountContainerName: templateStorageAccountContainerName
    deployTemplateStore: deployDependentResources
    keyVaultName: keyVaultName
    keyVaultUserAssignedIdentityName: keyVaultUserAssignedIdentityName
    deployKeyVault: deployDependentResources
  }
}

@description('Name of the container app environment.')
param envName string = '${baseName}-capp-env'

module convertInfrastructureDeploy 'Infrastructure-Setup.bicep' = {
  name: 'convertInfrastructureDeploy'
  scope: resourceGroup
  params: {
    location: location
    envName: envName
  }
  dependsOn: [
    dependentResourcesSetup
  ]
}

@description('Name of the container app to run the fhirconverter service.')
param appName string = '${baseName}-fhirconverter'

@description('Minimum number of replicas for the container app')
param minReplicas int = 0

@description('Maximum number of replicas for the container app')
param maxReplicas int = 30

@description('CPU limit for the container app')
param cpuLimit string = '1.0'

@description('Memory limit for the container app')
param memoryLimit string = '2Gi'

@description('Set to true to authentication requirement on the api endpoint.')
param securityEnabled bool = false

@description('Audiences for the api authentication.')
param securityAuthenticationAudiences array = []

@description('Authority for the api authentication.')
param securityAuthenticationAuthority string = ''

// TODO remove once image is public
param imagePullTokenName string = 'sdonn-test'
param imagePullPwdSecretName string = 'ConvertImagePullSecret'


module fhirConverterDeploy 'Deploy-FhirConverterService.bicep' = {
  name: 'fhirConverterDeploy'
  scope: resourceGroup
  params: {
    location: location
    appName: appName
    envName: convertInfrastructureDeploy.outputs.containerAppEnvironmentName
    minReplicas: minReplicas
    maxReplicas: maxReplicas
    cpuLimit: cpuLimit
    memoryLimit: memoryLimit
    securityEnabled: securityEnabled
    securityAuthenticationAudiences: securityAuthenticationAudiences
    securityAuthenticationAuthority: securityAuthenticationAuthority
    imagePullTokenName: imagePullTokenName
    imagePullPwdSecretName: imagePullPwdSecretName
    templateStorageAccountName: deployDependentResources ? dependentResourcesSetup.outputs.templateStorageAccountName : templateStorageAccountName
    templateStorageAccountContainerName: deployDependentResources ? dependentResourcesSetup.outputs.templateStorageAccountContainerName : templateStorageAccountContainerName
    keyVaultName: deployDependentResources ? dependentResourcesSetup.outputs.keyVaultName : keyVaultName
    keyVaultUAMIName: deployDependentResources ? dependentResourcesSetup.outputs.keyVaultUserAssignedIdentityName : keyVaultUserAssignedIdentityName
  }
  dependsOn: [
    dependentResourcesSetup
    convertInfrastructureDeploy
  ]
}

output fhirConverterApiEndpoint string = fhirConverterDeploy.outputs.containerAppFQDN
output resourceGroupName string = resourceGroup.name