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

        static FhirR4ProcessorTests()
        {
            _fhirR4TestData = File.ReadAllText(Path.Join(TestConstants.SampleDataDirectory, "FhirR4", "PatientWithIdentifiers.json"));
            _fhirR4DuplicateMRData = File.ReadAllText(Path.Join(TestConstants.SampleDataDirectory, "FhirR4", "PatientDuplicateMR.json"));
            _fhirR4NoMRData = File.ReadAllText(Path.Join(TestConstants.SampleDataDirectory, "FhirR4", "PatientNoMR.json"));
            _fhirR4MultiCodingMRData = File.ReadAllText(Path.Join(TestConstants.SampleDataDirectory, "FhirR4", "PatientMultiCodingMR.json"));
            _processorSettings = new ProcessorSettings();
            _fhirR4Processor = new FhirR4Processor(_processorSettings, FhirConverterLogging.CreateLogger<FhirR4Processor>());
            _templateProvider = new TemplateProvider(TestConstants.FhirR4TemplateDirectory, DataType.FhirR4);
        }

        private static Dictionary<string, string> GetDragonVariables()
        {
            return new Dictionary<string, string>
            {
                { "matchCode", "MR" },
                { "dragonSystem", "urn:oid:1.2.3.4.5.6.7.8.9-dragon-copilot" },
            };
        }

        [Fact]
        public void GivenVariables_WhenConvertViaInterface_VariablesAreAccessible()
        {
            IFhirConverterWithVariables converter = _fhirR4Processor;
            var variables = GetDragonVariables();
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
            IFhirConverterWithVariables converter = _fhirR4Processor;
            var variables = GetDragonVariables();
            var result = converter.Convert(_fhirR4TestData, "DragonPatientMrn", _templateProvider, variables, CancellationToken.None);
            var resultObj = JObject.Parse(result);

            var identifiers = resultObj["identifier"] as JArray;
            Assert.NotNull(identifiers);
            Assert.Equal(4, identifiers.Count);
        }

        [Fact]
        public void GivenVariables_WhenConvertDirect_VariablesAreAccessible()
        {
            var variables = GetDragonVariables();
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
            var variables = GetDragonVariables();
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
            var variables = GetDragonVariables();
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
            var variables = GetDragonVariables();
            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(_fhirR4NoMRData, "DragonPatientMrn", _templateProvider, variables));
            Assert.Contains("No identifier found", exception.Message);
            Assert.Contains("MR", exception.Message);
        }

        [Fact]
        public void GivenDuplicateMatchingIdentifiers_WhenConvert_RenderExceptionThrown()
        {
            var variables = GetDragonVariables();
            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(_fhirR4DuplicateMRData, "DragonPatientMrn", _templateProvider, variables));
            Assert.Contains("Multiple identifiers", exception.Message);
            Assert.Contains("2", exception.Message);
            Assert.Contains("MR", exception.Message);
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
            FhirR4Processor.ValidateVariableName(name);
        }

        [Fact]
        public void GivenReservedNameMsg_WhenValidate_RenderExceptionThrown()
        {
            var exception = Assert.Throws<RenderException>(() => FhirR4Processor.ValidateVariableName("msg"));
            Assert.Contains("reserved", exception.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void GivenNullOrEmptyName_WhenValidate_RenderExceptionThrown(string name)
        {
            Assert.Throws<RenderException>(() => FhirR4Processor.ValidateVariableName(name));
        }

        [Theory]
        [InlineData("123abc")]
        [InlineData("match-code")]
        [InlineData("match.code")]
        [InlineData("match code")]
        [InlineData("valid!!!")]
        public void GivenInvalidVariableNames_WhenValidate_RenderExceptionThrown(string name)
        {
            var exception = Assert.Throws<RenderException>(() => FhirR4Processor.ValidateVariableName(name));
            Assert.Contains("Invalid variable name", exception.Message);
        }

        [Fact]
        public void GivenPreCancelledToken_WhenConvert_OperationCancelledExceptionThrown()
        {
            IFhirConverterWithVariables converter = _fhirR4Processor;
            var variables = GetDragonVariables();
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
            Assert.IsAssignableFrom<IFhirConverterWithVariables>(processor);
        }

        [Fact]
        public void GivenMissingMatchCode_WhenConvert_RaiseErrorThrown()
        {
            var variables = new Dictionary<string, string>
            {
                { "dragonSystem", "urn:oid:1.2.3.4.5.6.7.8.9-dragon-copilot" },
            };
            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(_fhirR4TestData, "DragonPatientMrn", _templateProvider, variables));
            Assert.Equal(FhirConverterErrorCode.TemplateRenderingError, exception.FhirConverterErrorCode);
            Assert.Contains("matchCode", exception.Message);
        }

        [Fact]
        public void GivenMissingDragonSystem_WhenConvert_RenderExceptionThrown()
        {
            var variables = new Dictionary<string, string>
            {
                { "matchCode", "MR" },
            };

            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(_fhirR4TestData, "DragonPatientMrn", _templateProvider, variables));
            Assert.Equal(FhirConverterErrorCode.TemplateRenderingError, exception.FhirConverterErrorCode);
            Assert.Contains("dragonSystem", exception.Message);
        }

        [Fact]
        public void GivenEmptyDragonSystem_WhenConvert_RenderExceptionThrown()
        {
            var variables = new Dictionary<string, string>
            {
                { "matchCode", "MR" },
                { "dragonSystem", string.Empty },
            };

            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(_fhirR4TestData, "DragonPatientMrn", _templateProvider, variables));
            Assert.Equal(FhirConverterErrorCode.TemplateRenderingError, exception.FhirConverterErrorCode);
            Assert.Contains("dragonSystem", exception.Message);
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
            Assert.Equal(FhirConverterErrorCode.InvalidVariableName, exception.FhirConverterErrorCode);
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
            Assert.Equal(FhirConverterErrorCode.InvalidVariableName, exception.FhirConverterErrorCode);
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
            Assert.Equal(FhirConverterErrorCode.InvalidVariableName, exception.FhirConverterErrorCode);
        }

        [Fact]
        public void GivenIdentifierWithMultipleMatchingCodings_WhenConvert_DragonIdentifierAdded()
        {
            // Single identifier that has two codings both with code MR — should count as ONE identifier match
            var variables = GetDragonVariables();
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
            var variables = GetDragonVariables();
            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(data, "DragonPatientMrn", _templateProvider, variables));
            Assert.Equal(FhirConverterErrorCode.TemplateRenderingError, exception.FhirConverterErrorCode);
            Assert.Contains("No identifier found", exception.Message);
        }

        [Fact]
        public void GivenIdentifierWithNoTypeField_WhenConvert_RenderExceptionThrown()
        {
            // Identifier without a type field is skipped by the template guard — no match → error
            var data = "{\"resourceType\":\"Patient\",\"id\":\"p1\",\"identifier\":[{\"system\":\"http://example.org\",\"value\":\"123\"}]}";
            var variables = GetDragonVariables();
            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(data, "DragonPatientMrn", _templateProvider, variables));
            Assert.Equal(FhirConverterErrorCode.TemplateRenderingError, exception.FhirConverterErrorCode);
            Assert.Contains("No identifier found", exception.Message);
        }

        [Fact]
        public void GivenIdentifierWithEmptyCodingArray_WhenConvert_RenderExceptionThrown()
        {
            // Identifier has a type but coding array is empty — inner loop does nothing → no match → error
            var data = "{\"resourceType\":\"Patient\",\"id\":\"p1\",\"identifier\":[{\"type\":{\"coding\":[]},\"value\":\"123\"}]}";
            var variables = GetDragonVariables();
            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(data, "DragonPatientMrn", _templateProvider, variables));
            Assert.Equal(FhirConverterErrorCode.TemplateRenderingError, exception.FhirConverterErrorCode);
            Assert.Contains("No identifier found", exception.Message);
        }

        [Fact]
        public void GivenValidationHelperDirectly_WhenInvalidName_CorrectErrorCode()
        {
            var exception = Assert.Throws<RenderException>(() => FhirR4Processor.ValidateVariableName("match.code"));
            Assert.Equal(FhirConverterErrorCode.InvalidVariableName, exception.FhirConverterErrorCode);

            exception = Assert.Throws<RenderException>(() => FhirR4Processor.ValidateVariableName("msg"));
            Assert.Equal(FhirConverterErrorCode.InvalidVariableName, exception.FhirConverterErrorCode);

            exception = Assert.Throws<RenderException>(() => FhirR4Processor.ValidateVariableName(null));
            Assert.Equal(FhirConverterErrorCode.InvalidVariableName, exception.FhirConverterErrorCode);
        }

        [Fact]
        public void GivenPatientWithNoIdentifierField_WhenConvert_RenderExceptionThrown()
        {
            var data = "{\"resourceType\":\"Patient\",\"id\":\"p1\",\"active\":true}";
            var variables = GetDragonVariables();
            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(data, "DragonPatientMrn", _templateProvider, variables));
            Assert.Equal(FhirConverterErrorCode.TemplateRenderingError, exception.FhirConverterErrorCode);
            Assert.Contains("No identifier found", exception.Message);
        }

        [Fact]
        public void GivenMalformedJsonInput_WhenConvert_DataParseExceptionThrown()
        {
            var malformedData = "this is not valid json {{{";
            var variables = GetDragonVariables();
            Assert.ThrowsAny<FhirConverterException>(() =>
                _fhirR4Processor.Convert(malformedData, "DragonPatientMrn", _templateProvider, variables));
        }

        [Fact]
        public void GivenNullRootTemplate_WhenConvertWithVariables_RenderExceptionThrown()
        {
            var variables = GetDragonVariables();
            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(_fhirR4TestData, null, _templateProvider, variables));
            Assert.Equal(FhirConverterErrorCode.NullOrEmptyRootTemplate, exception.FhirConverterErrorCode);
        }

        [Fact]
        public void GivenEmptyRootTemplate_WhenConvertWithVariables_RenderExceptionThrown()
        {
            var variables = GetDragonVariables();
            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(_fhirR4TestData, string.Empty, _templateProvider, variables));
            Assert.Equal(FhirConverterErrorCode.NullOrEmptyRootTemplate, exception.FhirConverterErrorCode);
        }

        [Fact]
        public void GivenNullTemplateProvider_WhenConvertWithVariables_RenderExceptionThrown()
        {
            var variables = GetDragonVariables();
            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(_fhirR4TestData, "DragonPatientMrn", null, variables));
            Assert.Equal(FhirConverterErrorCode.NullTemplateProvider, exception.FhirConverterErrorCode);
        }

        [Fact]
        public void GivenNonExistentTemplate_WhenConvertWithVariables_RenderExceptionThrown()
        {
            var variables = GetDragonVariables();
            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(_fhirR4TestData, "NonExistentTemplate", _templateProvider, variables));
            Assert.Equal(FhirConverterErrorCode.TemplateNotFound, exception.FhirConverterErrorCode);
        }

        [Fact]
        public void GivenLowercaseMatchCode_WhenConvert_NoMatchBecauseCaseSensitive()
        {
            var variables = new Dictionary<string, string>
            {
                { "matchCode", "mr" },
                { "dragonSystem", "urn:oid:1.2.3.4.5.6.7.8.9-dragon-copilot" },
            };
            var exception = Assert.Throws<RenderException>(() =>
                _fhirR4Processor.Convert(_fhirR4TestData, "DragonPatientMrn", _templateProvider, variables));
            Assert.Equal(FhirConverterErrorCode.TemplateRenderingError, exception.FhirConverterErrorCode);
            Assert.Contains("No identifier found", exception.Message);
        }

        [Theory]
        [InlineData("MSG")]
        [InlineData("Msg")]
        [InlineData("VARS")]
        [InlineData("Vars")]
        public void GivenReservedNameCaseVariations_WhenValidate_RenderExceptionThrown(string name)
        {
            var exception = Assert.Throws<RenderException>(() => FhirR4Processor.ValidateVariableName(name));
            Assert.Equal(FhirConverterErrorCode.InvalidVariableName, exception.FhirConverterErrorCode);
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
            Assert.Equal(FhirConverterErrorCode.InvalidVariableName, exception.FhirConverterErrorCode);
            Assert.Contains("Too many variables", exception.Message);
        }

        [Fact]
        public void GivenVariableNameExceedingMaxLength_WhenValidate_RenderExceptionThrown()
        {
            var longName = new string('a', 129);
            var exception = Assert.Throws<RenderException>(() => FhirR4Processor.ValidateVariableName(longName));
            Assert.Equal(FhirConverterErrorCode.InvalidVariableName, exception.FhirConverterErrorCode);
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
            Assert.Equal(FhirConverterErrorCode.InvalidVariableName, exception.FhirConverterErrorCode);
            Assert.Contains("exceeds", exception.Message);
        }
    }
}
