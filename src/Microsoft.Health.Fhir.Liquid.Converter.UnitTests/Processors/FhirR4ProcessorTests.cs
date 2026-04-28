// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Processors;
using Microsoft.Health.Fhir.Liquid.Converter.Utilities;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.Processors
{
    public class FhirR4ProcessorTests
    {
        private static readonly string _fhirR4TestData;
        private static readonly string _fhirR4DuplicateMRData;
        private static readonly string _fhirR4NoMRData;
        private static readonly ProcessorSettings _processorSettings;
        private static readonly FhirR4Processor _fhirR4Processor;
        private static readonly ITemplateProvider _templateProvider;

        private static readonly string _fhirR4MultiCodingMRData;

        private static readonly string _identifierSelectionSchema;

        static FhirR4ProcessorTests()
        {
            _fhirR4TestData = File.ReadAllText(Path.Join(TestConstants.SampleDataDirectory, "FhirR4", "PatientWithIdentifiers.json"));
            _fhirR4DuplicateMRData = File.ReadAllText(Path.Join(TestConstants.SampleDataDirectory, "FhirR4", "PatientDuplicateMR.json"));
            _fhirR4NoMRData = File.ReadAllText(Path.Join(TestConstants.SampleDataDirectory, "FhirR4", "PatientNoMR.json"));
            _fhirR4MultiCodingMRData = File.ReadAllText(Path.Join(TestConstants.SampleDataDirectory, "FhirR4", "PatientMultiCodingMR.json"));
            _processorSettings = new ProcessorSettings();
            _fhirR4Processor = new FhirR4Processor(_processorSettings, FhirConverterLogging.CreateLogger<FhirR4Processor>());
            _templateProvider = new TemplateProvider(TestConstants.FhirR4TemplateDirectory, DataType.FhirR4);
            _identifierSelectionSchema = File.ReadAllText(Path.Join(TestConstants.FhirR4TemplateDirectory, "Schemas", "IdentifierSelectionCriteria.json"));
        }

        private static Dictionary<string, string> GetDragonVariables()
        {
            return new Dictionary<string, string>
            {
                { "matchCode", "MR" },
                { "dragonSystem", "urn:oid:1.2.3.4.5.6.7.8.9-dragon-copilot" },
            };
        }

        private static IList<VariableDefinition> GetDragonTypedVariables(string codingCode = "MR", string outputSystem = "urn:oid:1.2.3.4.5.6.7.8.9-dragon-copilot")
        {
            var criteriaValue = $"{{ \"conditions\": [{{ \"path\": \"type.coding.code\", \"value\": \"{codingCode}\" }}], \"outputSystem\": \"{outputSystem}\" }}";
            var schemaPath = Path.Join(TestConstants.FhirR4TemplateDirectory, "Schemas", "IdentifierSelectionCriteria.json");
            var schema = File.ReadAllText(schemaPath);
            return new List<VariableDefinition>
            {
                new VariableDefinition
                {
                    Name = "selectionCriteria",
                    Type = VariableType.Complex,
                    Value = criteriaValue,
                    Schema = schema,
                },
            };
        }

        [Fact]
        public void GivenVariables_WhenConvertViaInterface_VariablesAreAccessible()
        {
            IFhirConverter converter = _fhirR4Processor;
            var variables = GetDragonTypedVariables();
            var result = converter.Convert(_fhirR4TestData, "DragonPatientMrn", _templateProvider, variables);
            var resultObj = JObject.Parse(result);

            Assert.Equal("Patient", resultObj["resourceType"]?.ToString());
            var identifiers = resultObj["identifier"] as JArray;
            Assert.NotNull(identifiers);
            Assert.Equal(4, identifiers.Count);

            // Verify the Dragon identifier was added
            var dragonId = identifiers[3];
            Assert.Equal("urn:oid:1.2.3.4.5.6.7.8.9-dragon-copilot", dragonId["system"]?.ToString());
            Assert.Equal("MRN-12345", dragonId["value"]?.ToString());
        }

        [Fact]
        public void GivenVariables_WhenConvertWithCancellationToken_CorrectResultReturned()
        {
            IFhirConverter converter = _fhirR4Processor;
            var variables = GetDragonTypedVariables();
            var result = converter.Convert(_fhirR4TestData, "DragonPatientMrn", _templateProvider, variables, CancellationToken.None);
            var resultObj = JObject.Parse(result);

            var identifiers = resultObj["identifier"] as JArray;
            Assert.NotNull(identifiers);
            Assert.Equal(4, identifiers.Count);
        }

        [Fact]
        public void GivenVariables_WhenConvertDirect_VariablesAreAccessible()
        {
            var variables = GetDragonTypedVariables();
            var result = _fhirR4Processor.Convert(_fhirR4TestData, "DragonPatientMrn", _templateProvider, variables);
            var resultObj = JObject.Parse(result);

            Assert.Equal("Patient", resultObj["resourceType"]?.ToString());
            var identifiers = resultObj["identifier"] as JArray;
            Assert.NotNull(identifiers);
            Assert.Equal(4, identifiers.Count);
        }

        [Fact]
        public void GivenDragonMrnTemplate_WhenConvert_AllOriginalFieldsPreserved()
        {
            var variables = GetDragonTypedVariables();
            var result = _fhirR4Processor.Convert(_fhirR4TestData, "DragonPatientMrn", _templateProvider, variables);
            var resultObj = JObject.Parse(result);

            // Verify passthrough fields
            Assert.Equal("example-patient-1", resultObj["id"]?.ToString());
            Assert.Equal(true, resultObj["active"]?.Value<bool>());
            Assert.Equal("male", resultObj["gender"]?.ToString());
            Assert.Equal("1990-01-15", resultObj["birthDate"]?.ToString());

            // Verify nested fields preserved by mergeDiff
            Assert.NotNull(resultObj["name"]);
            Assert.NotNull(resultObj["address"]);
            Assert.NotNull(resultObj["meta"]);
        }

        [Fact]
        public void GivenDragonMrnTemplate_WhenConvert_OriginalIdentifiersPreserved()
        {
            var variables = GetDragonTypedVariables();
            var result = _fhirR4Processor.Convert(_fhirR4TestData, "DragonPatientMrn", _templateProvider, variables);
            var resultObj = JObject.Parse(result);
            var identifiers = resultObj["identifier"] as JArray;

            // First 3 identifiers should be the originals
            Assert.Equal("MR", identifiers[0]?["type"]?["coding"]?[0]?["code"]?.ToString());
            Assert.Equal("http://hospital.example.org/mrn", identifiers[0]?["system"]?.ToString());
            Assert.Equal("SS", identifiers[1]?["type"]?["coding"]?[0]?["code"]?.ToString());
            Assert.Equal("DL", identifiers[2]?["type"]?["coding"]?[0]?["code"]?.ToString());
        }

        [Fact]
        public void GivenNoMatchingIdentifier_WhenConvert_RenderExceptionThrown()
        {
            var variables = GetDragonTypedVariables();
            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(_fhirR4NoMRData, "DragonPatientMrn", _templateProvider, variables));
            Assert.Contains("No identifier matched", exception.Message);
        }

        [Fact]
        public void GivenDuplicateMatchingIdentifiers_WhenConvert_RenderExceptionThrown()
        {
            var variables = GetDragonTypedVariables();
            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(_fhirR4DuplicateMRData, "DragonPatientMrn", _templateProvider, variables));
            Assert.Contains("Multiple identifiers", exception.Message);
            Assert.Contains("2", exception.Message);
        }

        [Fact]
        public void GivenNullVariables_WhenConvert_NoException()
        {
            // Using a simple template that doesn't require vars
            var templateCollection = new List<Dictionary<string, DotLiquid.Template>>
            {
                new Dictionary<string, DotLiquid.Template>
                {
                    { "SimpleTemplate", DotLiquid.Template.Parse(@"{""resourceType"":""Patient""}") },
                },
            };
            var simpleProvider = new TemplateProvider(templateCollection);

            var result = _fhirR4Processor.Convert(_fhirR4TestData, "SimpleTemplate", simpleProvider, null);
            Assert.Contains("Patient", result);
        }

        [Fact]
        public void GivenEmptyVariables_WhenConvert_NoException()
        {
            var templateCollection = new List<Dictionary<string, DotLiquid.Template>>
            {
                new Dictionary<string, DotLiquid.Template>
                {
                    { "SimpleTemplate", DotLiquid.Template.Parse(@"{""resourceType"":""Patient""}") },
                },
            };
            var simpleProvider = new TemplateProvider(templateCollection);

            var result = _fhirR4Processor.Convert(_fhirR4TestData, "SimpleTemplate", simpleProvider, new Dictionary<string, string>());
            Assert.Contains("Patient", result);
        }

        [Theory]
        [InlineData("matchCode")]
        [InlineData("dragon_system")]
        [InlineData("_private")]
        [InlineData("a1")]
        [InlineData("A")]
        public void GivenValidVariableNames_WhenValidate_NoException(string name)
        {
            VariableValidator.ValidateVariableName(name);
        }

        [Fact]
        public void GivenReservedNameMsg_WhenValidate_RenderExceptionThrown()
        {
            var exception = Assert.Throws<RenderException>(() => VariableValidator.ValidateVariableName("msg"));
            Assert.Contains("reserved", exception.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void GivenNullOrEmptyName_WhenValidate_RenderExceptionThrown(string name)
        {
            Assert.Throws<RenderException>(() => VariableValidator.ValidateVariableName(name));
        }

        [Theory]
        [InlineData("123abc")]
        [InlineData("match-code")]
        [InlineData("match.code")]
        [InlineData("match code")]
        [InlineData("valid!!!")]
        public void GivenInvalidVariableNames_WhenValidate_RenderExceptionThrown(string name)
        {
            var exception = Assert.Throws<RenderException>(() => VariableValidator.ValidateVariableName(name));
            Assert.Contains("Invalid variable name", exception.Message);
        }

        [Fact]
        public void GivenPreCancelledToken_WhenConvert_OperationCancelledExceptionThrown()
        {
            IFhirConverter converter = _fhirR4Processor;
            var variables = GetDragonTypedVariables();
            var cts = new CancellationTokenSource();
            cts.Cancel();
            Assert.Throws<OperationCanceledException>(() =>
                converter.Convert(_fhirR4TestData, "DragonPatientMrn", _templateProvider, variables, cts.Token));
        }

        [Fact]
        public void GivenFactoryCreatedProcessor_WhenCastToVariableInterface_CastSucceeds()
        {
            var factory = new ConvertProcessorFactory(new Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory());
            var processor = factory.GetProcessor(DataType.FhirR4, ConvertDataOutputFormat.Fhir);
            var variables = GetDragonTypedVariables();
            var result = processor.Convert(_fhirR4TestData, "DragonPatientMrn", _templateProvider, variables);
            Assert.Contains("Patient", result);
        }

        [Fact]
        public void GivenMissingSelectionCriteria_WhenConvert_RaiseErrorThrown()
        {
            var variables = new Dictionary<string, string>
            {
                { "dragonSystem", "urn:oid:1.2.3.4.5.6.7.8.9-dragon-copilot" },
            };
            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(_fhirR4TestData, "DragonPatientMrn", _templateProvider, variables));
            Assert.Equal(FhirConverterErrorCode.TemplateRenderingError, exception.FhirConverterErrorCode);
            Assert.Contains("selectionCriteria", exception.Message);
        }

        [Fact]
        public void GivenMissingDragonSystem_WhenConvert_RenderExceptionThrown()
        {
            // With Phase 2, the template expects selectionCriteria. Missing it should raise an error.
            var variables = new Dictionary<string, string>
            {
                { "matchCode", "MR" },
            };

            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(_fhirR4TestData, "DragonPatientMrn", _templateProvider, variables));
            Assert.Equal(FhirConverterErrorCode.TemplateRenderingError, exception.FhirConverterErrorCode);
            Assert.Contains("selectionCriteria", exception.Message);
        }

        [Fact]
        public void GivenEmptyStringDictVariables_WhenConvert_MissingSelectionCriteriaError()
        {
            var variables = new Dictionary<string, string>
            {
                { "matchCode", "MR" },
                { "dragonSystem", string.Empty },
            };

            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(_fhirR4TestData, "DragonPatientMrn", _templateProvider, variables));
            Assert.Equal(FhirConverterErrorCode.TemplateRenderingError, exception.FhirConverterErrorCode);
            Assert.Contains("selectionCriteria", exception.Message);
        }

        [Fact]
        public void GivenInvalidVariableKey_WhenConvert_InvalidVariableNameErrorCodeThrown()
        {
            var variables = new Dictionary<string, string>
            {
                { "match-code", "MR" },
            };
            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(_fhirR4TestData, "DragonPatientMrn", _templateProvider, variables));
            Assert.Equal(FhirConverterErrorCode.InvalidVariable, exception.FhirConverterErrorCode);
        }

        [Fact]
        public void GivenReservedVariableKey_WhenConvert_InvalidVariableNameErrorCodeThrown()
        {
            var variables = new Dictionary<string, string>
            {
                { "msg", "shadowed" },
            };
            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(_fhirR4TestData, "DragonPatientMrn", _templateProvider, variables));
            Assert.Equal(FhirConverterErrorCode.InvalidVariable, exception.FhirConverterErrorCode);
            Assert.Contains("reserved", exception.Message);
        }

        [Fact]
        public void GivenVariableKeyStartingWithDigit_WhenConvert_InvalidVariableNameErrorCodeThrown()
        {
            var variables = new Dictionary<string, string>
            {
                { "1code", "MR" },
            };
            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(_fhirR4TestData, "DragonPatientMrn", _templateProvider, variables));
            Assert.Equal(FhirConverterErrorCode.InvalidVariable, exception.FhirConverterErrorCode);
        }

        [Fact]
        public void GivenIdentifierWithMultipleMatchingCodings_WhenConvert_DragonIdentifierAdded()
        {
            // Single identifier that has two codings both with code MR — should count as ONE identifier match
            var variables = GetDragonTypedVariables();
            var result = _fhirR4Processor.Convert(_fhirR4MultiCodingMRData, "DragonPatientMrn", _templateProvider, variables);
            var resultObj = JObject.Parse(result);
            var identifiers = resultObj["identifier"] as JArray;
            Assert.Equal(3, identifiers.Count);
            var dragonId = identifiers[2];
            Assert.Equal("MRN-99999", dragonId["value"]?.ToString());
            Assert.Equal("urn:oid:1.2.3.4.5.6.7.8.9-dragon-copilot", dragonId["system"]?.ToString());
        }

        [Fact]
        public void GivenEmptyIdentifierArray_WhenConvert_RenderExceptionThrown()
        {
            var data = "{\"resourceType\":\"Patient\",\"id\":\"p1\",\"identifier\":[]}";
            var variables = GetDragonTypedVariables();
            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(data, "DragonPatientMrn", _templateProvider, variables));
            Assert.Equal(FhirConverterErrorCode.TemplateRenderingError, exception.FhirConverterErrorCode);
            Assert.Contains("No identifier matched", exception.Message);
        }

        [Fact]
        public void GivenIdentifierWithNoTypeField_WhenConvert_RenderExceptionThrown()
        {
            // Identifier without a type field is skipped by the filter — no match → error
            var data = "{\"resourceType\":\"Patient\",\"id\":\"p1\",\"identifier\":[{\"system\":\"http://example.org\",\"value\":\"123\"}]}";
            var variables = GetDragonTypedVariables();
            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(data, "DragonPatientMrn", _templateProvider, variables));
            Assert.Equal(FhirConverterErrorCode.TemplateRenderingError, exception.FhirConverterErrorCode);
            Assert.Contains("No identifier matched", exception.Message);
        }

        [Fact]
        public void GivenIdentifierWithEmptyCodingArray_WhenConvert_RenderExceptionThrown()
        {
            // Identifier has a type but coding array is empty — no match → error
            var data = "{\"resourceType\":\"Patient\",\"id\":\"p1\",\"identifier\":[{\"type\":{\"coding\":[]},\"value\":\"123\"}]}";
            var variables = GetDragonTypedVariables();
            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(data, "DragonPatientMrn", _templateProvider, variables));
            Assert.Equal(FhirConverterErrorCode.TemplateRenderingError, exception.FhirConverterErrorCode);
            Assert.Contains("No identifier matched", exception.Message);
        }

        [Fact]
        public void GivenValidationHelperDirectly_WhenInvalidName_CorrectErrorCode()
        {
            var exception = Assert.Throws<RenderException>(() => VariableValidator.ValidateVariableName("match.code"));
            Assert.Equal(FhirConverterErrorCode.InvalidVariable, exception.FhirConverterErrorCode);

            exception = Assert.Throws<RenderException>(() => VariableValidator.ValidateVariableName("msg"));
            Assert.Equal(FhirConverterErrorCode.InvalidVariable, exception.FhirConverterErrorCode);

            exception = Assert.Throws<RenderException>(() => VariableValidator.ValidateVariableName(null));
            Assert.Equal(FhirConverterErrorCode.InvalidVariable, exception.FhirConverterErrorCode);
        }

        [Fact]
        public void GivenPatientWithNoIdentifierField_WhenConvert_RenderExceptionThrown()
        {
            var data = "{\"resourceType\":\"Patient\",\"id\":\"p1\",\"active\":true}";
            var variables = GetDragonTypedVariables();
            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(data, "DragonPatientMrn", _templateProvider, variables));
            Assert.Equal(FhirConverterErrorCode.TemplateRenderingError, exception.FhirConverterErrorCode);
        }

        [Fact]
        public void GivenMalformedJsonInput_WhenConvert_DataParseExceptionThrown()
        {
            var malformedData = "this is not valid json {{{";
            var variables = GetDragonTypedVariables();
            Assert.ThrowsAny<FhirConverterException>(() =>
                _fhirR4Processor.Convert(malformedData, "DragonPatientMrn", _templateProvider, variables));
        }

        [Fact]
        public void GivenNullRootTemplate_WhenConvertWithVariables_RenderExceptionThrown()
        {
            var variables = GetDragonTypedVariables();
            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(_fhirR4TestData, null, _templateProvider, variables));
            Assert.Equal(FhirConverterErrorCode.NullOrEmptyRootTemplate, exception.FhirConverterErrorCode);
        }

        [Fact]
        public void GivenEmptyRootTemplate_WhenConvertWithVariables_RenderExceptionThrown()
        {
            var variables = GetDragonTypedVariables();
            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(_fhirR4TestData, string.Empty, _templateProvider, variables));
            Assert.Equal(FhirConverterErrorCode.NullOrEmptyRootTemplate, exception.FhirConverterErrorCode);
        }

        [Fact]
        public void GivenNullTemplateProvider_WhenConvertWithVariables_RenderExceptionThrown()
        {
            var variables = GetDragonTypedVariables();
            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(_fhirR4TestData, "DragonPatientMrn", null, variables));
            Assert.Equal(FhirConverterErrorCode.NullTemplateProvider, exception.FhirConverterErrorCode);
        }

        [Fact]
        public void GivenNonExistentTemplate_WhenConvertWithVariables_RenderExceptionThrown()
        {
            var variables = GetDragonTypedVariables();
            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(_fhirR4TestData, "NonExistentTemplate", _templateProvider, variables));
            Assert.Equal(FhirConverterErrorCode.TemplateNotFound, exception.FhirConverterErrorCode);
        }

        [Fact]
        public void GivenLowercaseMatchCode_WhenConvert_NoMatchBecauseCaseSensitive()
        {
            var variables = GetDragonTypedVariables("mr");
            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(_fhirR4TestData, "DragonPatientMrn", _templateProvider, variables));
            Assert.Equal(FhirConverterErrorCode.TemplateRenderingError, exception.FhirConverterErrorCode);
            Assert.Contains("No identifier matched", exception.Message);
        }

        [Theory]
        [InlineData("MSG")]
        [InlineData("Msg")]
        [InlineData("VARS")]
        [InlineData("Vars")]
        public void GivenReservedNameCaseVariations_WhenValidate_RenderExceptionThrown(string name)
        {
            var exception = Assert.Throws<RenderException>(() => VariableValidator.ValidateVariableName(name));
            Assert.Equal(FhirConverterErrorCode.InvalidVariable, exception.FhirConverterErrorCode);
            Assert.Contains("reserved", exception.Message);
        }

        [Fact]
        public void GivenTooManyVariables_WhenConvert_RenderExceptionThrown()
        {
            var variables = new Dictionary<string, string>();
            for (int i = 0; i < 101; i++)
            {
                variables[$"var_{i}"] = "value";
            }

            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(_fhirR4TestData, "DragonPatientMrn", _templateProvider, variables));
            Assert.Equal(FhirConverterErrorCode.InvalidVariable, exception.FhirConverterErrorCode);
            Assert.Contains("Too many variables", exception.Message);
        }

        [Fact]
        public void GivenVariableNameExceedingMaxLength_WhenValidate_RenderExceptionThrown()
        {
            var longName = new string('a', 129);
            var exception = Assert.Throws<RenderException>(() => VariableValidator.ValidateVariableName(longName));
            Assert.Equal(FhirConverterErrorCode.InvalidVariable, exception.FhirConverterErrorCode);
            Assert.Contains("length", exception.Message);
        }

        [Fact]
        public void GivenVariableValueExceedingMaxLength_WhenConvert_RenderExceptionThrown()
        {
            var variables = new Dictionary<string, string>
            {
                { "matchCode", new string('x', 1048577) },
            };
            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(_fhirR4TestData, "DragonPatientMrn", _templateProvider, variables));
            Assert.Equal(FhirConverterErrorCode.InvalidVariable, exception.FhirConverterErrorCode);
            Assert.Contains("exceeds", exception.Message);
        }

        [Fact]
        public void GivenDuplicateVariableNamesCaseInsensitive_WhenConvert_RenderExceptionThrown()
        {
            var variables = new Dictionary<string, string>
            {
                { "matchCode", "MR" },
                { "MatchCode", "MR" },
            };
            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(_fhirR4TestData, "DragonPatientMrn", _templateProvider, variables));
            Assert.Equal(FhirConverterErrorCode.InvalidVariable, exception.FhirConverterErrorCode);
            Assert.Contains("Duplicate", exception.Message);
        }

        // ============================================================
        // Phase 2: Typed variable validation and injection tests
        // ============================================================

        private static IList<VariableDefinition> GetTypedDragonVariables()
        {
            var criteriaValue = @"{
                ""conditions"": [{ ""path"": ""type.coding.code"", ""value"": ""MR"" }],
                ""outputSystem"": ""urn:oid:1.2.3.4.5.6.7.8.9-dragon-copilot""
            }";
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

        [Fact]
        public void GivenTypedStringVariable_WhenValidate_Passes()
        {
            var variables = new List<VariableDefinition>
            {
                new VariableDefinition { Name = "myVar", Type = VariableType.String, Value = "hello" },
            };

            // Should not throw
            var result = _fhirR4Processor.Convert(_fhirR4TestData, "Passthrough", _templateProvider, variables);
            Assert.NotNull(result);
        }

        [Fact]
        public void GivenTypedNumericIntegerVariable_WhenValidate_Passes()
        {
            var variables = new List<VariableDefinition>
            {
                new VariableDefinition { Name = "count", Type = VariableType.Numeric, Value = "42" },
            };

            var result = _fhirR4Processor.Convert(_fhirR4TestData, "Passthrough", _templateProvider, variables);
            Assert.NotNull(result);
        }

        [Fact]
        public void GivenTypedNumericFloatVariable_WhenValidate_Passes()
        {
            var variables = new List<VariableDefinition>
            {
                new VariableDefinition { Name = "rate", Type = VariableType.Numeric, Value = "3.14" },
            };

            var result = _fhirR4Processor.Convert(_fhirR4TestData, "Passthrough", _templateProvider, variables);
            Assert.NotNull(result);
        }

        [Fact]
        public void GivenTypedNumericInvalid_WhenConvert_RenderExceptionThrown()
        {
            var variables = new List<VariableDefinition>
            {
                new VariableDefinition { Name = "count", Type = VariableType.Numeric, Value = "not-a-number" },
            };

            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(_fhirR4TestData, "Passthrough", _templateProvider, variables));
            Assert.Equal(FhirConverterErrorCode.InvalidVariable, exception.FhirConverterErrorCode);
            Assert.Contains("could not be parsed", exception.Message);
        }

        [Fact]
        public void GivenTypedComplexVariableWithValidSchema_WhenConvert_Passes()
        {
            var variables = GetTypedDragonVariables();

            var result = _fhirR4Processor.Convert(_fhirR4TestData, "DragonPatientMrn", _templateProvider, variables);
            var resultObj = JObject.Parse(result);

            Assert.Equal("Patient", resultObj["resourceType"]?.ToString());
            var identifiers = resultObj["identifier"] as JArray;
            Assert.NotNull(identifiers);
            Assert.Equal(4, identifiers.Count);

            var dragonId = identifiers[3];
            Assert.Equal("urn:oid:1.2.3.4.5.6.7.8.9-dragon-copilot", dragonId["system"]?.ToString());
            Assert.Equal("MRN-12345", dragonId["value"]?.ToString());
        }

        [Fact]
        public void GivenTypedComplexVariableWithSchemaViolation_WhenConvert_RenderExceptionThrown()
        {
            // Missing required "conditions" field
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
                _fhirR4Processor.Convert(_fhirR4TestData, "DragonPatientMrn", _templateProvider, variables));
            Assert.Equal(FhirConverterErrorCode.InvalidVariable, exception.FhirConverterErrorCode);
            Assert.Contains("does not conform", exception.Message);
        }

        [Fact]
        public void GivenTypedComplexVariableWithNullSchema_WhenConvert_RenderExceptionThrown()
        {
            var variables = new List<VariableDefinition>
            {
                new VariableDefinition
                {
                    Name = "selectionCriteria",
                    Type = VariableType.Complex,
                    Value = @"{ ""conditions"": [] }",
                    Schema = null,
                },
            };

            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(_fhirR4TestData, "DragonPatientMrn", _templateProvider, variables));
            Assert.Equal(FhirConverterErrorCode.InvalidVariable, exception.FhirConverterErrorCode);
            Assert.Contains("requires a JSON Schema", exception.Message);
        }

        [Fact]
        public void GivenTypedComplexVariableWithInvalidJsonSchema_WhenConvert_RenderExceptionThrown()
        {
            var variables = new List<VariableDefinition>
            {
                new VariableDefinition
                {
                    Name = "selectionCriteria",
                    Type = VariableType.Complex,
                    Value = @"{ ""conditions"": [] }",
                    Schema = "this is not valid json",
                },
            };

            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(_fhirR4TestData, "DragonPatientMrn", _templateProvider, variables));
            Assert.Equal(FhirConverterErrorCode.InvalidVariable, exception.FhirConverterErrorCode);
            Assert.Contains("invalid JSON Schema", exception.Message);
        }

        [Fact]
        public void GivenTypedComplexVariableWithInvalidJsonValue_WhenConvert_RenderExceptionThrown()
        {
            var variables = new List<VariableDefinition>
            {
                new VariableDefinition
                {
                    Name = "selectionCriteria",
                    Type = VariableType.Complex,
                    Value = "not valid json {{{",
                    Schema = _identifierSelectionSchema,
                },
            };

            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(_fhirR4TestData, "DragonPatientMrn", _templateProvider, variables));
            Assert.Equal(FhirConverterErrorCode.InvalidVariable, exception.FhirConverterErrorCode);
            Assert.Contains("not valid JSON", exception.Message);
        }

        [Fact]
        public void GivenTypedStringVariableWithSchema_WhenConvert_RenderExceptionThrown()
        {
            var variables = new List<VariableDefinition>
            {
                new VariableDefinition { Name = "myVar", Type = VariableType.String, Value = "hello", Schema = "{}" },
            };

            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(_fhirR4TestData, "Passthrough", _templateProvider, variables));
            Assert.Equal(FhirConverterErrorCode.InvalidVariable, exception.FhirConverterErrorCode);
            Assert.Contains("must not have a schema", exception.Message);
        }

        [Fact]
        public void GivenTypedNumericVariableWithSchema_WhenConvert_RenderExceptionThrown()
        {
            var variables = new List<VariableDefinition>
            {
                new VariableDefinition { Name = "count", Type = VariableType.Numeric, Value = "42", Schema = "{}" },
            };

            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(_fhirR4TestData, "Passthrough", _templateProvider, variables));
            Assert.Equal(FhirConverterErrorCode.InvalidVariable, exception.FhirConverterErrorCode);
            Assert.Contains("must not have a schema", exception.Message);
        }

        [Fact]
        public void GivenTypedVariableWithNullValue_WhenConvert_RenderExceptionThrown()
        {
            var variables = new List<VariableDefinition>
            {
                new VariableDefinition { Name = "myVar", Type = VariableType.String, Value = null },
            };

            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(_fhirR4TestData, "Passthrough", _templateProvider, variables));
            Assert.Equal(FhirConverterErrorCode.InvalidVariable, exception.FhirConverterErrorCode);
            Assert.Contains("must not be null", exception.Message);
        }

        [Fact]
        public void GivenTypedVariableWithReservedName_WhenConvert_RenderExceptionThrown()
        {
            var variables = new List<VariableDefinition>
            {
                new VariableDefinition { Name = "msg", Type = VariableType.String, Value = "test" },
            };

            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(_fhirR4TestData, "Passthrough", _templateProvider, variables));
            Assert.Equal(FhirConverterErrorCode.InvalidVariable, exception.FhirConverterErrorCode);
            Assert.Contains("reserved", exception.Message);
        }

        [Fact]
        public void GivenTypedDuplicateVariableNames_WhenConvert_RenderExceptionThrown()
        {
            var variables = new List<VariableDefinition>
            {
                new VariableDefinition { Name = "myVar", Type = VariableType.String, Value = "a" },
                new VariableDefinition { Name = "MyVar", Type = VariableType.String, Value = "b" },
            };

            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(_fhirR4TestData, "Passthrough", _templateProvider, variables));
            Assert.Equal(FhirConverterErrorCode.InvalidVariable, exception.FhirConverterErrorCode);
            Assert.Contains("Duplicate", exception.Message);
        }

        [Fact]
        public void GivenTypedVariableExceedingMaxCount_WhenConvert_RenderExceptionThrown()
        {
            var variables = new List<VariableDefinition>();
            for (int i = 0; i <= VariableValidator.MaxVariableCount; i++)
            {
                variables.Add(new VariableDefinition { Name = $"var{i}", Type = VariableType.String, Value = "v" });
            }

            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(_fhirR4TestData, "Passthrough", _templateProvider, variables));
            Assert.Equal(FhirConverterErrorCode.InvalidVariable, exception.FhirConverterErrorCode);
            Assert.Contains("Too many", exception.Message);
        }

        [Fact]
        public void GivenTypedVariableViaInterface_WhenConvert_Works()
        {
            IFhirConverter converter = _fhirR4Processor;
            var variables = GetTypedDragonVariables();

            var result = converter.Convert(_fhirR4TestData, "DragonPatientMrn", _templateProvider, variables);
            var resultObj = JObject.Parse(result);

            Assert.Equal("Patient", resultObj["resourceType"]?.ToString());
            var identifiers = resultObj["identifier"] as JArray;
            Assert.NotNull(identifiers);
            Assert.Equal(4, identifiers.Count);
        }

        [Fact]
        public void GivenTypedVariableViaCancellationToken_WhenConvert_Works()
        {
            var variables = GetTypedDragonVariables();
            var cts = new CancellationTokenSource();

            var result = _fhirR4Processor.Convert(_fhirR4TestData, "DragonPatientMrn", _templateProvider, variables, cts.Token);
            var resultObj = JObject.Parse(result);

            Assert.Equal("Patient", resultObj["resourceType"]?.ToString());
            var identifiers = resultObj["identifier"] as JArray;
            Assert.NotNull(identifiers);
            Assert.Equal(4, identifiers.Count);
        }

        // ============================================================
        // TypedVarTest.liquid: verify runtime types are correct
        // ============================================================

        [Fact]
        public void GivenNumericTypedVariable_WhenConvertViaTypedVarTest_NumericRendersAsNumber()
        {
            var variables = new List<VariableDefinition>
            {
                new VariableDefinition { Name = "numericVar", Type = VariableType.Numeric, Value = "42" },
                new VariableDefinition { Name = "stringVar", Type = VariableType.String, Value = "hello" },
            };

            var result = _fhirR4Processor.Convert(_fhirR4TestData, "TypedVarTest", _templateProvider, variables);
            var resultObj = JObject.Parse(result);

            // numericVar should render as a number (42), not a string ("42")
            Assert.Equal(42L, resultObj["numericVar"]?.Value<long>());
            Assert.Equal(JTokenType.Integer, resultObj["numericVar"]?.Type);
            Assert.Equal("hello", resultObj["stringVar"]?.ToString());
        }

        [Fact]
        public void GivenFloatTypedVariable_WhenConvertViaTypedVarTest_FloatRendersAsNumber()
        {
            var variables = new List<VariableDefinition>
            {
                new VariableDefinition { Name = "numericVar", Type = VariableType.Numeric, Value = "3.14" },
                new VariableDefinition { Name = "stringVar", Type = VariableType.String, Value = "world" },
            };

            var result = _fhirR4Processor.Convert(_fhirR4TestData, "TypedVarTest", _templateProvider, variables);
            var resultObj = JObject.Parse(result);

            Assert.Equal(3.14, resultObj["numericVar"]?.Value<double>() ?? 0, precision: 2);
            Assert.Equal(JTokenType.Float, resultObj["numericVar"]?.Type);
        }

        [Fact]
        public void GivenComplexTypedVariable_WhenConvert_ComplexIsNavigableInLiquid()
        {
            // Verify complex variable sub-properties are navigable (vars.selectionCriteria.outputSystem)
            var variables = GetTypedDragonVariables();
            var result = _fhirR4Processor.Convert(_fhirR4TestData, "DragonPatientMrn", _templateProvider, variables);
            var resultObj = JObject.Parse(result);

            // The Dragon identifier system comes from vars.selectionCriteria.outputSystem
            var dragonId = (resultObj["identifier"] as JArray)?[3];
            Assert.Equal("urn:oid:1.2.3.4.5.6.7.8.9-dragon-copilot", dragonId?["system"]?.ToString());
        }

        // ============================================================
        // Negative cases: null entries, oversize schemas
        // ============================================================

        [Fact]
        public void GivenNullEntryInTypedVariableList_WhenConvert_RenderExceptionThrown()
        {
            var variables = new List<VariableDefinition>
            {
                new VariableDefinition { Name = "myVar", Type = VariableType.String, Value = "ok" },
                null,
            };

            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(_fhirR4TestData, "Passthrough", _templateProvider, variables));
            Assert.Equal(FhirConverterErrorCode.InvalidVariable, exception.FhirConverterErrorCode);
        }

        [Fact]
        public void GivenOversizeSchema_WhenConvert_RenderExceptionThrown()
        {
            var oversizeSchema = new string('x', VariableValidator.MaxVariableSchemaLength + 1);
            var variables = new List<VariableDefinition>
            {
                new VariableDefinition
                {
                    Name = "bigSchemaVar",
                    Type = VariableType.Complex,
                    Value = "{}",
                    Schema = oversizeSchema,
                },
            };

            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(_fhirR4TestData, "Passthrough", _templateProvider, variables));
            Assert.Equal(FhirConverterErrorCode.InvalidVariable, exception.FhirConverterErrorCode);
            Assert.Contains("schema exceeds", exception.Message);
        }
    }
}
