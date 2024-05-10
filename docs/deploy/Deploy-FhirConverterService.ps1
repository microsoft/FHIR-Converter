param
(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [ValidateLength(3,9)]
    [ValidateScript({
        if ("$_" -cmatch "(^([a-z]|\d)+$)") {
            return $true
        }
        else {
            throw "Service name must be lowercase and numbers"
            return $false
        }
    })]
    [string]$serviceName,

    [Parameter(Mandatory = $true)]
    [ValidateSet(
        'australiaeast',
        'brazilsouth',
        'canadacentral',
        'canadaeast',
        'centralindia',
        'centralus',
        'chinanorth3',
        'eastasia',
        'eastus',
        'eastus2',
        'francecentral',
        'germanywestcentral',
        'japaneast',
        'koreacentral',
        'northcentralus',
        'northeurope',
        'norwayeast',
        'southafricanorth',
        'southcentralus',
        'southeastasia',
        'swedencentral',
        'switzerlandnorth',
        'uaenorth',
        'uksouth',
        'westeurope',
        'westus',
        'westus2',
        'westus3'
   )]
    [string]$location,

    [Parameter(Mandatory = $true)]
    [string]$containerAppImageTag,

    [string]$resourceGroupName = "$serviceName-rg",

    [switch]$deployTemplateStore = $false,

    [string]$templateStorageAccountName = $deployTemplateStore ? "$($serviceName)templatestorage" : "",

    [string]$templateStorageAccountContainerName = $deployTemplateStore ? "$($serviceName)templatecontainer" : "",

    [string]$containerAppEnvName = "$($serviceName)-app-env",

    [string]$containerAppName = "$($serviceName)-app",

    [int]$minReplicas = 0,

    [int]$maxReplicas = 30,

    [string]$cpuLimit = '1.0',

    [string]$memoryLimit = '2Gi',

    [switch]$securityEnabled = $false,

    [string[]]$securityAuthenticationAudiences = @(),

    [string]$securityAuthenticationAuthority = "",

    [bool]$deployApplicationInsights = $true
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# Get current Az context
try {
    Write-Host "Get current Az context..."
    az account show 
} 
catch {
    throw "Please log in with az login cmdlet before proceeding."
}

# Get current account context - User/Service Principal
$azAccountId = az account show --query user.name --output tsv
$azAccountType = az account show --query user.type --output tsv
if ($azAccountType -eq "user") {
    Write-Host "Current account context is user: $($azAccountId)."
}
elseif ($azAccountType -eq "servicePrincipal") {
    Write-Host "Current account context is service principal: $($azAccountId)."
}
else {
    Write-Host "Current context is account of type '$($azAccountType)' with id of '$($azAccountId)."
    throw "Running as an unsupported account type. Please use either a 'User' or 'Service Principal' to run this command."
}

# Validate params
if ($securityEnabled -and ((-not $securityAuthenticationAudiences) -or (-not $securityAuthenticationAuthority)))
{
    Write-Error "If securityEnabled is set, then securityAuthenticationAudiences and securityAuthenticationAuthority should be provided."
}

Write-Host "Deploying FHIR converter service..."

$timestamp = Get-Date -Format "yyyyMMddHHmmss"
$templateFile = "FhirConverter-SingleAzureDeploy.bicep"
$securityAuthenticationAudiencesArray = "['" + ($securityAuthenticationAudiences -join "','") + "']"

az deployment sub create `
    --location $location `
    --template-file $templateFile `
    --name "$($serviceName)-$($templateFile)-$($timestamp)" `
    --parameters `
        serviceName=$serviceName `
        location=$location `
        containerAppImageTag=$containerAppImageTag `
        resourceGroupName=$resourceGroupName `
        deployTemplateStore=$deployTemplateStore `
        templateStorageAccountName=$templateStorageAccountName `
        templateStorageAccountContainerName=$templateStorageAccountContainerName `
        containerAppEnvName=$containerAppEnvName `
        containerAppName=$containerAppName `
        minReplicas=$minReplicas `
        maxReplicas=$maxReplicas `
        cpuLimit=$cpuLimit `
        memoryLimit=$memoryLimit `
        securityEnabled=$securityEnabled `
        securityAuthenticationAudiences=$securityAuthenticationAudiencesArray `
        securityAuthenticationAuthority=$securityAuthenticationAuthority `
        deployApplicationInsights=$deployApplicationInsights

Write-Host "Deployment complete."