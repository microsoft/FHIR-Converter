# Set up FHIR converter service

The FHIR converter APIs are packaged as a containerized application and made available as an image in [Microsoft Container Registry](https://github.com/microsoft/containerregistry).

This how-to-guide details instructions on how to deploy the FHIR converter as a web service in Azure using [this container image](https://mcr.microsoft.com/en-us/product/healthcareapis/fhir-converter/tags) and configure the service with your desired settings, to enable it for your conversion requests.

## 1. Deploy FHIR converter service in Azure

To deploy your FHIR converter service in Azure using the MCR artifact, see [Deployment options to set up FHIR converter service in Azure](deployment-options.md).

## 2. Configure FHIR converter service settings

The quickstart version of the deployment options will set up your service with the default configuration, which is ideal for testing or initial setup.
To learn more about the various options available to customize your service to meet your needs, and to configure the settings of your FHIR converter service in Azure, see [Configure FHIR converter service settings](configuration-settings.md).

## 3. [Optional] Configure custom Liquid templates

The FHIR converter APIs come pre-packaged with [default Liquid templates](../../data/Templates) for the supported conversion scenarios.
However, to support custom transformation requirements, the APIs also have the capability to use custom templates provided for conversion.
To learn more about how to customize Liquid templates to use for your conversion requests, see [Customize templates](customize-templates.md).

These templates need to be uploaded to the template store configured with your FHIR converter service. Refer [Enable template store integration](enable-template-store-integration.md) for detailed instructions on configuring the service to use custom templates.

## 4. Verify FHIR converter service health

Once you have set up your service, you can check its health status by using the health check endpoint. The health status indicates if the service is configured correctly, is running, and is available to service requests. Refer to [Health check](use-convert-web-apis.md#health-check) for more information.

In case of any issues with the setup, refer [Troubleshooting guide](troubleshoot.md) for information on how to debug and resolve the issue.

## Summary

In this how-to-guide, you learned how to set up your FHIR converter service in Azure using the MCR container image.

Once the setup is complete, you can use the endpoint corresponding to the application url of your Container App running the web service.

To get started with your FHIR converter service, refer to the following documents:

* [Use FHIR converter APIs](use-convert-web-apis.md)
* [Monitor FHIR converter service](monitoring.md)
* [Troubleshooting guide](troubleshoot.md)
