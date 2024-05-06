# Template Store Integration

This how-to-guide shows you how to configure the template store for the FHIR Converter service in Azure. This is needed to support the ability to use custom Liquid templates for your conversion requests.

The service currently supports the integration with Storage Accounts to pull custom templates hosted within the blob container.

If you are using the quickstart deployment options, your FHIR Converter service will be automatically configured to pull templates from a newly created Storage Account by specifying (**TODO** insert instructions and link).

Alternatively, to configure a pre-existing storage account, follow the steps in this document.

## Template store settings overview

The configurable template store settings are :

```json
{
  "TemplateHosting": {
    "StorageAccountConfiguration": {
      "ContainerUrl": ""
    }
  }
}
```

## Configure storage account details

### Prerequisites

To configure your template store with your FHIR Converter service, you need to have an Azure Storage Account created with a blob container.

Refer [Create a Storage Account](https://learn.microsoft.com/en-us/azure/storage/common/storage-account-create?tabs=azure-portal) for instructions to create one.

### Grant permissions to the storage account

In order for the service to be able to load the custom templates from the storage account, the Azure Container App running the service needs to be granted appropriate permissions to read from the storage account.

1. Enable managed identity on your Azure Container App.
    * Your container app can be granted either a system-assigned identity or a user-assigned identity.

Refer [Managed Identities in Azure Container Apps](https://learn.microsoft.com/en-us/azure/container-apps/managed-identity?tabs=portal%2Cdotnet) for more information.

![Convert identity](../images/convertidentity.png)

1. Assign the identity created above,[`Storage Blob Data Reader`](https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles/storage#storage-blob-data-reader) role priveleges on the storage account container being configured with the service.

Refer [Assign an Azure role for access to blob data](https://learn.microsoft.com/en-us/azure/storage/blobs/assign-azure-role-data-access?tabs=portal) for more information.

![Convert template store permissions](../images/converttemplatestorepermissions.png)

### Set the template store configuration of your FHIR Converter service

1. If you have deployed the FHIR Converter service to Azure, provide the configuration:
    * Use the deployment option (**TODO** insert instructions)

    * Alternatively, you can directly provide the configuration via environment variables in your Azure Container App running the  FHIR Converter service:
        1. **TemplateHosting__StorageAccountConfiguration__ContainerUrl** - the url to the blob container.

         Refer [Configure environment variables](https://learn.microsoft.com/en-us/azure/container-apps/environment-variables?tabs=portal) for more information.

        ![Convert template store config](../images/converttemplatestoreconfig.png)

### Verify template store health check

To verify your FHIR Converter service is setup correctly to pull the custom templates from the configured storage account, use the below health check endpoint:

GET https://\<CONTAINER APP ENDPOINT URL\>/health/check

Sample response body

```json
{
    "overallStatus": "Healthy",
    "details": [
        {
            "name": "TemplateStoreHealthCheck",
            "status": "Healthy",
            "description": "Sucessfully connected to blob template store.",
            "data": {}
        }
    ]
}
```

## Summary

In this how-to-guide, you learned how to configure the template store settings for the FHIR Converter service to be able to use custom Liquid templates for conversion.

To get started using your FHIR Converter service, refer to the following documents:

* [Customize Liquid templates](customize-templates.md)
* [Use FHIR Converter APIs](use-convert-web-apis.md)
* [Monitor FHIR Converter service](monitoring.md)
