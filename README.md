# FHIR Converter

FHIR Converter is an open source project that enables conversion of health data from legacy formats to FHIR.

The first version of the FHIR Converter released to open source on Mar 6th, 2020. It used Handlebars template language and Javascript runtime. A new converter engine was released on Nov 13, 2020 that uses Liquid templating language and .Net runtime.

Both Handlebars and Liquid converters, and corresponding templates/filters, are supported by Microsoft. We recommend using Liquid converter for better alignment with [Azure API for FHIR](https://azure.microsoft.com/en-us/services/azure-api-for-fhir/), [FHIR Server for Azure](https://github.com/microsoft/fhir-server), and [Microsoft Logic Apps](https://azure.microsoft.com/en-us/services/logic-apps/).

The following table compares the two converter engines:

|  | Handlebars Engine | Liquid Engine | 
| ----- | ----- | ----- |
| **Template language** | [Handlebars](https://handlebarsjs.com/) | [Liquid](https://shopify.github.io/liquid/) |
| **Template authoring tool** | Self-hosted web-app | [VS Code extension](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-health-fhir-converter)|
| **Supported conversions** | 1. HL7v2 to FHIR <br> 2. CCDA to FHIR | 1. HL7 v2 to FHIR <br> 2. *CCDA to FHIR** |
| **Available as** | 1. Self-deployed web service <br> (on-prem or on Azure)| 1. Command line tool <br> 2. $data-convert operation of Azure API for FHIR <br> 3. *$data-convert operation on  FHIR Server for Azure**|


**To be released soon.*

⚠ Rest of this document is about the Liquid converter. For the Handlebars converter, refer to the [Handlebars branch](https://github.com/microsoft/FHIR-Converter/tree/handlebars).

The Converter makes use of templates that define the mappings between different data formats.
The templates are written in [Liquid](https://shopify.github.io/liquid/) templating language and make use of custom [filters](docs/FiltersSummary.md), which make it easy with work with HL7 v2 messages.

The converter comes with ready to use templates for HL7v2 to FHIR conversion. These templates are based on the [spreadsheet](https://docs.google.com/spreadsheets/d/1PaFYPSSq4oplTvw_4OgOn6h2Bs_CMvCAU9CqC4tPBgk/edit#gid=0) created by the HL7 [2-To-FHIR project](https://confluence.hl7.org/display/OO/2-To-FHIR+Project). If needed, you can create new, or modify existing templates to meet your specific conversion requirements.

FHIR Converter with DotLiquid engine is integrated into the [FHIR Server for Azure](https://github.com/microsoft/fhir-server) as the [$convert-data](https://github.com/microsoft/fhir-server/blob/personal/yufei/convert-data-doc/docs/ConvertDataOperation.md) operation. In addition, it is also available as a command-line tool. The converter transforms the input data into FHIR bundles. These bundles can be persisted to a FHIR server such as the FHIR Server for Azure, or [Azure API for FHIR](https://azure.microsoft.com/en-us/services/azure-api-for-fhir/).

This project consists of the following components:

1. A command-line tool for converting data.
2. [Templates](data/Templates) for HL7 v2 to FHIR conversion.
3. [Sample data](data/SampleData) for testing purpose.

## Using the FHIR Converter

### $convert-data operation in the Azure API for FHIR

FHIR Converter is integrated into the FHIR Server for Azure to run as part of the service. Refer to the [$convert-data](https://github.com/microsoft/fhir-server/blob/personal/yufei/convert-data-doc/docs/ConvertDataOperation.md) documentation for using the FHIR converter in the FHIR Server for Azure.

### Command-line tool

The command-line tool can be used to convert a folder containing HL7 v2 messages to FHIR resources.
Here are the parameters that the tool accepts:

| Option | Name | Optionality | Default | Description |
| ----- | ----- | ----- |----- |----- |
| -d | TemplateDirectory | Required | | Root directory of templates. |
| -r | RootTemplate | Required | | Name of root template. |
| -c | InputDataContent | Optional| | Input data content. Specify OutputDataFile to get the results. |
| -f | OutputDataFile | Optional | | Output data file. |
| -i | InputDataFolder | Optional | | Input data folder. Specify OutputDataFolder to get the results.. |
| -o | OutputDataFolder | Optional | | Output data folder. |
| --version | Version | Optional | | Display version information. |
| --help | Help | Optional | | Display usage information of this tool. |

Example usage to convert HL7 v2 messages to FHIR resources in a folder:
```
>.\Microsoft.Health.Fhir.Liquid.Converter.Tool.exe -d myTemplateDirectory -e ADT_A01 -i myInputDataFolder -o myOutputDataFolder
```

### A note on Resource ID generation 

The default templates provided with the Converter computes resource ids using the fields present in the input data. In order to preserve the generated resource ids, the converter generates PUT calls, instead of POST calls.

There are a set of ID generation [templates](data/Templates/Hl7v2/ID) to help generate FHIR resource IDs from HL7 v2 messages.

An ID generation template does 3 things: 1) extract identifiers from input segment or field; 2) combine the identifers with resource type and base ID (optional) as hash seed; 3) compute hash as output ID.

The Converter introduces a concept of "base resource/base ID". Base resources are independent entities, like Patient, Organization, Device, etc, whose IDs are defined as base ID. Base IDs could be used to generate IDs for other resources that relate to them. It helps enrich the input for hash and thus reduce ID collision.
For example, a Patient ID is used as part of hash input for an AllergyIntolerance ID, as this resource is closely related with a specific patient.

Below is an example where an AllergyIntolerance ID is generated, using ID/AllergyIntolerance template, AL1 segment and patient ID as its base ID.
The syntax is `{% evaluate [id] using [template] [variables] -%}`.
```
{% evaluate allergyIntoleranceId using 'ID/AllergyIntolerance' AL1: al1Segment, baseId: patientId -%}
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
