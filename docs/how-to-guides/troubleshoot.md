# FHIR converter Troubleshooting Guide

This how-to-guide provides information and steps to help you troubleshoot issues you may encounter while using the FHIR Converter.

Errors encountered during setup or when starting the web service, will be surfaced in the application logs. Errors encountered during the conversion request, will be returned in the API response body as well as surfaced in the application logs.

## Error response body

If the conversion operation fails, the API request fails with a 4xx or 5xx HTTP status code with a `x-ms-error-code` response header and the response body providing information on the error.

All error response bodies are returned in JSON format following the below structure:

```json
{
    "error": {
        "code": "<top-level error code>",
        "message": "<description of the top-level error>",
        "innerError": {
            "code": "<error code corresponding to the inner error>",
            "message": "<message describing the inner error>"
        }
    }
}
```

| Property    |         | Type    | Description                                |
|-------------|---------|---------| -------------------------------------------|
| code        |         | String  | One of a server-defined set of error codes. <br> This value matches the `x-ms-error-code` response header value. |
| message     |         | String  | A high-level description of the error.     |
| innerError  |         | Object  | An object containing more specific information than the top-level error details. |
|             | code    | String  | A more specific error code than the top-level error code. |
|             | message | String  | A detailed description of the error providing granular information on the cause of error. |

> [!Note]
>
> * The top-level error code which matches the `x-ms-error-code` response header value, are bound by the API contract; this means that a change to the error code that is returned for a given set of conditions will not change without an API version update.
> * No new top-level error codes will be added to the API without a version update.
> * All remaining fields in the response body are *not* bound to the API contract, which means that these values may change without an API version update and are intended for end-user debugging, not to be relied upon by the client code.
> * The top-level error code and message are guaranteed to be populated, but the InnerError will be empty for 500-level responses.

## Debugging with application logs

Application logs can provide detailed debugging information on the errors encountered either during the set up of the service, or during the FHIR converter API request operations.

Error logs conform to the following format:

```text
Failed conversion request with HTTP Status {HTTP status}; TOP-LEVEL-ERROR: {error code} - {top-level error message}.; INNER ERROR: Code: {inner error code}, Message: {inner error message}, Exception: {the exception that causes the Inner Error}; InnerException: {the inner exception of the Inner Error} - Code: {error code of the inner exception}, Message: {inner exception message}.
```

The top-level error information in the error log aligns with the top-level error fields in the response body. If the inner error details are present in the response body, they align with the inner error details of the log message, though the log message will likely provide more details than the response body.

See the [Monitoring](monitoring.md) document for more information on accessing these logs and sample queries that may be helpful for debugging.

## Top-Level Error Codes

This section describes potential causes for each top-level error code, root causes, and recommended resolutions.

### IncompatibleDataError

This error is observed if the template used for conversion and the input data being converted are technically valid, but the converted result produced is poorly formed.

#### __InnerError code: JsonParsingError__

 **Root Cause:** For convertToFhir requests, the output produced by the template cannot be parsed into valid JSON format. For convertToHl7v2 requests, the InputData value cannot be parsed into valid JSON format.

 **Troubleshooting:** In the case of a convertToFhir request, ensure that the template is properly formatted to generate a valid JSON object. Refer to the provided [sample templates](../../data/Templates) as valid examples or see more information on template authoring [here](customize-templates.md). In the case of a convertToHl7v2 request, ensure that the InputData string is of a format that can be correctly parsed into a JSON object.

#### __InnerError code: JsonMergingError__

 **Root Cause:** The conversion produced an output payload that can be parsed to a JSON object, but this JSON object does not have the expected structure or contains invalid data.

 **Troubleshooting:** Ensure that the template structures the data to align with the expected output format and does not contain any invalid data. Refer to the provided [sample templates](../../data/Templates) and [sample InputData](../../data/SampleData) for valid examples or see more information on template authoring [here](customize-templates.md).

### InternalServerError

An unexpected internal server error has occurred. Please see the application logs for more details and/or retry the request.

### InvalidInputData

The InputData value is not null or empty, but is invalid or cannot be parsed.

__InnerError code: InputParsingError__

> **Root Cause:** InputData parsing logic is specific to the expected format. If you are encountering this error, it is likely the result of a mismatch between the InputData and the specified InputDataFormat.

> **Troubleshooting:** Ensure that the InputDataFormat field value in the request body aligns with the format of the InputData field. For more information on authoring the request body, view the sample request bodies in the [Use FHIR converter APIs](use-convert-web-apis) document and the sample InputData [here](../../data/SampleData).

__InnerError code: InvalidInputDataContent

  **Root Cause:** The content of the InputData string passed by the user is invalid.

  **Troubleshooting:** Examples of known causes for this error for an Hl7v2 to FHIR request are missing or duplicate Hl7v2 separators, an invalid Hl7v2 message, or an invalid Hl7v2 escape character. See the logs for specific details on why the InputData value for the failed request is invalid. You can find examples of valid InputData [here](../../data/SampleData).

### InvalidRequestBody

The request body does not match the required format. See the [Use FHIR converter APIs](use-convert-web-apis) document for examples of valid request bodies.

