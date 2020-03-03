# HL7 v2 Message to FHIR Bundle Conversion

The power of the FHIR Converter lies in its ability to convert 100s of messages per second real-time. Once you have modified the templates to meet your need, you can call the API as part of your workflow. In the sections below we have an overview of the API and the output you get from the conversion.

## APIs

To convert your messages leveraging the API, there are two different POST calls you can make depending on how you want to call your template. You can either convert by passing the entire template's content or you can call a template from storage by name.

| Function | Syntax                    | Details                                         |
|----------|---------------------------|-------------------------------------------------|
|POST      |/api/convert/hl7           |Converts an HL7 v2 message to FHIR using a template directly from the end point|
|POST      |/api/convert/hl7/{template}|Converts an HL7 v2 message to FHIR using a template from storage|

## Conversion output

Each time a message is converted using the APIs, there are three pieces of information returned:

| Section | Details | Use Case |
|-|-|-|
| **fhirResource** | The FHIR bundle for the converted HL7 v2 message | The fhirResource is the FHIR bundle that you can do further manipulation on or persist directly in a FHIR server
| **unusedSegments** | A list of segments that the template didn’t look at that were present in the message. In the Web UI, these are the segments that were underlined in red dots (...) | You can use the details returned in this section to see if there were any required segments that weren't processed. In this way, you can ensure that you don't store a FHIR bundle that is missing key information from the HL7 v2 message |
| **invalidAccess** | A list of segments the template tried to access that didn’t exist in the incoming HL7 v2 message | The invalidAccess section allows you to do post-processing on the FHIR bundle to ensure that the incoming HL7 v2 messages that was processed didn't have any major issues. For example, you may want to reject or investigate any message that is missing the Patient Identifier |

## Examples

Below are examples of each piece of the conversion output.

### fhirResource example

```JSON
{
  "fhirResource": {
    "resourceType": "Bundle",
    "$2$": "transaction",
    "entry": [
      {
        "fullUrl": "urn:uuid:4eb5c7ca-9f74-3032-981c-c1954b471dbe",
        "resource": {
          "resourceType": "Patient",
          "id": "4eb5c7ca-9f74-3032-981c-c1954b471dbe",
          "identifier": [
            {
              "value": "10006579"
            },
            {
              "value": "123121234"
            }
          ],
          "name": [
            {
              "family": "DUCK",
              "given": [
                "DONALD",
                "D"
              ]
            }
          ],
          "birthDate": "1924-10-10",
          "gender": "male",
          "address": [
            {
              "line": [
                "111 DUCK ST"
              ],
              "city": "FOWL",
              "state": "CA",
              "postalCode": "999990000",
              "type": "postal"
            },
            {
              "district": "1"
            }
          ],
          "telecom": [
            {
              "value": "8885551212",
              "use": "home"
            }
          ],
          "communication": [
            {
              "preferred": "true"
            }
          ]
        },
        "request": {
          "method": "POST",
          "url": "Patient"
        }
      },
    ]
  }
  ```

### unusedSegments

  ```JSON
  "unusedSegments": [
    {
      "type": "IN1",
      "line": 7,
      "field": [
        {
          "index": 1,
          "component": [
            "1"
          ]
        },
        {
          "index": 2,
          "component": [
            "MEDICARE"
          ]
        },
        {
          "index": 3,
          "component": [
            "3"
          ]
        },
        {
          "index": 12,
          "component": [
            "19891001"
          ]
        },
        {
          "index": 15,
          "component": [
            "4"
          ]
        },
        {
          "index": 16,
          "component": [
            "DUCK",
            "DONALD",
            "D"
          ]
        },
      ]
    },
  ]
  ```

### invalidAccess

```json
  "invalidAccess": [
    {
      "type": "PID",
      "line": 2,
      "field": [
        {
          "index": 40
        },
        {
          "index": 33
        }
      ]
    },
    {
      "type": "NK1",
      "line": 3,
      "field": [
        {
          "index": 32
        },
        {
          "index": 31
        },
        {
          "index": 40
        },
        {
          "index": 41
        },
        {
          "index": 30
        }
      ]
    },
  ]
}
```
