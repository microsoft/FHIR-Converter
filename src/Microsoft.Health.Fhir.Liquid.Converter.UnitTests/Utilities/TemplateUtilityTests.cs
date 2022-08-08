// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using DotLiquid.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
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
            var parsedTemplates = TemplateUtility.ParseTemplates(templates);
            Assert.Equal("a", parsedTemplates["ADT_A01"].Render());
            Assert.Equal("b", parsedTemplates["Resource/Patient"].Render());
            Assert.Equal("c", parsedTemplates["Resource/Encounter"].Render());

            var codeMapping = (CodeMapping)parsedTemplates["CodeSystem/CodeSystem"].Root.NodeList.First();
            Assert.Equal("d", codeMapping.Mapping["a"]["b"]["c"]);

            templates["Resource/_Patient.liquid"] = null;
            templates["CodeSystem/CodeSystem.json"] = null;
            parsedTemplates = TemplateUtility.ParseTemplates(templates);
            Assert.Null(parsedTemplates["Resource/Patient"]);
            Assert.Null(parsedTemplates["CodeSystem/CodeSystem"]);
        }

        [Fact]
        public void GivenValidCcdaTemplateContents_WhenParseTemplates_CorrectResultShouldBeReturned()
        {
            var templates = new Dictionary<string, string>
            {
                { "CCD.liquid", "a" },
                { "Resource/_Patient.liquid", "b" },
                { @"Resource\_Encounter.liquid", "c" },
                { "ValueSet/ValueSet.json", @"{""mapping"": {""a"": {""b"": {""c"": ""d""}}}}" },
            };
            var parsedTemplates = TemplateUtility.ParseTemplates(templates);

            Assert.Equal("a", parsedTemplates["CCD"].Render());
            Assert.Equal("b", parsedTemplates["Resource/Patient"].Render());
            Assert.Equal("c", parsedTemplates["Resource/Encounter"].Render());

            var codeMapping = (CodeMapping)parsedTemplates["ValueSet/ValueSet"].Root.NodeList.First();
            Assert.Equal("d", codeMapping.Mapping["a"]["b"]["c"]);

            templates["Resource/_Patient.liquid"] = null;
            templates["ValueSet/ValueSet.json"] = null;
            parsedTemplates = TemplateUtility.ParseTemplates(templates);
            Assert.Null(parsedTemplates["Resource/Patient"]);
            Assert.Null(parsedTemplates["ValueSet/ValueSet"]);
        }

        [Fact]
        public void GivenInvalidHl7v2TemplateContents_WhenParseTemplates_ExceptionsShouldBeThrown()
        {
            // Invalid DotLiquid template
            var templates = new Dictionary<string, string> { { "ADT_A01.liquid", "{{" } };
            var exception = Assert.Throws<TemplateLoadException>(() => TemplateUtility.ParseTemplates(templates));
            Assert.Equal(FhirConverterErrorCode.TemplateSyntaxError, exception.FhirConverterErrorCode);
            Assert.True(exception.InnerException is SyntaxException);

            // Invalid JSON
            templates = new Dictionary<string, string> { { "CodeSystem/CodeSystem.json", @"{""a""" } };
            exception = Assert.Throws<TemplateLoadException>(() => TemplateUtility.ParseTemplates(templates));
            Assert.Equal(FhirConverterErrorCode.InvalidCodeMapping, exception.FhirConverterErrorCode);
            Assert.True(exception.InnerException is JsonException);

            // Null CodeSystemMapping
            templates = new Dictionary<string, string> { { "CodeSystem/CodeSystem.json", string.Empty } };
            exception = Assert.Throws<TemplateLoadException>(() => TemplateUtility.ParseTemplates(templates));
            Assert.Equal(FhirConverterErrorCode.InvalidCodeMapping, exception.FhirConverterErrorCode);

            // Null CodeSystemMapping.Mapping
            templates = new Dictionary<string, string> { { "CodeSystem/CodeSystem.json", @"{""a"": ""b""}" } };
            exception = Assert.Throws<TemplateLoadException>(() => TemplateUtility.ParseTemplates(templates));
            Assert.Equal(FhirConverterErrorCode.InvalidCodeMapping, exception.FhirConverterErrorCode);
        }

        [Fact]
        public void GivenInvalidCcdaTemplateContents_WhenParseTemplates_ExceptionsShouldBeThrown()
        {
            // Invalid DotLiquid template
            var templates = new Dictionary<string, string> { { "CCD.liquid", "{{" } };
            var exception = Assert.Throws<TemplateLoadException>(() => TemplateUtility.ParseTemplates(templates));
            Assert.Equal(FhirConverterErrorCode.TemplateSyntaxError, exception.FhirConverterErrorCode);
            Assert.True(exception.InnerException is SyntaxException);

            // Invalid JSON
            templates = new Dictionary<string, string> { { "ValueSet/ValueSet.json", @"{""a""" } };
            exception = Assert.Throws<TemplateLoadException>(() => TemplateUtility.ParseTemplates(templates));
            Assert.Equal(FhirConverterErrorCode.InvalidCodeMapping, exception.FhirConverterErrorCode);
            Assert.True(exception.InnerException is JsonException);

            // Null ValueSetMapping
            templates = new Dictionary<string, string> { { "ValueSet/ValueSet.json", string.Empty } };
            exception = Assert.Throws<TemplateLoadException>(() => TemplateUtility.ParseTemplates(templates));
            Assert.Equal(FhirConverterErrorCode.InvalidCodeMapping, exception.FhirConverterErrorCode);

            // Null ValueSetMapping.Mapping
            templates = new Dictionary<string, string> { { "ValueSet/ValueSet.json", @"{""a"": ""b""}" } };
            exception = Assert.Throws<TemplateLoadException>(() => TemplateUtility.ParseTemplates(templates));
            Assert.Equal(FhirConverterErrorCode.InvalidCodeMapping, exception.FhirConverterErrorCode);
        }

        [Fact]
        public void GivenInvalidJsonSchemaTemplateContents_WhenParseTemplates_ExceptionsShouldBeThrown()
        {
            // Invalid schema content
            var templates = new Dictionary<string, string> { { "InvalidSchema.schema.json", @"{""type"": ""InvalidType"" }" } };
            var exception = Assert.Throws<TemplateLoadException>(() => TemplateUtility.ParseTemplates(templates));
            Assert.Equal(FhirConverterErrorCode.InvalidJsonSchema, exception.FhirConverterErrorCode);

            // Invalid JSON
            templates = new Dictionary<string, string> { { "InvalidSchema.schema.json", @"{""a""" } };
            exception = Assert.Throws<TemplateLoadException>(() => TemplateUtility.ParseTemplates(templates));
            Assert.Equal(FhirConverterErrorCode.InvalidJsonSchema, exception.FhirConverterErrorCode);

            // Null or empty schema
            templates = new Dictionary<string, string> { { "InvalidSchema.schema.json", string.Empty } };
            exception = Assert.Throws<TemplateLoadException>(() => TemplateUtility.ParseTemplates(templates));

            templates = new Dictionary<string, string> { { "InvalidSchema.schema.json", null} };
            exception = Assert.Throws<TemplateLoadException>(() => TemplateUtility.ParseTemplates(templates));
        }
    }
}
