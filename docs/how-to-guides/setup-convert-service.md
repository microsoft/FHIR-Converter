# Setup FHIR converter service

The FHIR converter APIs are packaged as a containerized application and made available as an [image in Microsoft Container Registry](https://mcr.microsoft.com/en-us/product/healthcareapis/fhir-converter/tags).

This how-to-guide details instructions on how to deploy the FHIR converter service in Azure using this container image and configure the service with your desired settings, to enable it for your conversion scenarios.

## 1. Deploy FHIR converter service in Azure

To deploy your FHIR converter service in Azure using the MCR artifact, see [Deployment options to setup FHIR converter service in Azure](deployment-options.md).

## 2. Configure FHIR converter service settings

To configure the settings of your FHIR converter service in Azure, see [Configure FHIR converter service settings](configuration-settings.md).

## 3. [Optional] Configure custom Liquid templates

To setup your custom Liquid templates to use with your FHIR converter service in Azure, see [Customize templates](customize-templates.md).

## 4. Verify FHIR converter service health

To check the health status of the service which indicates if the service is configured correctly and is running and available to service requests, see [Health check](use-convert-web-apis.md#health-check).

## Summary

In this how-to-guide, you learned how to setup your FHIR converter service in Azure.

Once the FHIR converter service in Azure is setup, you can use the endpoint corresponding to the application url of your Container App: `https://<SERVICE NAME>.<ENV UNIQUE ID>.<REGION NAME>.azurecontainerapps.io`.

To get started using your newly deployed FHIR converter service, refer to the following documents:

* [Use FHIR converter APIs](use-convert-web-apis.md)
* [Monitor FHIR converter service](monitoring.md)
