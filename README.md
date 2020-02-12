# FHIR Converter Overview
The FHIR Converter is an open source project that enables healthcare organizations to convert legacy data (currently HL7 v2 messages) into FHIR bundles. Converting legacy data to FHIR expands the use cases for health data and enables interoperability. Healthcare organizations investing in FHIR initiatives need to convert legacy data formats into FHIR. 

Leveraging the FHIR Converter, organizations can customize their own mapping templates based on their HL7 v2 implementation and translate them into FHIR bundles. These FHIR bundles are available for further manipulation or can be immediately persisted into the FHIR server. The FHIR Converter released to open source on Thursday March 5th, 2020. 

Right now, the FHIR Converter converts HL7 v2 messages leveraging handlebars templates into FHIR bundles. The FHIR Converter uses API Keys to authenticate access. Right now, the open-source FHIR Converter consists of the following of functionality:

* A set of starting templates to translate HL7 v2 messages into FHIR. These templates were generated based on the mappings defined by the HL7 community. As more mappings are defined, we will continue to release updated version of these templates. Current examples are ADT-A01 and ORU-R01
* A collection of APIs to convert messages real time and assist in the template management and creation. 
* A Web UI editor to modify templates and test single message conversion to FHIR bundles

For additional information, see the following documentation
* FHIR Converter Functionality Overview
* QuickStart guide to deploy locally
* QuickStart guide to deploy to Azure
* How to create templates guide

# Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
