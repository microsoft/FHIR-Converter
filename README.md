# FHIR Converter

FHIR Converter is an open source project that enables conversion of health data from legacy formats to and from FHIR.  The FHIR Converter uses the [Liquid template language](https://shopify.github.io/liquid/) and the .NET runtime.

The FHIR Converter supports the following conversions: **HL7v2 to FHIR**, **C-CDA to FHIR**, **JSON to FHIR**, **FHIR STU3 to R4**, and **FHIR to HL7v2** (*Preview*).

The converter uses templates that define mappings between these different data formats. The templates are written in [Liquid](https://shopify.github.io/liquid/) templating language and make use of custom [filters](docs/Filters-and-Tags.md).  

The converter comes with a few ready-to-use templates. If needed, you can create a new template, or modify existing templates to meet your specific conversion requirements. The provided templates are based off of HL7 v2.8. Other versions may require you to make modifications to these templates on your own. See [Templates & Authoring](#templates--authoring) for specifics.

<del>FHIR Converter with DotLiquid engine transforms the input data into FHIR bundles that are persisted to a FHIR server.</del>



## What's New?
The latest iteration of the FHIR Converter makes some sigifigant changes over [previous versions](#previous-versions).

Some of the changes include:
 * Containerized API
 * Support Azure Storage for customer templates.
 * Removal of Azure Container repository dependency for custom templates.
 * *Preview* Support for FHIR to HL7v2 conversion. 

## Architecture

## Templates & Authoring

* Cover R4 as FHIR destination
* Default HL7 templates are based off of HL7v2

| Conversion | Examples | Notes
| ----- |  ----- |  ----- |
| HL7v2 to FHIR | | | 
| C-CDA to FHIR | | |
| JSON to FHIR | | |
| FHIR STU3 to R4 | | |
| FHIR to HL7v2 (*Preview*) | | |

[Template Store Integration](/docs/how-to-guides/enable-template-store-integration.md)


## Deployment

## API Documentation

Complete details on the Convert APIs and examples can be found [here](/docs/how-to-guides/use-convert-web-apis.md).


## Troubleshooting

Detailed troubleshooting options for the Convert API can be found [here](/docs/how-to-guides/troubleshoot.md).

## Previous Versions
Detailed documentation of prior Converter release is covered in the table below.

|  Version | Summary | 
| ----- |  ----- |
| [5.x Liquid](https://github.com/microsoft/FHIR-Converter/tree/e49b56f165e5607726063c681e90a28e68e39133) | Liquid engine release covers: <br> 1. HL7v2, CCDA, and JSON to FHIR transformations. <br> 2. Command Line utility. <br> 3. VS Code authoring extension. <br> 4. FHIR Service $convert integration. <br> 5. ACR template storage. |
| [3.x Handlebars](https://github.com/microsoft/FHIR-Converter/tree/handlebars) | Previous handlebars base solution.  No longer supported. See full comparision [here](https://github.com/microsoft/FHIR-Converter/tree/e49b56f165e5607726063c681e90a28e68e39133?tab=readme-ov-file#fhir-converter).

## Old Content start


## Using Templates

The command line tool supports managing different versions of templates from Azure Container Registry (ACR). You can customize templates and store them in your ACR if default templates are not sufficient for meeting conversion requirements. After [ACR authentication](docs/TemplateManagementCLI.md#authentication), you can pull templates from ACR or push templates to ACR using our command line tool.

> Note: Template version is aligned with the version of FHIR Converter.

### Command line example

Example command to push a collection of templates to ACR image from a folder:
```
>.\Microsoft.Health.Fhir.Liquid.Converter.Tool.exe push testacr.azurecr.io/templatetest:default myInputFolder
```
Example usage of pulling an image of templates in a folder:
```
>.\Microsoft.Health.Fhir.Liquid.Converter.Tool.exe pull testacr.azurecr.io/templatetest@sha256:412ea84f1bb1a9d98345efb7b427ba89616ec29ac332d543eff9a2161ca12a58 myOutputFolder
```

For more details on how to push and pull template collections, please refer to the documentation on [Template Management CLI tool](docs/TemplateManagementCLI.md).

To see the current version of templates we support, check out the complete list of [templates](data/Templates).

There are other versions released by Microsoft that are stored in a public ACR `healthplatformregistry.azurecr.io`. You can directly pull templates from ``` healthplatformregistry.azurecr.io/hl7v2defaulttemplates:<version> ``` without ACR authentication.

### HL7v2 to FHIR conversion templates

There are three documentations to note for HL7v2 to FHIR conversion. Please make sure to reference these as you use our HL7v2 default templates:

* **A complete list and explanation of each of the 57 HL7v2 to FHIR conversion templates:** [see here](docs/HL7v2-templates.md)
* **Important points to note for HL7v2 to FHIR conversion:** [see here](docs/HL7v2-ImportantPoints.md)
* **Common FHIR Validator errors/warning you might run into, and their explanations:** [see here](docs/HL7v2-FHIRValidator.md)

## Resource ID generation

The default templates provided with the Converter computes Resource IDs using the input data fields. In order to preserve the generated Resource IDs, the converter creates **PUT requests**, instead of POST requests in the generated bundles.

For **HL7v2 to FHIR conversion**, [HL7v2 DotLiquid templates](data/Templates/Hl7v2/ID) help generate FHIR resource IDs from HL7v2 messages. An ID generation template does three things: 1) extract identifiers from the input segment or field; 2) combine the identifers with resource type and base ID (optional) as hash seed; 3) compute hash as output ID.

For **C-CDA to FHIR conversion**, [C-CDA DotLiquid templates](data/Templates/Ccda/Utils) generate FHIR resource IDs in two ways: 1) [ID generation template](data/Templates/Ccda/Utils/_GenerateId.liquid) helps generate Patient ID and Practitioner ID; 2) the resource IDs for other resources are generated from the resource object directly.

For **JSON to FHIR conversion**, there is no standardized JSON input message types unlike HL7v2 messages or C-CDA documents. Therefore, instead of default templates we provide you with some sample JSON DotLiquid templates that you can use as a starting guide for your custom JSON conversion templates. You can decide how to generate the resource IDs according to your own inputs, and use our sample templates as a reference.

For **FHIR STU3 to R4 conversion**, the Resource ID from STU3 resource is copied over to corresponding R4 resource.

The Converter introduces a concept of "base resource/base ID". Base resources are independent entities, like Patient, Organization, Device, etc, whose IDs are defined as base ID. Base IDs could be used to generate IDs for other resources that relate to them. It helps enrich the input for hash and thus reduce ID collision.
For example, a Patient ID is used as part of hash input for an AllergyIntolerance ID, as this resource is closely related with a specific patient.

Below is an example where an AllergyIntolerance ID is generated, using ID/AllergyIntolerance template, AL1 segment and patient ID as its base ID.
The syntax is `{% evaluate [id] using [template] [variables] -%}`.

```liquid
{% evaluate allergyIntoleranceId using 'ID/AllergyIntolerance' AL1: al1Segment, baseId: patientId -%}
```

## Resource validation and post-processing

The output of converter depends on the templates as well as the quality and richness of input messages. Therefore, it is important that you review and validate the Converter output before using those in production.

In general, you can use [HL7 FHIR validator](https://wiki.hl7.org/Using_the_FHIR_Validator) to validate a FHIR resource. You may be able to fix some of the conversion issues by appropriately changing the templates. For other issues, you may need to have a post-processing step in your pipeline.

In some cases, due to lack of field level data in the incoming messages, the Converter may produce resources without useful information or even without ID. You can use `Hl7.Fhir.R4` .NET library to filter such resources in your pipeline. Here is the sample code for such purpose.

```C#
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

public class PostProcessor
{
    private readonly FhirJsonParser _parser = new FhirJsonParser();

    public IEnumerable<Resource> FilterResources(IEnumerable<string> fhirResources)
    {
        return fhirResources
            .Select(fhirResource => _parser.Parse<Resource>(fhirResource))
            .Where(resource => !IsEmptyResource(resource))
            .Where(resource => !IsIdAbsentResource(resource));
    }

    public bool IsEmptyResource(Resource resource)
    {
        try
        {
            var fhirResource = resource.ToJObject();
            var properties = fhirResource.Properties().Select(property => property.Name);
            // an empty resource contains no properties other than "resourceType" and "id"
            return !properties
                .Where(property => !property.Equals("resourceType"))
                .Where(property => !property.Equals("id"))
                .Any();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            // deal with the exception...
        }

        return false;
    }

    public bool IsIdAbsentResource(Resource resource)
    {
        try
        {
            return string.IsNullOrWhiteSpace(resource.Id);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            // deal with the exception...
        }
        return false;
    }
}
```

## Reference documentation

- [Filters summary](docs/Filters-and-Tags.md)
- [Snippet concept](docs/SnippetConcept.md)

## External resources

- [DotLiquid wiki](https://github.com/dotliquid/dotliquid/wiki)
- [HL7 Community 2-To-FHIR-Project](https://confluence.hl7.org/display/OO/2-To-FHIR+Project)
 
## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit [the CLA site](https://cla.opensource.microsoft.com).

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
