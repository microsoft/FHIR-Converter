# FHIR Converter

FHIR Converter is an open source project that enables conversion of health data from legacy formats to FHIR.

The first version of the FHIR Converter released to open source on Mar 6th, 2020. It used Handlebars template language and Javascript runtime. A new converter engine was released on Nov 13, 2020 that uses Liquid templating language and .Net runtime.

Both Handlebars and Liquid converters, and corresponding templates/filters, are supported by Microsoft. We recommend using Liquid converter for better alignment with [Azure API for FHIR](https://azure.microsoft.com/en-us/services/azure-api-for-fhir/), [FHIR Server for Azure](https://github.com/microsoft/fhir-server), and [Microsoft Logic Apps](https://azure.microsoft.com/en-us/services/logic-apps/).

The following table compares the two converter engines:

|  | Handlebars Engine | Liquid Engine | 
| ----- | ----- | ----- |
| **Template language** | [Handlebars](https://handlebarsjs.com/) | [Liquid](https://shopify.github.io/liquid/) |
| **Template authoring tool** | Self-hosted web-app | [VS Code extension](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-health-fhir-converter)|
| **Supported conversions** | 1. HL7v2 to FHIR <br> 2. CCDA to FHIR | 1. HL7 v2 to FHIR <br> 2. *CCDA to FHIR (to be released soon)*|
| **Available as** | 1. Self-deployed web service <br> (on-prem or on Azure)| 1. Command line tool <br> 2. $convert-data operation in  FHIR Server for Azure <br> 3. $convert-data operation in Azure API for FHIR.|


⚠ Rest of this document is about the Liquid converter. For the Handlebars converter, refer to the [Handlebars branch](https://github.com/microsoft/FHIR-Converter/tree/handlebars).

The Converter makes use of templates that define the mappings between different data formats.
The templates are written in [Liquid](https://shopify.github.io/liquid/) templating language and make use of custom [filters](docs/FiltersSummary.md), which make it easy with work with HL7 v2 messages.

The converter comes with ready to use templates for HL7v2 to FHIR conversion. These templates are based on the [spreadsheet](https://docs.google.com/spreadsheets/d/1PaFYPSSq4oplTvw_4OgOn6h2Bs_CMvCAU9CqC4tPBgk/edit#gid=0) created by the HL7 [2-To-FHIR project](https://confluence.hl7.org/display/OO/2-To-FHIR+Project). If needed, you can create new, or modify existing templates to meet your specific conversion requirements.

FHIR Converter with DotLiquid engine is integrated into the [Azure API for FHIR](https://azure.microsoft.com/en-us/services/azure-api-for-fhir/), and [FHIR Server for Azure](https://github.com/microsoft/fhir-server) as the [$convert-data](https://docs.microsoft.com/en-us/azure/healthcare-apis/convert-data) operation. In addition, it is also available as a command-line tool. The converter transforms the input data into FHIR bundles that can be persisted to a FHIR server.

This project consists of the following components:

1. A command-line tool) for converting data and managing templates.
2. [Templates](data/Templates) for HL7 v2 to FHIR conversion.
3. [Sample data](data/SampleData) for testing purpose.

## Using the FHIR Converter

### $convert-data operation in the Azure API for FHIR

FHIR Converter is integrated into Azure API for FHIR, and FHIR Server for Azure to run as part of the service. Refer to the [$convert-data](https://docs.microsoft.com/en-us/azure/healthcare-apis/convert-data) documentation for using the FHIR converter in the FHIR Server for Azure.

### Command-line tool

**Convert Data**

The command-line tool can be used to convert a folder containing HL7 v2 messages to FHIR resources.
Here are the parameters that the tool accepts:

| Option | Name | Optionality | Default | Description |
| ----- | ----- | ----- |----- |----- |
| -d | TemplateDirectory | Required | | Root directory of templates. |
| -r | RootTemplate | Required | | Name of root template. Valid values are ADT_A01, OML_O21, ORU_R01, VXU_V04. |
| -c | InputDataContent | Optional| | Input data content. Specify OutputDataFile to get the results. |
| -f | OutputDataFile | Optional | | Output data file. |
| -i | InputDataFolder | Optional | | Input data folder. Specify OutputDataFolder to get the results. |
| -o | OutputDataFolder | Optional | | Output data folder. |
| -t | IsTraceInfo | Optional | | Provide trace information in the output if "-t" is set. |
| --version | Version | Optional | | Display version information. |
| --help | Help | Optional | | Display usage information of this tool. |

Example usage to convert HL7 v2 messages to FHIR resources in a folder:
```
>.\Microsoft.Health.Fhir.Liquid.Converter.Tool.exe convert -d myTemplateDirectory -r ADT_A01 -i myInputDataFolder -o myOutputDataFolder
```

**Manage Templates**

The command-line tool also supports managing different versions of templates from Azure Container Registry (ACR). Users can customize templates and store them on ACR if default templates can not meet requirements. After [ACR authentication](docs/TemplateManagementCLI.md), users can pull and push templates from/to a remote ACR through our tool.

Example command to push a collection of templates to ACR image from a folder:
```
>.\Microsoft.Health.Fhir.Liquid.Converter.Tool.exe push testacr.azurecr.io/templatetest:default myInputFolder
```
Example usage of pulling an image of templates in a folder:

```
>.\Microsoft.Health.Fhir.Liquid.Converter.Tool.exe pull testacr.azurecr.io/templatetest@sha256:412ea84f1bb1a9d98345efb7b427ba89616ec29ac332d543eff9a2161ca12a58 myOutputFolder

```
More details of usage are given in [Template Management CLI tool](docs/TemplateManagementCLI.md).

Besides current version of [templates](data/Templates) given in our project, other versions that released by Microsoft are stored in a public ACR: healthplatformregistry.azurecr.io, users can directly pull templates from ``` healthplatformregistry.azurecr.io/hl7v2defaulttemplates:<version> ``` without authentication.
>Note!: Template version is aligned with the version of FHIR Converter. 

## Usage Notes

### Resource ID generation

The default templates provided with the Converter computes resource ids using the fields present in the input data. In order to preserve the generated resource ids, the converter created PUT requests, instead of POST requests in the generated bundles.

A set of [templates](data/Templates/Hl7v2/ID) help generate FHIR resource IDs from HL7 v2 messages. An ID generation template does 3 things: 1) extract identifiers from input segment or field; 2) combine the identifers with resource type and base ID (optional) as hash seed; 3) compute hash as output ID.

The Converter introduces a concept of "base resource/base ID". Base resources are independent entities, like Patient, Organization, Device, etc, whose IDs are defined as base ID. Base IDs could be used to generate IDs for other resources that relate to them. It helps enrich the input for hash and thus reduce ID collision.
For example, a Patient ID is used as part of hash input for an AllergyIntolerance ID, as this resource is closely related with a specific patient.

Below is an example where an AllergyIntolerance ID is generated, using ID/AllergyIntolerance template, AL1 segment and patient ID as its base ID.
The syntax is `{% evaluate [id] using [template] [variables] -%}`.

```liquid
{% evaluate allergyIntoleranceId using 'ID/AllergyIntolerance' AL1: al1Segment, baseId: patientId -%}
```

### Resource validation and post-processing

Real world HL7 messages vary in richness and level of conformance with the spec. The output of converter depends on the templates as well as the quality and richness of input messages. Therefore, it is important that you review and validate the Converter output before using those in production.

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
- [Filters summary](docs/FiltersSummary.md)
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
