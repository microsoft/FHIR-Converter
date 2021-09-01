# Snippet Concept

⚠ **This document applies to the Liquid engine. Follow [this](https://github.com/microsoft/FHIR-Converter/tree/handlebars) link for the documentation of Handlebars engine.**

**Snippets** are helpful when creating templates for the FHIR Converter. They are "snippets of templates" that you can reference when writing your own templates, preventing you from having to rewrite the same code over again.
Within the FHIR converter release, there are seven types of snippets: **Resources**, **References**, **Data Type**, **Code Systems**, **Sections**, **Utils** and **Value Set**. 
The following sections will describe the purpose of each category of released snippets and give you things to consider when you are creating your own snippets.

### Data Type

Data type snippets are building blocks used to define the contents of a data field.
In most cases, the data types in HL7 v2 map to FHIR attributes as part of the FHIR resource.
The FHIR Converter includes a large number of data types as part of the release and new data type snippets will be added as they are developed by the HL7 Community or provided via customer feedback.
As you create templates, you can create your own custom data type snippets that map data fields in your implementation to FHIR.

Below is an example where a data type is included.

```
{% include 'DataType/XPN' with XPN: PID.5 -%}
```

### Resource

Resource snippets are used to create one specific FHIR resource in the FHIR bundle.
Examples of these are patient, encounter, and condition.
What you need in your resource may be message type specific or you may be able to use the same resource snippet across multiple message types.

While the resource snippet maps to a single FHIR resource type, it may pull from multiple segments in an HL7 v2 message.
For example, the released patient resource for HL7 v2 pulls from PID (Patient ID) and NK1 (Next of Kin) segments to generate the resource.

Most of the resource snippets will reference data type or code system snippets.
Resource snippet are created by parsing the HL7 v2 elements and mapping those directly to the FHIR attribute.
When parsing these elements, the filters can be helpful to ensure that you are able to pull the exact data that you need.
For more details on the filters, please see the [filters summary](#TBD) page.

Below is an example where a resource snippet is included.

```
{% include 'Resource/RelatedPerson' with NK1: nk1Segment, ID: nk1Id -%}
```

### Reference

Reference snippets allow you to create references between two related resources.
This is used to help ensure that the context of the data is carried across into the FHIR bundle.
Below is an example where the reference snippet creates a reference between the condition found in the diagnosis (from DG1 segment) and the encounter (PV1).
The reference ensures that when the condition resource is created, there is a reference to the correct encounter that the condition came from:

```json
{
    "resource":{
        "resourceType": "Encounter",
        "id":"{{ ID }}",
        "diagnosis":
        [
            {
                "condition":
                {
                    "reference":"{{ REF }}",
                },
            },
        ],
    },
},
```

When this template is called in the root template, you must specify the values for **ID** and **REF**.
Below is the example in the ADT_A01 template, where a reference snippet is included.

```
{% include 'Reference/Encounter/Diagnosis_Condition' with ID: pv1Id, REF: fullDg1Id -%}
```
## HL7 v2 Specific Snippets

The **Code Systems** snippet is unique to HL7 v2.

### Code System

Code system snippets define mappings of common codes from HL7 v2 to FHIR bundles.
An example of this is mapping "F" in HL7 v2 to "female" in a FHIR resource for gender. You can also create your own code system snippets.
These snippets will typically use if/else statements to map values from HL7 v2 to values in FHIR.
There is a sample template named *_SampleUniversalIDType.liquid* in the *CodeSystem* folder of the released templates, you can add your code system templates by referring to it.

Specifically, besides including code system snippets, there is an alternative way to convert common codes from HL7 v2 to FHIR bundles.
The Converter provides a JSON file named *CodeSystem.json* in the *CodeSystem* folder and stores all mappings in it.
You can add or modify the mappings inside this file if needed.
This way is more **recommended**, as it avoids many if/else checks and leads to a faster processing speed.

You can use it with *get_property* filter.
Below is an example using this code system mapping.

```
"gender":"{{ PID.8.Value | get_property: 'CodeSystem/Gender', 'code' }}",
```

## C-CDA Specific Snippets

The **Sections**, **Utils** and **Value Set** snippets are unique to C-CDA.

### Sections

Section snippets are used in the C-CDA to FHIR Converter. A C-CDA document is comprised of sections, each of which contain narrative text and some of which contain structured data elements. Examples of these sections include *Encounters*, *Immunization*, *Procedures* and *Vital Signs*. The section snippets map these sections to FHIR resources. Each C-CDA document template is comprised of section snippets.

### Value Set

Value Set snippets are used in the C-CDA to FHIR Converter. These snippets map the C-CDA value sets to FHIR value sets and code systems. These are implemented based on the C-CDA and FHIR specifications.
There is a sample template named *_SampleAddressUse.liquid* in the *ValueSet* folder of the released templates, you can add your value set templates by referring to it.

The same as *CodeSystem* of HL7 v2, the Converter also provides a JSON file named *ValueSet.json* in *ValueSet* folder, which stores all mappings in it. You can use this more **recommended** way with *get_property* filter as well.

The *_SystemReference.liquid* template in the *ValueSet* folder is kept unremoved since it contains complex grammar that JSON file can't express efficiently.

### Utils

Utils snippets are used in the implementation of C-CDA to FHIR we have released. These snippets provide utility functions for our implementation. For example, Utils/ResourceTypeFromSection.hbs maps a C-CDA section to a FHIR resource type based on C-CDA ID values.

## Summary

Outside of the four types of snippets outlined above, you are welcome to create your own types of snippets.
In DotLiquid, snippets always start with "\_" and end with ".liquid" extension: *_\{name\}.liquid*.
You can also visit the [DotLiquid wiki](https://github.com/dotliquid/dotliquid/wiki) for more documentation.
