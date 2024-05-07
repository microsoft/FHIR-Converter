# Deployment options to setup FHIR converter service in Azure

This article details various deployment options for provisioning a FHIR converter service in Azure using the [MCR container image](https://mcr.microsoft.com/en-us/product/healthcareapis/fhir-converter/tags).

The following Azure resources will be provisioned once the deployment has completed:

* 1 x Container Apps Environment
* 1 x Azure Container App
* 1 x Log Analytics Workspace
* 1 x App Insights
* 1 x Storage Account

(**TODO** add screenshot of resources created)

## Prerequisites

(**TODO** add prereqs - Az sub, privileges, etc.)

## Deployment

### Deployment settings

The deployment options below provide a quickstart version which will setup your service with the default configuration, which is typically intended for testing or initial setup. The deployment options also allow for specifying specific configurations as needed for your service, during deployment.

Note: You are also able to update the service configuration post initial deployment, by redeploying with the updated settings.

#### Default settings

(**TODO** mention default deployment settings, i.e., auth disabled but recommend enabling, template store created, app insights created, etc.)

#### Configurable settings

To learn more about the various options available to customize your service, and to configure the settings of your FHIR converter service in Azure, refer [Configure FHIR converter service settings](configuration-settings.md).

### Deployment options

#### Option 1: Single-click Deploy to Azure via ARM template generated from Bicep Template

#### Option 2: Deploy a single Bicep file locally

#### Option 3: Execute a single PowerShell deployment script locally

### Redeployment scenarios

The following scenarios will require a redeployment of your service using anyone of the above options:

* Update container image tag - If you intend to update your service to use the latest container image tag available, a redeployment will setup your service to pull the correct image tag specified.

* Updated settings - If you choose to update any configuration, a redeployment is required for the changes to take effect. Some examples are:

  * Enable or disable authentication
  * To update authentication audience/authority.
  * Switch to default templates or custom templates usage.
  * To update the storage account to pull custom templates from.
  * Enable or disable app insights telemetry.

* Custom template collection update - If you add/update any custom template in your storage account, a redeployment is required for the service to pick up the latest template collection and use that for conversion.

### Note

* Container App supports [zero downtime deployment](https://learn.microsoft.com/en-us/azure/container-apps/revisions#zero-downtime-deployment).

  In case of updates to the service (image tag, configuration, etc.), a new container revision is created. If there are any issues in setting up the new revision, the endpoint is still available to use using the prior successfully provisioned revision. You can check the status and logs of the failing revision to debug issues with the deployment. Refer [Revisions](https://learn.microsoft.com/en-us/azure/container-apps/revisions) for more information.

## Summary

In this how-to-guide, you learned how to deploy your FHIR converter service in Azure.

Once the deployment is complete, you can use the Azure Portal to navigate to the newly created Azure Container App to see the details of your service.
The default URL to access your FHIR converter service will be the application url of your Container App of the format:`https://<SERVICE NAME>.<ENV UNIQUE ID>.<REGION NAME>.azurecontainerapps.io`.

To get started with your newly deployed FHIR converter service, refer to the following documents:

* [Configure FHIR converter service settings](configuration-settings.md)
* [Use FHIR converter APIs](use-convert-web-apis.md)
