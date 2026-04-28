// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Processors;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.FunctionalTests
{
    public class FhirR4ConvertDataFunctionalTests
    {
        private static readonly ProcessorSettings _processorSettings = new ProcessorSettings();
        private static readonly string _schemaPath = Path.Join(Constants.TemplateDirectory, "FhirR4", "Schemas", "IdentifierSelectionCriteria.json");
        private static readonly string _identifierSelectionSchema = File.ReadAllText(_schemaPath);

        private static FhirR4Processor CreateProcessor()
        {
            return new FhirR4Processor(_processorSettings, FhirConverterLogging.CreateLogger<FhirR4Processor>());
        }

        private static ITemplateProvider CreateTemplateProvider()
        {
            var templateDirectory = Path.Join(Constants.TemplateDirectory, "FhirR4");
            return new TemplateProvider(templateDirectory, DataType.FhirR4);
        }

        private static IList<VariableDefinition> BuildTypedCriteria(string conditionsJson, string outputSystem)
        {
            var criteriaValue = $"{{ \"conditions\": [{conditionsJson}], \"outputSystem\": \"{outputSystem}\" }}";
            return new List<VariableDefinition>
            {
                new VariableDefinition
                {
                    Name = "selectionCriteria",
                    Type = VariableType.Complex,
                    Value = criteriaValue,
                    Schema = _identifierSelectionSchema,
                },
            };
        }

        // ============================================================
        // Test Case 2a: Single condition on type.coding.code
        // ============================================================
        [Fact]
        public void GivenSingleCodingCodeCondition_WhenConvert_DragonIdentifierAdded()
        {
            var processor = CreateProcessor();
            var templateProvider = CreateTemplateProvider();
            var inputContent = File.ReadAllText(Path.Join(Constants.SampleDataDirectory, "FhirR4", "PatientWithIdentifiers.json"));
            var expectedContent = File.ReadAllText(Path.Join(Constants.ExpectedDataFolder, "FhirR4", "DragonPatientMrn.json"));
            var variables = BuildTypedCriteria(
                @"{ ""path"": ""type.coding.code"", ""value"": ""MR"" }",
                "urn:oid:1.2.3.4.5.6.7.8.9-dragon-copilot");

            var result = processor.Convert(inputContent, "DragonPatientMrn", templateProvider, variables);

            var expectedObject = JObject.Parse(expectedContent);
            var actualObject = JObject.Parse(result);
            Assert.True(JToken.DeepEquals(expectedObject, actualObject));
        }

        // ============================================================
        // Test Case 2b: Compound condition on type.coding.system AND type.coding.code
        // ============================================================
        [Fact]
        public void GivenCompoundCodingConditions_WhenConvert_DragonIdentifierAdded()
        {
            var processor = CreateProcessor();
            var templateProvider = CreateTemplateProvider();
            var inputContent = File.ReadAllText(Path.Join(Constants.SampleDataDirectory, "FhirR4", "PatientWithIdentifiers.json"));
            var expectedContent = File.ReadAllText(Path.Join(Constants.ExpectedDataFolder, "FhirR4", "DragonPatientMrn.json"));
            var variables = BuildTypedCriteria(
                @"{ ""path"": ""type.coding.system"", ""value"": ""http://terminology.hl7.org/CodeSystem/v2-0203"" }, { ""path"": ""type.coding.code"", ""value"": ""MR"" }",
                "urn:oid:1.2.3.4.5.6.7.8.9-dragon-copilot");

            var result = processor.Convert(inputContent, "DragonPatientMrn", templateProvider, variables);

            var expectedObject = JObject.Parse(expectedContent);
            var actualObject = JObject.Parse(result);
            Assert.True(JToken.DeepEquals(expectedObject, actualObject));
        }

        // ============================================================
        // Test Case 2c: Condition on identifier.system
        // ============================================================
        [Fact]
        public void GivenIdentifierSystemCondition_WhenConvert_DragonIdentifierAdded()
        {
            var processor = CreateProcessor();
            var templateProvider = CreateTemplateProvider();
            var inputContent = File.ReadAllText(Path.Join(Constants.SampleDataDirectory, "FhirR4", "PatientWithIdentifiers.json"));
            var variables = BuildTypedCriteria(
                @"{ ""path"": ""system"", ""value"": ""http://hospital.example.org/mrn"" }",
                "urn:oid:1.2.3.4.5.6.7.8.9-dragon-copilot");

            var result = processor.Convert(inputContent, "DragonPatientMrn", templateProvider, variables);
            var resultObj = JObject.Parse(result);

            var identifiers = resultObj["identifier"] as JArray;
            Assert.NotNull(identifiers);
            Assert.Equal(4, identifiers.Count);

            var dragonId = identifiers[3];
            Assert.Equal("urn:oid:1.2.3.4.5.6.7.8.9-dragon-copilot", dragonId["system"]?.ToString());
            Assert.Equal("MRN-12345", dragonId["value"]?.ToString());
        }

        // ============================================================
        // Test Case 2d: Condition on type.text
        // ============================================================
        [Fact]
        public void GivenTypeTextCondition_WhenConvert_DragonIdentifierAdded()
        {
            var processor = CreateProcessor();
            var templateProvider = CreateTemplateProvider();
            var inputContent = File.ReadAllText(Path.Join(Constants.SampleDataDirectory, "FhirR4", "PatientWithIdentifiers.json"));
            var variables = BuildTypedCriteria(
                @"{ ""path"": ""type.text"", ""value"": ""Medical Record Number"" }",
                "urn:oid:1.2.3.4.5.6.7.8.9-dragon-copilot");

            var result = processor.Convert(inputContent, "DragonPatientMrn", templateProvider, variables);
            var resultObj = JObject.Parse(result);

            var identifiers = resultObj["identifier"] as JArray;
            Assert.NotNull(identifiers);
            Assert.Equal(4, identifiers.Count);

            var dragonId = identifiers[3];
            Assert.Equal("urn:oid:1.2.3.4.5.6.7.8.9-dragon-copilot", dragonId["system"]?.ToString());
            Assert.Equal("MRN-12345", dragonId["value"]?.ToString());
        }

        // ============================================================
        // Test Case 2e: Schema validation failure (missing conditions)
        // ============================================================
        [Fact]
        public void GivenSchemaValidationFailure_WhenConvert_RenderExceptionThrown()
        {
            var processor = CreateProcessor();
            var templateProvider = CreateTemplateProvider();
            var inputContent = File.ReadAllText(Path.Join(Constants.SampleDataDirectory, "FhirR4", "PatientWithIdentifiers.json"));

            var variables = new List<VariableDefinition>
            {
                new VariableDefinition
                {
                    Name = "selectionCriteria",
                    Type = VariableType.Complex,
                    Value = @"{ ""outputSystem"": ""urn:oid:test"" }",
                    Schema = _identifierSelectionSchema,
                },
            };

            var exception = Assert.Throws<RenderException>(() =>
                processor.Convert(inputContent, "DragonPatientMrn", templateProvider, variables));
            Assert.Equal(FhirConverterErrorCode.InvalidVariable, exception.FhirConverterErrorCode);
            Assert.Contains("does not conform", exception.Message);
        }

        // ============================================================
        // Test Case 2f: No match error
        // ============================================================
        [Fact]
        public void GivenNoMatchingIdentifier_WhenConvert_RenderExceptionThrown()
        {
            var processor = CreateProcessor();
            var templateProvider = CreateTemplateProvider();
            var inputContent = File.ReadAllText(Path.Join(Constants.SampleDataDirectory, "FhirR4", "PatientNoMR.json"));
            var variables = BuildTypedCriteria(
                @"{ ""path"": ""type.coding.code"", ""value"": ""MR"" }",
                "urn:oid:1.2.3.4.5.6.7.8.9-dragon-copilot");

            var exception = Assert.Throws<RenderException>(() =>
                processor.Convert(inputContent, "DragonPatientMrn", templateProvider, variables));
            Assert.Contains("No identifier matched", exception.Message);
        }

        // ============================================================
        // Test Case 2g: Multiple match error
        // ============================================================
        [Fact]
        public void GivenMultipleMatchingIdentifiers_WhenConvert_RenderExceptionThrown()
        {
            var processor = CreateProcessor();
            var templateProvider = CreateTemplateProvider();
            var inputContent = File.ReadAllText(Path.Join(Constants.SampleDataDirectory, "FhirR4", "PatientDuplicateMR.json"));
            var variables = BuildTypedCriteria(
                @"{ ""path"": ""type.coding.code"", ""value"": ""MR"" }",
                "urn:oid:1.2.3.4.5.6.7.8.9-dragon-copilot");

            var exception = Assert.Throws<RenderException>(() =>
                processor.Convert(inputContent, "DragonPatientMrn", templateProvider, variables));
            Assert.Contains("Multiple identifiers", exception.Message);
        }

        // ============================================================
        // Cross-coding false positive prevention
        // ============================================================
        [Fact]
        public void GivenMultiCodingPatient_WhenCompoundCondition_NoCrossCodingFalsePositive()
        {
            var processor = CreateProcessor();
            var templateProvider = CreateTemplateProvider();
            var inputContent = File.ReadAllText(Path.Join(Constants.SampleDataDirectory, "FhirR4", "PatientMultiCodingMR.json"));

            // Compound condition that should only match if BOTH system and code are on the SAME coding entry.
            var variables = BuildTypedCriteria(
                @"{ ""path"": ""type.coding.system"", ""value"": ""http://terminology.hl7.org/CodeSystem/v2-0203"" }, { ""path"": ""type.coding.code"", ""value"": ""MR"" }",
                "urn:oid:1.2.3.4.5.6.7.8.9-dragon-copilot");

            // Should succeed — the MR coding entry has both system and code matching.
            var result = processor.Convert(inputContent, "DragonPatientMrn", templateProvider, variables);
            var resultObj = JObject.Parse(result);

            var identifiers = resultObj["identifier"] as JArray;
            Assert.NotNull(identifiers);

            // Last identifier should be the Dragon identifier
            var dragonId = identifiers[identifiers.Count - 1];
            Assert.Equal("urn:oid:1.2.3.4.5.6.7.8.9-dragon-copilot", dragonId["system"]?.ToString());
        }

        // ============================================================
        // Factory interface with typed variables
        // ============================================================
        [Fact]
        public void GivenR4PatientViaFactoryInterface_WhenConvertWithTypedVariables_CorrectResultReturned()
        {
            var factory = new ConvertProcessorFactory(new Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory());
            var converter = factory.GetProcessor(DataType.FhirR4, ConvertDataOutputFormat.Fhir);
            var templateProvider = CreateTemplateProvider();
            var inputContent = File.ReadAllText(Path.Join(Constants.SampleDataDirectory, "FhirR4", "PatientWithIdentifiers.json"));
            var variables = BuildTypedCriteria(
                @"{ ""path"": ""type.coding.code"", ""value"": ""MR"" }",
                "urn:oid:1.2.3.4.5.6.7.8.9-dragon-copilot");

            var result = converter.Convert(inputContent, "DragonPatientMrn", templateProvider, variables);
            var resultObj = JObject.Parse(result);

            Assert.Equal("Patient", resultObj["resourceType"]?.ToString());
            var identifiers = resultObj["identifier"] as JArray;
            Assert.Equal(4, identifiers.Count);
        }
    }
}
