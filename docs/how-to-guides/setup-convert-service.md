# Setup FHIR Converter service

The FHIR Converter APIs are packaged as a containerized application and can be found as a preview image on Microsoft Container Registry.

This section details instructions on how to deploy the FHIR Converter in Azure and configure the service with your desired settings, to enable it for your conversion scenarios.

## 1. Deploy FHIR Converter service in Azure

To deploy your FHIR Converter Service in Azure, see [Deployment options to setup FHIR Converter service in Azure](deployment-options.md).

## 2. Configure FHIR Converter service settings

To configure the settings of your FHIR Converter Service in Azure, see  [Configure FHIR Converter service settings](configuration-settings.md).

## 3. [Optional] Configure custom Liquid templates

To setup your custom Liquid to use with your FHIR Converter Service in Azure, see [Customize templates](customize-templates.md).

## Summary

In this how-to-guide, you learned how to setup your FHIR Converter service in Azure.

Once the FHIR Converter service in Azure is setup, you can use the endpoint corresponding to the application url of your Container App: https://*\<SERVICE NAME\>*.*\<ENV UNIQUE ID\>*.*\<REGION NAME\>*.azurecontainerapps.io.

To get started using your newly deployed FHIR Converter service, refer to the following documents:

* [Use FHIR Converter APIs](use-convert-web-apis.md)
* [Monitor FHIR Converter service](monitoring.md)
