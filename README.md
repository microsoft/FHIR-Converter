# FHIR Converter

FHIR Converter is an open source project that enables the conversion of health data from legacy format to FHIR.
It could be powered by two engines: [Handlebars](https://handlebarsjs.com/) and [DotLiquid](http://dotliquidmarkup.org).
This page mainly discusses the DotLiquid one.
For the Handlebars one, please refer to the [Handlebars branch](https://github.com/microsoft/FHIR-Converter/tree/handlebars).

Currently FHIR Converter with DotLiquid engine supports HL7 v2 to FHIR conversion.

The Converter makes use of templates that define the mappings between different data formats.
The templates are written in [Liquid](https://shopify.github.io/liquid/) templating language and make use of custom [filters](docs/FiltersSummary.md), which make it easy with work with HL7 v2 messages.

Templates for HL7 v2 to FHIR conversion are attached with the Converter.
HL7v2 to FHIR templates are based on the [spreadsheet](https://docs.google.com/spreadsheets/d/1PaFYPSSq4oplTvw_4OgOn6h2Bs_CMvCAU9CqC4tPBgk/edit#gid=0) created by the HL7 [2-To-FHIR project](https://confluence.hl7.org/display/OO/2-To-FHIR+Project).
If needed, you can create new, or modify existing templates to meet your specific conversion requirements.

FHIR Converter with DotLiquid engine runs as a command-line tool.
It takes raw data as input and converts it to FHIR bundles.
These bundles can be persisted to a FHIR server such as the [Azure API for FHIR](https://azure.microsoft.com/en-us/services/azure-api-for-fhir/).

FHIR Converter with DotLiquid engine consists of the following components:

1. A command-line tool for converting data.
2. [Templates](data/Templates) for HL7 v2 to FHIR conversion.
3. [Sample data](data/SampleData) for testing purpose.

## Using the FHIR Converter

HL7 v2 to FHIR conversion templates are attached with the Converter.
You can test the default conversion behavior of it by using the command-line tool.
In case the default templates do not meet your requirements, you can modify the templates by yourself.

### The command-line tool

The command-line tool can be used to convert a folder containing HL7 v2 messages to FHIR resources.
Here are the parameters that the tool accepts:

| Option | Name | Optionality | Default | Description |
| ----- | ----- | ----- |----- |----- |
| -d | TemplateDirectory | Required | | Root directory of templates. |
| -e | EntryTemplate | Required | | Name of entry template. |
| -c | InputDataContent | Optional| | Input data content. Please specify OutputDataFile to get the results. |
| -f | OutputDataFile | Optional | | Output data file. |
| -i | InputDataFolder | Optional | | Input data folder. Please specify OutputDataFolder to get the results.. |
| -o | OutputDataFolder | Optional | | Output data folder. |
| --version | Version | Optional | | Display version information. |
| --help | Help | Optional | | Display usage information of this tool. |

Example usage to convert HL7 v2 messages to FHIR resources in a folder:
```
>.\Microsoft.Health.Fhir.Liquid.Converter.Tool.exe -d myTemplateDirectory -e ADT_A01 -i myInputDataFolder -o myOutputDataFolder
```

## Reference documentation
- [Filters summary](docs/FiltersSummary.md)
- [Partial template concept](docs/SnippetConcept.md)

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
