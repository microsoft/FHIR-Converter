// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Hl7v2.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Utilities;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.Utilities
{
    public class TemplateUtilityTests
    {
        [Fact]
        public void GivenValidHl7v2TemplateContents_WhenParseTemplates_CorrectResultShouldBeReturned()
        {
            var templates = new Dictionary<string, string>
            {
                { "ADT_A01.liquid", "a" },
                { "Resource/_Patient.liquid", "b" },
                { @"Resource\_Encounter.liquid", "c" },
                { "CodeSystem/CodeSystem.json", @"{""mapping"": {""a"": {""b"": {""c"": ""d""}}}}" },
            };
            var parsedTemplates = TemplateUtility.ParseHl7v2Templates(templates);
            Assert.Equal("a", parsedTemplates["ADT_A01"].Render());
            Assert.Equal("b", parsedTemplates["Resource/Patient"].Render());
            Assert.Equal("c", parsedTemplates["Resource/Encounter"].Render());

            var codeSystemMapping = (CodeSystemMapping)parsedTemplates["CodeSystem/CodeSystem"].Root.NodeList.First();
            Assert.Equal("d", codeSystemMapping.Mapping["a"]["b"]["c"]);

            templates["Resource/_Patient.liquid"] = null;
            templates["CodeSystem/CodeSystem.json"] = null;
            parsedTemplates = TemplateUtility.ParseHl7v2Templates(templates);
            Assert.Null(parsedTemplates["Resource/Patient"]);
            Assert.Null(parsedTemplates["CodeSystem/CodeSystem"]);
        }

        [Fact]
        public void GivenInvalidHl7v2TemplateContents_WhenParseTemplates_ExceptionsShouldBeThrown()
        {
            // Invalid DotLiquid template
            var templates = new Dictionary<string, string> { { "ADT_A01.liquid", "{{" } };
            var exception = Assert.Throws<ConverterInitializeException>(() => TemplateUtility.ParseHl7v2Templates(templates));
            Assert.Equal(FhirConverterErrorCode.TemplateSyntaxError, exception.FhirConverterErrorCode);
            Assert.True(exception.InnerException is DotLiquid.Exceptions.SyntaxException);

            // Invalid JSON
            templates = new Dictionary<string, string> { { "CodeSystem/CodeSystem.json", @"{""a""" } };
            exception = Assert.Throws<ConverterInitializeException>(() => TemplateUtility.ParseHl7v2Templates(templates));
            Assert.Equal(FhirConverterErrorCode.InvalidCodeSystemMapping, exception.FhirConverterErrorCode);
            Assert.True(exception.InnerException is JsonException);

            // Null CodeSystemMapping
            templates = new Dictionary<string, string> { { "CodeSystem/CodeSystem.json", string.Empty } };
            exception = Assert.Throws<ConverterInitializeException>(() => TemplateUtility.ParseHl7v2Templates(templates));
            Assert.Equal(FhirConverterErrorCode.InvalidCodeSystemMapping, exception.FhirConverterErrorCode);

            // Null CodeSystemMapping.Mapping
            templates = new Dictionary<string, string> { { "CodeSystem/CodeSystem.json", @"{""a"": ""b""}" } };
            exception = Assert.Throws<ConverterInitializeException>(() => TemplateUtility.ParseHl7v2Templates(templates));
            Assert.Equal(FhirConverterErrorCode.InvalidCodeSystemMapping, exception.FhirConverterErrorCode);
        }
    }
}
