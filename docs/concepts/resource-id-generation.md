# Resource ID generation

The default templates provided with the Converter computes Resource IDs using the input data fields. In order to preserve the generated Resource IDs, the default templates provide create **PUT requests**, instead of POST requests in the generated bundles.

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