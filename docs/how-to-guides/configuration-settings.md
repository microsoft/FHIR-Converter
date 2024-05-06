# Configure FHIR Converter service settings

This how-to-guide explains how to configure settings for the FHIR Converter service.

## Authentication

To ensure restricted access to your FHIR Converter APIs, allowing only tokens issued from within your tenant to be able to interact with the APIs, the FHIR Converter service can be configured with authentication settings enabled.

Refer [Enable Authentication](enable-authentication.md) for detailed instructions on configuring your FHIR Converter service with authentication settings.

## Template store integration

The FHIR Converter APIs come pre-packaged with [default Liquid templates](https://github.com/microsoft/FHIR-Converter/tree/main/data/Templates) for the supported conversion scenarios.

However, to allow the ability to use custom Liquid templates for custom transformation requirements, your FHIR Converter service can be configured to integrate with your template store.

Refer [Enable template store integration](enable-template-store-integration.md) for detailed instructions on configuring your FHIR Converter service with custom templates.

## Monitoring

The FHIR Converter service emits custom logs and metrics to provide information on your service and invocation of the conversion APIs, that could be used for insights or troubleshooting.

These can be viewed using Azure Container App's Azure Monitor Log Analytics.
Refer [Monitoring](monitoring.md) for more information on how to view logs and metrics emitted by your FHIR Converter service.

Additionally, your FHIR Converter service can be configured with an Application Insights resource, which allows you to visualize the custom metrics emitted in graphical format.
If you deployed the service using the quickstart deployment options, Application Insights is deployed by default and configured with your FHIR Converter service.
Alternatively, to provide your own Application Insights resource to collect the telemetry for your service, (**TODO** instructions).

Refer [Application Insights Overview](https://docs.microsoft.com/azure/azure-monitor/app/app-insights-overview) to learn how to customize Application Insights for your requirements.

## Additional configurations

### Azure Container App

The provided [deployment options](deployment-options.md) setup the FHIR Converter service to run on Azure Container Apps.

The quickstart deployment options set up the Container App with default configurations that is ideal for testing.
Azure Container Apps offers various configurable options for your app:

* To manage hardware requirements that meet your workload requirements, refer [Workload profiles](https://learn.microsoft.com/en-us/azure/container-apps/workload-profiles-overview)
.
* To manage automatic scaling of your service, refer [Scaling & performance](https://learn.microsoft.com/en-us/azure/container-apps/scale-app?pivots=azure-cli).
* To manage ingress of your service and advanced networking configurations, refer [Networking, ingress, and network security](https://learn.microsoft.com/en-us/azure/container-apps/networking?tabs=workload-profiles-env%2Cazure-cli).

## Summary

In this how-to-guide, you learned how to configure your FHIR Converter service in Azure, with your desired settings.

Once the FHIR Converter service in Azure is setup, you can use the endpoint corresponding to the application url of your Container App: https://*\<SERVICE NAME\>*.*\<ENV UNIQUE ID\>*.*\<REGION NAME\>*.azurecontainerapps.io.

To get started using your newly deployed FHIR Converter service, refer to the following documents:

* [Customize Liquid templates](customize-templates.md)
* [Use FHIR Converter APIs](use-convert-web-apis.md)
* [Monitor FHIR Converter service](monitoring.md)
