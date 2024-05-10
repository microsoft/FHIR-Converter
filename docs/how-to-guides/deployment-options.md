# Deployment options to set up FHIR converter service in Azure

This article details various deployment options for provisioning a FHIR converter service in Azure using the [MCR container image](https://mcr.microsoft.com/en-us/product/healthcareapis/fhir-converter/tags).

The following Azure resources will be provisioned once the deployment has completed:

* 1 x Container Apps Environment
* 1 x Azure Container App
* 1 x Log Analytics Workspace
* 1 x App Insights
* 1 x Storage Account

(**TODO** add screenshot of resources created)

## Prerequisites

To run any of these deployment options, the following items must be set up before execution:
 
* Contributor and User Access Administrator OR Owner permissions on your Azure subscription
 
For local deployments (Options 2 and 3), the following additional steps must be performed:
 
* Install the [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli) module
 
* Log into your Azure account:
 
```PowerShell
az login 
```
 
* If you have more than one subscription, select the subscription you would like to deploy to:
 
```PowerShell
az account set --subscription <SubscriptionId>
```
 
* Clone this repo and navigate to the Bicep deployment folder:
 
```PowerShell
git clone https://github.com/microsoft/FHIR-Converter.git
cd docs/deploy
```

## Deployment

### Deployment settings

The deployment options below provide a quickstart version which will set up your service with the default configuration, which is typically intended for testing or initial setup. The deployment options also allow for specifying specific configurations as needed for your service, during deployment.

Note: You are also able to update the service configuration post initial deployment, by redeploying with the updated settings.

#### Default settings

(**TODO** mention default deployment settings, i.e., auth disabled but recommend enabling, template store created, app insights created, etc.)

#### Configurable settings

To learn more about the various options available to customize your service, and to configure the settings of your FHIR converter service in Azure, refer [Configure FHIR converter service settings](configuration-settings.md).

### Deployment options

#### Option 1: Single-click Deploy to Azure via ARM template generated from Bicep Template

![Deploy to Azure](https://aka.ms/deploytoazurebutton)

#### Option 2: Deploy a single Bicep file locally

Deploy the [Single Deploy Bicep Template](../deploy/FhirConverter-SingleAzureDeploy.bicep) by running the following command:

```
az deployment sub create --location <Location> --template-file FhirConverter-SingleAzureDeploy.bicep
```

Note: See [region availability](https://azure.microsoft.com/en-us/explore/global-infrastructure/products-by-region/?products=monitor,storage,container-apps) for the required resources to select a valid location for the resources to be deployed in.

You will need to provide a *serviceName* that will be used to generate a name for each of the resources provisioned, and the *containerAppImageTag*, which is the tag of the FHIR Converter image version to be pulled from MCR. To see available image tags, visit the [FHIR Converter MCR page](https://mcr.microsoft.com/en-us/product/healthcareapis/fhir-converter/tags).

You have the option to specify custom values for any of the resources created by adding parameters to the command. For example, the containerAppName can be customized to be 'containerapp-test1' by specifying a value for the containerAppName parameter in the command:
```
az deployment sub create --location westus3 --template-file FhirConverter-SingleAzureDeploy.bicep --parameters containerAppName=containerapp-test1
```

For help, type '?' to see a description of a parameter.

By default, the Single Deploy Bicep Template will result in a FHIR Converter deployment with the following settings:


The deployment will create an Application Insights instance that will receive application logs and metrics for the FHIR Converter service. See the [Monitoring Overview](monitoring.md) for more information on how to view these logs and metrics.						
To disable Application Insights deployment for your service, or if you initially deployed your service with Application Insights and now want to disable telemetry export to Application Insights, run the deployment command with `--parameters deployApplicationInsights=false` included:

```
az deployment sub create --location <Location> --template-file FhirConverter-SingleAzureDeploy.bicep --parameters deployApplicationInsights=false
```

Note: for any time when the service is running while Application Insights is disabled, you will not have access to metrics and request logs that were captured during that time.

**2. Security settings for the API endpoints are disabled.**

It is **strongly** recommended to enable security for your FHIR Converter service. To enable security settings for the APIs, include `--parameters securityEnabled=true` and additional relevant security arguments in the deployment command (see more details in the [Configuration Settings](configuration-settings.md) document):

```
az deployment sub create --location <Location> --template-file FhirConverter-SingleAzureDeploy.bicep --parameters securityEnabled=true securityAuthenticationAudiences=<Audiences> securityAuthenticationAuthority=<Authority>
```

**3. Template store integration is disabled**

When template store integration is disabled, a storage account will not be provisioned with the deployment and the FHIR Converter service will use the provided default templates. To use custom templates, template store integration must be enabled so that custom templates can be stored in the deployed storage account; to deploy a new storage account, include the `--parameters templateStoreEnabled=true` argument in the deployment command:
```
az deployment sub create --location <Location> --template-file FhirConverter-SingleAzureDeploy.bicep --parameters deployTemplateStore=true

```

When no custom storage account and storage container names are provided, default names will be generated using the serviceName you provide. If you want to customize the storage account and storage container names, include the --parameters templateStorageAccountName=<StorageAccountName> templateStorageAccountContainerName=<StorageContainerName> arguments in the deployment command:

```
az deployment sub create --location <Location> --template-file FhirConverter-SingleAzureDeploy.bicep --parameters deployTemplateStore=true templateStorageAccountName=<StorageAccountName> templateStorageAccountContainerName=<StorageContainerName>
```

Alternatively, if you want to enable template store integration but already have a storage account and storage container that you want to use, include the --parameters storageAccountName=<StorageAccountName> storageContainerName=<StorageContainerName> arguments in the deployment command, leaving the deployTemplateStore parameter as false:

```
az deployment sub create --location <Location> --template-file FhirConverter-SingleAzureDeploy.bicep --parameters storageAccountName=<StorageAccountName> storageContainerName=<StorageContainerName>
```

Additional customizations are described in the [Configuration Settings](configuration-settings.md) document.

#### Option 3: Execute a single PowerShell deployment script locally

Run the following command to run the PowerShell deployment script:

```PowerShell
./Deploy-FhirConverterService.ps1
```

This [PowerShell deployment script](../deploy/Deploy-FhirConverterService.ps1) sets up all necessary Azure resources for running the FHIR converter service by deploying Bicep templates via Azure CLI commands.

Refer [Parameters](#parameters) for more information on the required parameters to be provided and the default values used for optional parameters.

### Redeployment scenarios

The following scenarios will require a redeployment of your service using any one of the above options:

* Update container image tag - If you intend to update your service to use the latest container image tag available, a redeployment will set up your service to pull the correct image tag specified.

* Updated settings - If you choose to update any configuration, a redeployment is required for the changes to take effect. Some examples are:

  * Enable or disable authentication
  * To update authentication audience/authority.
  * Switch to default templates or custom templates usage.
  * To update the storage account to pull custom templates from.
  * Enable or disable app insights telemetry.

* Custom template collection update - If you add/update any custom template in your storage account, a redeployment is required for the service to pick up the latest template collection and use that for conversion.

### Additional Deployment Notes

* To view the progress of a deployment, navigate to the resource group in Azure Portal and select the 'Deployments' tab under 'Settings' in the left panel.

* Container App supports [zero downtime deployment](https://learn.microsoft.com/en-us/azure/container-apps/revisions#zero-downtime-deployment).

  In case of updates to the service (image tag, configuration, etc.), a new container revision is created. If there are any issues in setting up the new revision, the endpoint is still available to use using the prior successfully provisioned revision. You can check the status and logs of the failing revision to debug issues with the deployment. Refer [Revisions](https://learn.microsoft.com/en-us/azure/container-apps/revisions) for more information.

## Summary

In this how-to-guide, you learned how to deploy your FHIR converter service in Azure.

Once the deployment is complete, you can use the Azure Portal to navigate to the newly created Azure Container App to see the details of your service.
The default URL to access your FHIR converter service will be the application url of your Container App of the format:`https://<SERVICE NAME>.<ENV UNIQUE ID>.<REGION NAME>.azurecontainerapps.io`.

To get started with your newly deployed FHIR converter service, refer to the following documents:

* [Configure FHIR converter service settings](configuration-settings.md)
* [Use FHIR converter APIs](use-convert-web-apis.md)
