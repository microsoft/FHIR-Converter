using System;
using System.IO;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Models.Json;
using Microsoft.Health.Fhir.Liquid.Converter.Processors;

namespace Microsoft.Health.Fhir.Liquid.Converter.ValidatePoc
{
    class Program
    {
        static void Main(string[] args)
        {
            string templateDirectory = @".\data\Templates";
            string rootTemplate = "ExamplePatient";
            // Template:
            /*
              {% validate "ExamplePatientSchema.json" -%}
              {
                  "resourceType": "{{ msg.resourceType }}",
                  "id": "{{ msg.id }}"
              }
              {% endvalidate -%}
             */

            // Schema:
            /*
             {
              "title": "Patient customized schema",
              "type": "object",
              "properties": {
                "resourceType": { "type": "string" },
                "id": { "type": "string" }
              },
              "required": [ "id" ]
            }
             */

            string sampleDataPath = @".\data\SampleData\ExamplePatient.json";
            var inputData = File.ReadAllText(sampleDataPath);
            // Input data:
            /*
            {
              "resourceType": "Patient",
              "id": "Patient-example"
            }
             */

            var processor = new JsonProcessor();
            var templateProvider = new TemplateProvider(templateDirectory, DataType.Json);

            // Convert the input data with validate tag template
            var traceInfo = new JsonTraceInfo();
            var result = processor.Convert(inputData, rootTemplate, templateProvider, traceInfo);

            foreach (var validateSchema in traceInfo.ValidateSchemas)
            {
                Console.WriteLine(validateSchema.ToString());
            }

            Console.WriteLine(result);
        }
    }
}
