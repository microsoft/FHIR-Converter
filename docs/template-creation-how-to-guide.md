# How to create a template

In this how-to guide, we will cover some of the basics around creating templates for converting HL7 v2 messages and CDA documents into FHIR bundles. The templates are an implementation of the open-source project [handlebars](https://handlebarsjs.com/). Handlebars compiles templates into JavaScript functions. The handlebars website has the most up to date information and is a great reference as you get into template building.

The HL7 v2 templates included in the release were created by generating the data from [Google spreadsheets](https://docs.google.com/spreadsheets/d/1PaFYPSSq4oplTvw_4OgOn6h2Bs_CMvCAU9CqC4tPBgk) created by the HL7 community as part of their [V2 to FHIR mapping project](https://confluence.hl7.org/display/OO/2-To-FHIR+Project) which describes the mapping of HL7 v2 version 2.8.2 into FHIR bundles version R4. The C-CDA templates included in this release were generated from customer feedback. There are top level templates that can be used to create a FHIR bundle by translating a full HL7 v2 message or CDA document. There are partial templates that are used as building blocks to create the top level template. For more details on the partial templates, see the [partial template concept section](partial-template-concept.md).

## Prerequisites

Before starting to create a template, ensure that you have deployed the GitHub code and have a local repository of the existing templates. As part of the code, we have included an optional web editor to assist with modifying and creating templates. You can choose to use this web editor or another code editor. For a high level overview of the features for the web UI, see the [web UI summary](web-ui-summary).

## Getting started

The easiest way to create a template is to start with an existing template and modify that template. However, you can also start from scratch to create your template. Right now for HL7 v2, we have top level templates for ADT_A01 (admit message), OML_021 (lab order message), ORU_R01 (observation result message), and VXU_V04 (vaccination update message). We will add more templates as the HL7 community defines them. For C-CDA, we have top level templates for Care Plan, CCD, Consultation Note, Discharge Summary, History and Physical, Operative Note, Procedure Note, Progress Note, Referral Note and Transfer Summary. We will add more templates as we receive more customer feedback. 

To get started updating/creating templates:

1. Load or paste in the sample message that you are using to validate your template. When modifying and creating templates, it’s helpful to have your sample message loaded so that you are able to see the FHIR results real time as you’re editing.

![load message](images/load-message.png)

2. Load your starting template or clear the template editing section. Rename the template and hit save so that your new template work doesn’t overwrite an existing template.

![load template](images/load-template.png)

3. As you make updates in the left-hand editor, you will see the results of those reflected on the right-hand side.

**TIP**: When editing templates, auto-completion is available for common scenarios to help you pull in commands, helper functions, and template names. To pull these in, start with {{. If you need to pull a partial template, type {{>.

4. To ensure that you have included all of the needed message parts in your FHIR bundle, any segment that is not referenced by the template will be underlined in red dots (…). Review the elements underlined in red to ensure you have accounted for all necessary segments.

![web UI](images/full-ui.png)

**NOTE**: The red dot underline functionality checks if the data is referenced in the template and does not guarantee that the specific value is directly included (or included at all) in the FHIR bundle output. Examples of this are any element used to generate the unique ID using the helper function generateUUID will count as included in the FHIR bundle output and any element referenced as part of an if statement will count as included, even if the if condition is not satisfied.

5. Once you are done editing, make sure to hit save. Your template will now be available to be called by the API for real time message translation.

### Creating Template best practices

1. Read the [handlebars documentation](https://handlebarsjs.com/guide/) for best practices on handlebars.
1. When creating FHIR resources, you will create a unique URL for each resource leveraging the helper function generateUUID. When you pass in the parameters for this helper function, you should consider what data would drive the same unique URL. For example, for a patient, you may only pass in the MRN to ensure that the unique URL For the patient resource is consistent even if other information like their address changes. 
1. In general, it is better to parse messages in the top level template and pass individual segment references to partial templates.
1. As part of handlebars, there are some built-in helper functions available to you. The handlebars documentation will have the most up to date information on this.
1. Leveraging partial templates allows you to build building blocks and utilize those building blocks across multiple templates.
1. Helper functions give you additional functionality for parsing the data. See the [helper concept guide](using-helpers-concept.md) for more details.
1. When updating templates, you may not want to save the updates immediately in your production system. To assist with this, the FHIR Converter includes basic Git functionality.

## Partial templates

Handlebars allows for nested templates, enabling you to create partial templates to be called by your top level template. If you want to edit a referenced partial template, you can double click on any underlined reference in the template editor and that template will open in a new tab in the bottom left-hand side of the UI. As you make modifications, you will see the results reflected on the right-hand side, which still shows the main top level template you loaded originally.

![edit partial template](images/partial-template-edit.png)

**NOTE**: When creating a FHIR bundle, it is often required to pull details from several partial templates. When doing this, it is possible that resources could be repeated. To avoid duplicate entries, once the message has been compiled, the FHIR Converter will merge/de-dup any created resources that have the same resourceType, ID, and versionID. An example of a template leveraging this is ADT_A01.hbs.

While you’re modifying a partial template, decide if you want to modify the partial template for all templates that reference this partial template or just the one you’re working on. If you only want to modify it for the one template you are working on, make sure to rename the template at the top before making any changes and hit save. On the main template you will have to update the template to call your newly named partial template.

**NOTE**: Scrolling between the template editor on the left and FHIR bundle output on the right-hand side only works for the original template you selected. When working with a template that you drilled into, you will need to scroll on both sides of the UI.

For more details on the released partial templates and examples, see the [Partial Templates concept](partial-template-concept.md) documentation.

## Helper Functions

As part of the handlebars functionality, helper functions exist to assist in template creation. We have released a set of starting helpers. You can see the full list of helpers [here](helper-functions-summary.md). We have also included a [using helper function](using-helpers-concept.md) conceptual guide to give some examples of how to use these in your templates.

## Summary

In this how-to-guide we reviewed some of the basics around creating templates. For more details, see some of our additional conceptual guides and resources:

- [Partial template concept](partial-template-concept.md)
- [Helper function concept](using-helpers-concept.md)
- [List of helper functions](helper-function-summary.md)
- [Web UI functionality](web-ui-summary.md)