#### __InnerError: InvalidInputDataRequestValue__

  **Root Cause:** The InputData field is required but is missing or empty.

  **Troubleshooting:** Examine the request body to ensure that the InputData field is present and is not null or empty.

#### <u>InnerError: InvalidInputDataFormat</u>

**Root Cause:** The InputDataFormat field is required but is missing or empty, or is not one of the accepted values.

**Troubleshooting:** Examine the request body to ensure that the InputDataFormat field is present and is not null or empty. If the field is present, for convertToFhir requests, this value must be one of `Hl7v2`, `Ccda`, `Json`, or `Fhir_STU3`. For convertToHl7v2 requests, this value must be `Fhir`. Note that these values are case-sensitive.

#### <u>InnerError: InvalidRootTemplate</u>

**Root Cause:** The RootTemplateName field is required but is missing or empty.

**Troubleshooting:** Examine the request body to ensure that the RootTemplateName field is present and is not null or empty.

#### <u>InnerError: InvalidRequestBody</u>

**Root Cause:** The request body failed validation for some reason other than those listed above.

**Troubleshooting:** Examine the request body to ensure that all required fields are present and that the values are correctly formatted. If the request body appears to be correct, see the application logs for more details on why the request body failed validation.

### InvalidTemplate

The template content or name is invalid.

#### <u>InnerError: InvalidFilter</u>

**Root Cause:** An error was encountered during the convert operation while using a filter referenced by the conversion template.

**Troubleshooting:** Examples of known causes of this error include usage of an invalid date-time format, an invalid hexadecimal number, and invalid time-zone handling. See the application logs for specific details of the error and examine any filters referenced by the conversion template to determine the source of the error. See more information on using filters [here](customize-templates.md). After addressing the issue in the template filter, upload the updated filter to the storage account, restart the container, and retry the request.

#### <u>InnerError: InvalidTemplateContent</u>

**Root Cause:** Some aspect of the template content is invalid.

**Troubleshooting:** See the application logs for specific details of the error and examine the conversion template (both root template and templates referenced by the root template) to determine the source of the error. See more information on template authoring [here](customize-templates.md). After addressing the issue, upload the updated template to the storage account, restart the container, and retry the request.

#### <u>InnerError: TemplateNotFound</u>

**Root Cause:** The template name or path specified in the RootTemplateName field of the request body could not be found.

**Troubleshooting:** Ensure that this value matches the value necessary to access the desired template. For default template requests, this should be only the template name. For example, to access the [ADT_A01](../../data/Templates/Hl7v2/ADT_A01.liquid) default template, the RootTemplateName field should be set to `ADT_A01`. For custom requests, this will be the name of the blob file containing the Liquid template in the storage account configured with the service. In the Azure portal, inspect your storage account to ensure that the provided `RootTemplateName` value matches the blob file name. Note that the `RootTemplateName` field should **not** contain the Storage Blob URI. See more information on writing valid request bodies to access custom templates in the [Use FHIR converter APIs](use-convert-web-apis) document.

### TemplateCollectionError

The service encountered an error while attempting to load the template collection.

#### <u>InnerError code: DependencyResourceAuthFailed</u>

**Root Cause:** If using custom templates, this is likely due to the service not having a user-assigned managed identity with the "Storage Blob Data Reader" role assignment granted by the storage account containing the template.

**Troubleshooting:** Ensure that the container app's system-assigned managed identity is granted the "Storage Blob Data Reader" role assignment by the storage account. To verify that this is configured correctly, navigate to the Storage Account, click the "Access Control (IAM)" blade, select "Role Assignments" and ensure that the system-assigned managed identity exists under the "Storage Blob Data Reader" role. If it does not, add this Role Assignment. See more information on Azure Role Assignments [here](https://learn.microsoft.com/en-US/Azure/role-based-access-control/role-assignments).

After any updates to the template store integration configuration, you will need to restart your container before retrying the request. See the [Enable Template Store Integration](enable-template-store-integration.md) document for more information on configuring the service to pull custom templates from a storage account, and consider using one of the [provided deployment options](deployment-options.md) to ensure that the service is configured correctly.

#### <u>InnerError code: DependencyResourceNotFound</u>

**Root Cause:** If using custom templates, this likely means that your service is configured with an incorrect blob container URL.

**Troubleshooting:** Ensure that your Container App is configured with the correct URL of the blob container containing your templates. To verify, navigate to your Container App. In the Overview blade, click on the "view JSON" button at the top right. In the `properties.template.containers` section, you should see your configured blob container URL as the value of the `TemplateHosting__StorageAccountConfiguration__ContainerUrl` environment variable. Compare this with the URL of your blob container; to find this, navigate to your storage account, then to the "Containers" blade under "Data storage", then select your container. Within your container page, navigate to the "Properties" blade under "Settings", and you should see the blob container URL at the top. If they do not match, update the blob container URL in your container app (see more details on configuring template store integration [here](enable-template-store-integration.md)), restart the Container, and retry the request.

#### <u>InnerError code: TemplateCollectionSizeExceedsLimit</u>

