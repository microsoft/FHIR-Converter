# Deployment options to setup FHIR converter service in Azure

This article details various deployment options for provisioning a FHIR converter service in Azure.

The following Azure resources will be provisioned once the deployment has completed:

* 1 x Container Apps Environment
* 1 x Azure Container App
* 1 x Log Analytics Workspace
* 1 x App Insights
* 1 x Storage Account

## Prerequisites

## Deployment

### Option 1: Single-click Deploy to Azure via ARM template generated from Bicep Template

### Option 2: Deploy a single Bicep file locally

Deploy the [Single Deploy Bicep Template](../deploy/FhirConverter-SingleAzureDeploy.bicep) by running the following command:

```
az deployment sub create --location <Location> --template-file FhirConverter-SingleAzureDeploy.bicep
```

Note: See [region availability](https://azure.microsoft.com/en-us/explore/global-infrastructure/products-by-region/?products=monitor,storage,container-apps) for the required resources to select a valid location for the resources to be deployed in.

You will need to provide a *serviceName* that will be used to generate a name for each of the resources provisioned. Alternatively, you can specify custom values for any of the resources created by adding parameters to the command. For example, the containerAppName can be customized to be 'containerapp-test1' by specifying a value for the containerAppName parameter in the command:
```
az deployment sub create --location westus3 --template-file FhirConverter-SingleAzureDeploy.bicep --parameters containerAppName=containerapp-test1
```

By default, the Single Deploy Bicep Template will result in a FHIR Converter deployment with the following settings:

**1. Application Insights is enabled.**

The deployment will create an Application Insights instance that will receive application logs and metrics for the FHIR Converter service. See the [Monitoring Overview](monitoring.md) for more information on how to view these logs and metrics.						
To disable Application Insights deployment for your service, or if you initially deployed your service with Application Insights and now want to disable telemetry export to Application Insights, run the deployment command with `--parameters deployApplicationInsights=false` included:

```
az deployment sub create --location <Location> --template-file FhirConverter-SingleAzureDeploy.bicep --parameters deployApplicationInsights=false
```

Note: for any time when the service is running while Application Insights is disabled, you will not have access to metrics and request logs that were captured during that time.

**2. Security settings for the API endpoints are disabled.**

It is **strongly** recommended to enable security for your FHIR Converter service. To enable security settings for the API endpoints, include `--parameters securityEnabled=true` and additional relevant security arguments in the deployment command (see more details in the [Configuration Settings](configuration-settings.md) document):

```
az deployment sub create --location <Location> --template-file FhirConverter-SingleAzureDeploy.bicep --parameters securityEnabled=true securityAuthenticationAudiences=<Audiences> securityAuthenticationAuthority=<Authority>
```

**3. Template store integration is disabled**

When template store integration is disabled, the FHIR Converter service will use the provided default templates. To use custom templates, template store integration must be enabled so that custom templates can be stored in the deployed storage account; to achieve this, include the `--parameters templateStoreEnabled=true` argument in the deployment command:

```
az deployment sub create --location <Location> --template-file FhirConverter-SingleAzureDeploy.bicep --parameters deployTemplateStore=true

```

Additional customizations are described in the [Configuration Settings](configuration-settings.md) document.

### Option 3: Execute a single PowerShell deployment script locally

## Configuration

The quickstart deployment options provision the service with default settings that are appropriate for testing.

For more configuration options for your desired settings, refer [Configure FHIR converter service settings](configuration-settings.md).

## Summary

In this how-to-guide, you learned how to deploy your FHIR converter service in Azure.

Once the deployment is complete, you can use the Azure Portal to navigate to the newly created Azure Container App to see the details of your service.
The default URL to access your FHIR converter service will be the application url of your Container App: https://*\<SERVICE NAME\>*.*\<ENV UNIQUE ID\>*.*\<REGION NAME\>*.azurecontainerapps.io.

To get started using your newly deployed FHIR converter service, refer to the following documents:

* [Configure FHIR converter service settings](configuration-settings.md)
* [Use FHIR converter APIs](use-convert-web-apis.md)
