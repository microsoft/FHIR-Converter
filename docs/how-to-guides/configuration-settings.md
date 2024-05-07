# Configure FHIR converter service settings

This how-to-guide explains how to configure settings for the FHIR converter service, if the [default configuration](deployment-options.md#default-settings) does not suit your requirements.

## Authentication

To ensure restricted access to your FHIR converter APIs, allowing only tokens issued from within your tenant to be able to interact with the APIs, the FHIR converter service can be configured with authentication settings enabled.

Refer [Enable Authentication](enable-authentication.md) for detailed instructions on configuring your FHIR converter service with authentication settings.

## Template store integration

The FHIR converter APIs come pre-packaged with [default Liquid templates](https://github.com/microsoft/FHIR-Converter/tree/main/data/Templates) for the supported conversion scenarios.

However, to allow the ability to use custom Liquid templates for custom transformation requirements (see [Customize templates](customize-templates.md) to learn more about how to customize templates), your FHIR converter service can be configured to integrate with your template store.

Refer [Enable template store integration](enable-template-store-integration.md) for detailed instructions on configuring your FHIR converter service with your custom template store.

## Monitoring

The FHIR converter service emits custom logs and metrics to provide information on your service and invocation of the conversion APIs, that could be used for insights or troubleshooting.

These can be viewed using Azure Container App's Azure Monitor Log Analytics.
Refer [Monitoring](monitoring.md) for more information on how to view logs and metrics emitted by your FHIR converter service.

Additionally, your FHIR converter service can be configured with an Application Insights resource, which allows you to visualize the custom metrics emitted in graphical format.
If you deployed the service using the quickstart deployment options, Application Insights is deployed by default and configured with your FHIR converter service.
Alternatively, to provide your own Application Insights resource to collect the telemetry for your service, (**TODO** instructions).

Refer [Application Insights Overview](https://docs.microsoft.com/azure/azure-monitor/app/app-insights-overview) to learn how to customize Application Insights for your requirements.

## Additional configurations

### Azure Container App

The provided [deployment options](deployment-options.md) setup the FHIR converter service to run on Azure Container Apps, which is configured with basic settings intended for testing.
Azure Container Apps offers various configurable options for your app, that you can update to better suit your requirements:

* To manage hardware requirements that meet your workload requirements, refer [Workload profiles](https://learn.microsoft.com/en-us/azure/container-apps/workload-profiles-overview)
.
* To manage automatic scaling of your service, refer [Scaling & performance](https://learn.microsoft.com/en-us/azure/container-apps/scale-app?pivots=azure-cli).
* To manage ingress of your service and advanced networking configurations, refer [Networking, ingress, and network security](https://learn.microsoft.com/en-us/azure/container-apps/networking?tabs=workload-profiles-env%2Cazure-cli).

## Summary

In this how-to-guide, you learned how to configure your FHIR converter service in Azure, with your desired settings.

Once the service is setup, you can use the endpoint corresponding to the application url of your Container App running the web service.

To get started using your newly deployed FHIR converter service, refer to the following documents:

* [Customize Liquid templates](customize-templates.md)
* [Use FHIR converter APIs](use-convert-web-apis.md)
* [Monitor FHIR converter service](monitoring.md)
* [Troubleshooting guide](troubleshoot.md)