**Root Cause:** The template collection uploaded to the storage account exceeds the allowed size limit.

**Troubleshooting:** The response body and error log should indicate the maximum allowed template collection size. Remove templates from the storage account so that the collection size aligns with the limit, restart the container, and then retry the request.

### TimeoutError

The convert operation timed out.

#### <u>InnerError: CancellationError</u>
**Root Cause:** The convert operation was cancelled, likely because it took longer than the allowed time.

**Troubleshooting:** First, attempt to identify which step of the convert operation is timing out. The application logs provide the latency of each individual step of the convert operation.

The following query may be helpful in viewing the latencies of the convert operation steps:

```KQL
AppTraces
| where TimeGenerated > ago(3hours)
| where Properties contains <latency_metric_name>
| project TimeGenerated, Metric = tostring(Properties.Metric), Latency = tostring(Properties.Duration), OperationId
```

Replace the `latency_metric_name` in the query above with the metric of interest from the list below. Compare the latency to that of successful requests to identify any step(s) running longer than normal:

* *InputDeserializationDuration*: If the long-running step is `InputDeserializationDuration`, this could be the result of the `InputData` value being too large. Retry the request with a smaller value.
* *TemplateRetrievalDuration*: If the long-running step is `TemplateRetrievalDuration`, this could be the result of the template collection having too many individual templates, resulting in the search timing out. Reduce the number of templates in the collection, restart the container, and retry the request.
* *TemplateRenderDuration*: If the long-running step is `TemplateRenderDuration`, this is likely due to the template being too large. Retry the request with a smaller template.
* *PostProcessDuration*: If the long-running step is `PostProcessDuration`, this could be the result of the convert operation producing an output payload that is too large. Retry the request with an InputData value with a reduced number of elements to be convert, or with a template that produces an output with fewer elements.

#### <u>InnerError: TimeoutError</u>
**Root Cause:** This is likely a result of the template rendering step timing out.

**Troubleshooting:** If you have had previously successful requests with smaller templates, you can use the query under "InnerError: CancellationError" above with `latency_metric_name` set to `TemplateRenderDuration` to compare the latency of the Template Rendering step for your successful requests vs. your failed requests. If the template rendering timeout seems to be the likely cause, re-attempt the request with a smaller template.

## 400 - Bad request

### Top-level error code

#### ApiVersionUnspecified

**Root Cause(s):**

* The request URL is missing the `api-version` query parameter or has an empty value provided.
  * Troubleshooting:
    * Provide the `api-version` query parameter in the request URL. Refer [API versions](use-convert-web-apis.md#api-versions) for more information.

#### InvalidApiVersion

**Root Cause(s):**

* The `api-version` query parameter value specified is not supported.
  * Troubleshooting:
    * Provide the `api-version` query parameter value in the request URL with any supported api-version listed in the response header `api-supported-versions`. Refer [API versions](use-convert-web-apis.md#api-versions) for more information.

## 401 - Unauthorized

This captures errors if the client making the request is not authenticated with the service.
Ensure the steps outlined in [Azure Active Directory Authentication](enable-authentication.md) guide were followed.

**Root Cause(s):**

* Missing/invalid `Authorization` request header provided in the request.
  * Troubleshooting:
    * Provide the  `Authorization` request header containing valid bearer token. Refer [Access token](enable-authentication.md#get-access-token) for more information.

* Client ID used to get credentials was not granted API permissions required by the service.
  * Troubleshooting:
    * Add the required API permissions in the client application and then request the bearer token. Refer [Create a Client Application](enable-authentication.md#create-a-service-client-application) for more information.

* The token used was intended for a different audience. This happens if the scope used when getting the access token is incorrect.
  * Troubleshooting:
    * Ensure the scope value provided when getting the access token is in the list of audiences configured with the service. Refer [Access token](enable-authentication.md#get-access-token) for more information.

## 404 - Not Found

This error is usually received if the request is made against an invalid endpoint.

**Root Cause(s):**

* The requested API route does not exist.
  * Troubleshooting:
    * Verify the API route is correct.
      Refer [FHIR converter APIs](use-convert-web-apis.md#apis) to get the exact API names.

* The service URL does not exist.
  * Troubleshooting:
    * Verify the service URL provided in the request URL corresponds to the actual service endpoint.  Refer [Endpoint](use-convert-web-apis.md#fhir-converter-endpoint) to learn how to get this URL.

## 405 - Method Not Allowed

**Root Cause(s):**

* The service URL used for the request is an HTTP endpoint.
  * Troubleshooting:
    * Update the service URL used in the request to start with `https://`.

## 415 - Unsupported Media Type

**Root Cause(s):**

* The request body provided is not in the supported JSON format.
  * Troubleshooting:
    * Ensure the request body is provided as a JSON object in the request.

## 500 - Server Error

This indicates something went wrong with the service and encountered an unrecoverable error during the conversion due to an unexpected scenario.

The application logs will provide more details on the error encountered.

Please contact the team for further diagnosis, by creating an [issue](https://github.com/microsoft/FHIR-Converter/labels/mcr-fhir-converter).
