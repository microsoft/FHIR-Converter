# Deployment options to setup FHIR Converter service in Azure

This article details various deployment options for provisioning a FHIR Converter service in Azure.

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

### Option 3: Execute a single PowerShell deployment script locally

## Configuration

The quickstart deployment options provision the service with default settings that are appropriate for testing.

For more configuration options for your desired settings, refer [Configure FHIR Converter service settings](configuration-settings.md).

## Summary

In this how-to-guide, you learned how to deploy your FHIR Converter service in Azure.

Once the deployment is complete, you can use the Azure Portal to navigate to the newly created Azure Container App to see the details of your service.
The default URL to access your FHIR Converter service will be the application url of your Container App: https://*\<SERVICE NAME\>*.*\<ENV UNIQUE ID\>*.*\<REGION NAME\>*.azurecontainerapps.io.

To get started using your newly deployed FHIR Converter service, refer to the following documents:

* [Configure FHIR Converter service settings](configuration-settings.md)
* [Use FHIR Converter APIs](use-convert-web-apis.md)
