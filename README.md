# FHIR® Converter Overview

For Healthcare IT teams and solution architects who want to integrate clinical data currently in different formats, the FHIR® Converter is an open source project that enables the conversion of legacy formatted health data to FHIR, expanding the use cases for health data and enabling interoperability.  

Right now, the FHIR Converter transforms HL7 v2 messages into FHIR bundles using templates that define the mappings between the two data formats. Additional data types will be released in the future. Leveraging the FHIR Converter, organizations can customize or create their own mapping templates based on their HL7 v2 implementation and transform them into FHIR bundles. These FHIR bundles are returned for further data manipulation or can be immediately persisted into a FHIR server, such as the Azure API for FHIR. The FHIR Converter released to open source on Thursday March 5th, 2020.

The open-source FHIR Converter consists of the following of functionality:

1. A set of starting templates, leveraging [handlebars](https://handlebarsjs.com/), to translate HL7 v2 messages into FHIR bundles. These templates were generated based on the mappings defined by the [HL7 community](https://confluence.hl7.org/display/OO/2-To-FHIR+Project). As more mappings are defined, we will continue to release updated of these templates. Current examples are ADT-A01, ORU-R01, and VXU_V04.
1. A collection of APIs to convert messages real time and assist in the template management and creation.
1. A Web UI editor to modify templates and test single message conversion to FHIR bundles.

## Documentation

For additional information, see the following documentation:

### QuickStart guides

* QuickStart guide to deploy locally
* QuickStart guide to deploy to Azure

### Template creation and management details

* [How to create templates guide](docs/template-creation-how-to-guide.md)
* [Partial template conceptual guide](docs/partial-template-concept.md)
* [Examples of using helper functions](docs/using-helpers-concept.md)

### Message conversion details

* [Converting messages conceptual guide](docs/convert-messages-concept.md)

### Additional Resources

* [Summary of available APIs](docs/api-summary.md)
* [Helper function summary](docs/helper-functions-summary.md)
* [Web UI summary](docs/web-ui-summary.md)

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

FHIR® is the registered trademark of HL7 and is used with the permission of HL7. Use of the FHIR trademark does not constitute endorsement of this product by HL7
