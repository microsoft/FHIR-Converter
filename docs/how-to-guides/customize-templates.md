# Customize Liquid Templates
This how-to-guide shows you how to customize the liquid templates for the FHIR Converter service.

The default templates for the FHIR Converter service are located [here](https://github.com/microsoft/FHIR-Converter/tree/main/data/), and are a recommended starting point for creating customized templates.

## Transforming data to FHIR

### HL7v2 to FHIR
When creating a custom template for HL7v2 to FHIR, it may be simplest to start creating a template that generates a FHIR bundle with only the FHIR resource(s) needed for your use case. The default templates iterate through the HL7v2 segments and make a best effort to generate FHIR resources as applicable, but for starting a custom template for an HL7v2 ADT_A01 message, for example, you may want to focus on generating resources like Patient, Encounter, and Coverage, and then later add additional resources as needed.

This [page](https://github.com/microsoft/FHIR-Converter/blob/main/docs/HL7v2-templates.md) contains information on what segments are mapped for each HL7v2 message type when using the default templates. You may want to decide on what is necessary to include in the output FHIR bundle, and then remove any content from the default template that is not necessary for your use case. 

In addition, there are some pages dedicated to HL7v2 to FHIR reusable snippets, and HL7v2 to FHIR important points, which may be useful when creating custom templates.
 - [Snippet concepts](https://github.com/microsoft/FHIR-Converter/blob/shared/convert-api-documentation-update/docs/SnippetConcept.md)
 - [HL7v2 to FHIR important points](https://github.com/microsoft/FHIR-Converter/blob/shared/convert-api-documentation-update/docs/HL7v2-ImportantPoints.md)

### CCD to FHIR
The default templates are a good starting point for creating a custom CCD transformation template. A FHIR bundle is created by iterating over the common CCD sections in this [template](https://github.com/microsoft/FHIR-Converter/blob/main/data/Templates/Ccda/CCD.liquid). As a starting point, you can start with the CCD sections that are needed and then modify the subtemplates referenced by this root template.

### JSON to FHIR
There are some [sample templates](https://github.com/microsoft/FHIR-Converter/tree/main/data/Templates/Json) that map JSON values to a Patient resource.

## Transforming from FHIR to HL7v2
When transforming FHIR to HL7v2, the templates are written such that the input FHIR resource is transformed into a JSON representation of an HL7v2 message. When the /convertToHl7v2 API is called, the API will transform the input FHIR resource into the JSON representation of an HL7 message. But before the result is returned to the user, the JSON representation of an HL7v2 message is transformed into the normal HL7v2 format with line breaks, and is returned to the user.

To represent HL7v2 in JSON, the transformation uses an array called messageDefinition, which is an ordered array of HL7v2 message segments.

Below is an overview of how an HL7v2 message is represented in JSON. Each HL7v2 segment is an object, where the key of the object is the name of the segment. Each HL7v2 segment object has keys which represent the HL7v2 field numbers and the HL7v2 field values.
```json
{
  "messageDefinition": [
    {
      "MSH":
      {
        "fieldNumber": "data resolution code"
      },
      "PID":
      {
        "fieldNumber": "data resolution code",
        "fieldNumber": "data resolution code",
        "fieldNumber": "data resolution code"
      },
      "PV1":
      {
        "fieldNumber": "data resolution code"
      }
    }
  ]
}
```

Based on this example JSON representation of an HL7v2 message above, this is a sample transformation that iterates over Observation resources in the FHIR Bundle, and creates HL7v2 ORU^R01 messages for each observation. Note: for demonstration purposes only a few HL7v2 fields are populated.

```json
{
  "messageDefinition": [
  {
    "FHS": {
      "2": "^~\\&",
      "3": "TestSystem"
    }
  },
  {
    "BHS": {
      "2": "^~\\&",
      "3": "TestSystem"
    }
  },

  {% for entry in msg.entry %}
  {% if entry.resource.resourceType == "Observation" %}

  {% assign patient_reference =  msg | to_json_string | get_object: "$.entry[?(@resource.resourceType == 'Patient')].resource" %}

  {
    "MSH": {
      "2": "^~\\&",
      "3": "TestSystem",
      "4": "",
      "5": "TransformationAgent",
      "6": "",
      "7": "123",
      "8": "",
      "9": "ORU^R01",
      "10": "1",
      "11": "T",
      "12": "2.5",
    }
  },
  {
    "PID": {
      "3": {{ patient_reference | evaluate: "$.identifier[0].value" }}
    }
  },
  {
    "OBR": {
      "2": "{{entry.resource.identifier[0].value}}",
      "3": "{{entry.resource.code.coding[0].code}}^{{entry.resource.code.coding[0].display}}^LN",

      {% if entry.resource.status == "final" %}
        "25": "F"
      {% endif %}
    }
  },
  {
    "OBX": {
      "5": "{{entry.resource.valueQuantity.value}}",
      "6": "{{entry.resource.valueQuantity.unit}}",
      "7": "{{entry.resource.referenceRange[0].low.value}}-{{entry.resource.referenceRange[0].high.value}}",
      "8": "{{entry.resource.interpretation[0].coding[0].code}}"
    }
  },
  {% endif %}
  {% endfor %}

  {
    "BTS": {
      "1": "1"
    }
  },
  {
    "FTS": {
      "1": "1"
    }
  }]
}
```

## Liquid Filters
Since the templates are written in the liquid templating language, there are a number of [liquid filters available](https://shopify.github.io/liquid/) which can assist with modifying string values. 

There are also some useful [filters and tags](https://github.com/microsoft/FHIR-Converter/blob/b3e36d918bb67de8d3775d55b1159ee26492bde2/docs/Filters-and-Tags.md) definited in the FHIR-Converter project that can be used in templates as well.


