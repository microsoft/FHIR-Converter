// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Models.Json;
using Microsoft.Health.Fhir.Liquid.Converter.Parsers;
using Microsoft.Health.Fhir.Liquid.Converter.Utilities;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.DotLiquids
{
    public class ValidateTests
    {
        public const string TemplateName = "TemplateName";

        public static IEnumerable<object[]> GetValidValidateMatchedTemplateContents()
        {
            yield return new object[] { File.ReadAllText(Path.Join(TestConstants.TestTemplateDirectory, @"ValidValidateTemplates/MatchedTemplate.liquid")), File.ReadAllText(Path.Join(TestConstants.ExpectedDirectory, @"ValidateResults/ValidateResult.json")) };
            yield return new object[] { File.ReadAllText(Path.Join(TestConstants.TestTemplateDirectory, @"ValidValidateTemplates/NestedSchemaTemplate.liquid")), File.ReadAllText(Path.Join(TestConstants.ExpectedDirectory, @"ValidateResults/NestedSchemaResult.json")) };
            yield return new object[] { File.ReadAllText(Path.Join(TestConstants.TestTemplateDirectory, @"ValidValidateTemplates/EmptyJsonContent.liquid")), File.ReadAllText(Path.Join(TestConstants.ExpectedDirectory, @"ValidateResults/EmptyValidateResult.json")) };
            yield return new object[] { File.ReadAllText(Path.Join(TestConstants.TestTemplateDirectory, @"ValidValidateTemplates/SubPropertySchemaTemplate.liquid")), File.ReadAllText(Path.Join(TestConstants.ExpectedDirectory, @"ValidateResults/NestedSchemaResult.json")) };
        }

        public static IEnumerable<object[]> GetValidValidateMatchedTemplateWithSchemaContents()
        {
            yield return new object[] { File.ReadAllText(Path.Join(TestConstants.TestTemplateDirectory, @"ValidValidateTemplates/MatchedTemplate.liquid")), new List<string> { "ValidValidateTemplates/Schemas/TestSchema.schema.json" } };
            yield return new object[] { File.ReadAllText(Path.Join(TestConstants.TestTemplateDirectory, @"ValidValidateTemplates/NestedSchemaTemplate.liquid")), new List<string> { "ValidValidateTemplates/Schemas/TestSchema.schema.json", "ValidValidateTemplates/Schemas/TestSubPropertySchema.schema.json" } };
            yield return new object[] { File.ReadAllText(Path.Join(TestConstants.TestTemplateDirectory, @"ValidValidateTemplates/EmptyJsonContent.liquid")), new List<string> { "ValidValidateTemplates/Schemas/TestSchema.schema.json" } };
            yield return new object[] { File.ReadAllText(Path.Join(TestConstants.TestTemplateDirectory, @"ValidValidateTemplates/SubPropertySchemaTemplate.liquid")), new List<string> { "ValidValidateTemplates/Schemas/TestSubPropertySchema.schema.json" } };
        }

        public static IEnumerable<object[]> GetValidValidateUnmatchedTemplateContents()
        {
            yield return new object[] { File.ReadAllText(Path.Join(TestConstants.TestTemplateDirectory, @"ValidValidateTemplates/ValidUnmatchedTemplate1.liquid")) };
            yield return new object[] { File.ReadAllText(Path.Join(TestConstants.TestTemplateDirectory, @"ValidValidateTemplates/ValidUnmatchedTemplate2.liquid")) };
            yield return new object[] { File.ReadAllText(Path.Join(TestConstants.TestTemplateDirectory, @"ValidValidateTemplates/ValidUnmatchedTemplate3.liquid")) };
        }

        public static IEnumerable<object[]> GetInvalidValidateBlockContentTypes()
        {
            yield return new object[] { File.ReadAllText(Path.Join(TestConstants.TestTemplateDirectory, @"InvalidValidateTemplates/InvalidEmptyContent.liquid")) };
            yield return new object[] { File.ReadAllText(Path.Join(TestConstants.TestTemplateDirectory, @"InvalidValidateTemplates/InvalidNoneJsonContent.liquid")) };
        }

        public static IEnumerable<object[]> GetInvalidValidateTemplateWithErrorSyntax()
        {
            yield return new object[] { File.ReadAllText(Path.Join(TestConstants.TestTemplateDirectory, @"InvalidValidateTemplates/ErrorNoSchemaInSyntax.liquid")) };
            yield return new object[] { File.ReadAllText(Path.Join(TestConstants.TestTemplateDirectory, @"InvalidValidateTemplates/ErrorQuotedSchemaInSyntax.liquid")) };
            yield return new object[] { File.ReadAllText(Path.Join(TestConstants.TestTemplateDirectory, @"InvalidValidateTemplates/ErrorVariableInSyntax.liquid")) };
        }

        [Theory]
        [MemberData(nameof(GetValidValidateMatchedTemplateContents))]
        public void GivenValidValidateMatchedTemplateContent_WhenParseAndRender_CorrectResultShouldBeReturned(string templateContent, string expectedResult)
        {
            // Template should be parsed correctly
            var template = TemplateUtility.ParseLiquidTemplate(TemplateName, templateContent);
            Assert.True(template.Root.NodeList.Count > 0);

            // Template should be rendered correctly
            var templateFolder = Path.Join(TestConstants.TestTemplateDirectory, @"ValidValidateTemplates");
            var parser = new JsonDataParser();
            var inputContent = "{\"id\": \"0\",\"valueReference\" : \"testReference\",\"resourceType\" : \"Patient\",\"name\":{\"valueString\" : \"valueString\"}}";

            var templateProvider = new TemplateProvider(templateFolder, DataType.Json);
            var jsonData = parser.Parse(inputContent);
            var dictionary = new Dictionary<string, object> { { "msg", jsonData } };
            var context = new Context(
                environments: new List<Hash> { Hash.FromDictionary(dictionary) },
                outerScope: new Hash(),
                registers: Hash.FromDictionary(new Dictionary<string, object>() { { "file_system", templateProvider.GetTemplateFileSystem() } }),
                errorsOutputMode: ErrorsOutputMode.Rethrow,
                maxIterations: 0,
                formatProvider: CultureInfo.InvariantCulture,
                cancellationToken: CancellationToken.None);
            context.AddFilters(typeof(Filters));
            var result = template.Render(RenderParameters.FromContext(context, CultureInfo.InvariantCulture));
            var expectedObject = JObject.Parse(expectedResult);
            var actualObject = JObject.Parse(result);
            Assert.True(JToken.DeepEquals(expectedObject, actualObject));
        }

        [Theory]
        [MemberData(nameof(GetValidValidateMatchedTemplateContents))]
        public void GivenValidValidateMatchedTemplateContent_WhenRenderMultipleTimes_CorrectResultShouldBeReturned(string templateContent, string expectedResult)
        {
            int repeatCount = 100000;

            // Template should be parsed correctly
            var template = TemplateUtility.ParseLiquidTemplate(TemplateName, templateContent);

            var templateFolder = Path.Join(TestConstants.TestTemplateDirectory, @"ValidValidateTemplates");
            var parser = new JsonDataParser();
            var inputContent = "{\"id\": \"0\",\"valueReference\" : \"testReference\",\"resourceType\" : \"Patient\",\"name\":{\"valueString\" : \"valueString\"}}";

            var templateProvider = new TemplateProvider(templateFolder, DataType.Json);
            var jsonData = parser.Parse(inputContent);
            var dictionary = new Dictionary<string, object> { { "msg", jsonData } };
            var context = new Context(
                environments: new List<Hash> { Hash.FromDictionary(dictionary) },
                outerScope: new Hash(),
                registers: Hash.FromDictionary(new Dictionary<string, object>() { { "file_system", templateProvider.GetTemplateFileSystem() } }),
                errorsOutputMode: ErrorsOutputMode.Rethrow,
                maxIterations: 0,
                formatProvider: CultureInfo.InvariantCulture,
                cancellationToken: CancellationToken.None);
            context.AddFilters(typeof(Filters));

            string result = string.Empty;
            for (int i = 0; i < repeatCount; i++)
            {
                result = template.Render(RenderParameters.FromContext(context, CultureInfo.InvariantCulture));
            }

            var expectedObject = JObject.Parse(expectedResult);
            var actualObject = JObject.Parse(result);
            Assert.True(JToken.DeepEquals(expectedObject, actualObject));
        }

        [Theory]
        [MemberData(nameof(GetValidValidateMatchedTemplateWithSchemaContents))]
        public void GivenValidValidateMatchedTemplateContent_WithJsonContext_WhenParseAndRender_InvolvedSchemaShouldBeReturned(string templateContent, List<string> expectSchemaFiles)
        {
            // Template should be parsed correctly
            var template = TemplateUtility.ParseLiquidTemplate(TemplateName, templateContent);
            Assert.True(template.Root.NodeList.Count > 0);

            // Template should be rendered correctly
            var templateFolder = Path.Join(TestConstants.TestTemplateDirectory, @"ValidValidateTemplates");
            var parser = new JsonDataParser();
            var inputContent = "{\"id\": \"0\",\"valueReference\" : \"testReference\",\"resourceType\" : \"Patient\",\"name\":{\"valueString\" : \"valueString\"}}";

            var templateProvider = new TemplateProvider(templateFolder, DataType.Json);
            var jsonData = parser.Parse(inputContent);
            var dictionary = new Dictionary<string, object> { { "msg", jsonData } };
            var context = new JSchemaContext(
                environments: new List<Hash> { Hash.FromDictionary(dictionary) },
                outerScope: new Hash(),
                registers: Hash.FromDictionary(new Dictionary<string, object>() { { "file_system", templateProvider.GetTemplateFileSystem() } }),
                errorsOutputMode: ErrorsOutputMode.Rethrow,
                maxIterations: 0,
                formatProvider: CultureInfo.InvariantCulture,
                cancellationToken: CancellationToken.None);
            context.AddFilters(typeof(Filters));
            var result = template.Render(RenderParameters.FromContext(context, CultureInfo.InvariantCulture));

            Assert.Equal(expectSchemaFiles.Count, context.ValidateSchemas.Count);

            for (int i = 0; i < expectSchemaFiles.Count; i++)
            {
                var expectedSchemaObject = JObject.Parse(File.ReadAllText(Path.Join(TestConstants.TestTemplateDirectory, expectSchemaFiles[i])));
                var actualSchemaObject = JObject.Parse(context.ValidateSchemas[i].ToJson());
                Assert.True(JToken.DeepEquals(expectedSchemaObject, actualSchemaObject));
            }
        }

        [Theory]
        [MemberData(nameof(GetValidValidateUnmatchedTemplateContents))]
        public void GivenValidValidateUnmatchedContent_WhenParseAndRender_ExceptionsShouldBeThrown(string templateContent)
        {
            var template = TemplateUtility.ParseLiquidTemplate(TemplateName, templateContent);

            var templateFolder = Path.Join(TestConstants.TestTemplateDirectory, @"ValidValidateTemplates");
            var templateProvider = new TemplateProvider(templateFolder, DataType.Json);
            var inputContent = "{\"id\": \"0\",\"resourceType\" : \"testType\",\"extension\":{\"test\" : \"test\"}}";
            var parser = new JsonDataParser();
            var jsonData = parser.Parse(inputContent);
            var dictionary = new Dictionary<string, object> { { "msg", jsonData } };
            var context = new Context(
                environments: new List<Hash> { Hash.FromDictionary(dictionary) },
                outerScope: new Hash(),
                registers: Hash.FromDictionary(new Dictionary<string, object>() { { "file_system", templateProvider.GetTemplateFileSystem() } }),
                errorsOutputMode: ErrorsOutputMode.Rethrow,
                maxIterations: 0,
                formatProvider: CultureInfo.InvariantCulture,
                cancellationToken: CancellationToken.None);
            context.AddFilters(typeof(Filters));
            Assert.Throws<RenderException>(() => template.Render(RenderParameters.FromContext(context, CultureInfo.InvariantCulture)));
        }

        [Theory]
        [MemberData(nameof(GetInvalidValidateBlockContentTypes))]
        public void GivenInvalidValidateContent_WhenParseAndRender_ExceptionsShouldBeThrown(string templateContent)
        {
            var template = TemplateUtility.ParseLiquidTemplate(TemplateName, templateContent);

            var templateFolder = Path.Join(TestConstants.TestTemplateDirectory, @"InvalidValidateTemplates");
            var templateProvider = new TemplateProvider(templateFolder, DataType.Json);
            var inputContent = "{\"id\": \"0\",\"resourceType\" : \"testType\",\"extension\":{\"test\" : \"test\"}}";
            var parser = new JsonDataParser();
            var jsonData = parser.Parse(inputContent);
            var dictionary = new Dictionary<string, object> { { "msg", jsonData } };
            var context = new Context(
                environments: new List<Hash> { Hash.FromDictionary(dictionary) },
                outerScope: new Hash(),
                registers: Hash.FromDictionary(new Dictionary<string, object>() { { "file_system", templateProvider.GetTemplateFileSystem() } }),
                errorsOutputMode: ErrorsOutputMode.Rethrow,
                maxIterations: 0,
                formatProvider: CultureInfo.InvariantCulture,
                cancellationToken: CancellationToken.None);
            context.AddFilters(typeof(Filters));
            Assert.Throws<RenderException>(() => template.Render(RenderParameters.FromContext(context, CultureInfo.InvariantCulture)));
        }

        [Fact]
        public void GivenInvalidValidateSchema_WhenParseAndRender_ExceptionsShouldBeThrown()
        {
            var templateContent = File.ReadAllText(Path.Join(TestConstants.TestTemplateDirectory, @"InvalidValidateTemplates/InvalidRefSchema.liquid"));
            var template = TemplateUtility.ParseLiquidTemplate(TemplateName, templateContent);

            var templateFolder = Path.Join(TestConstants.TestTemplateDirectory, @"InvalidValidateTemplates");
            var templateProvider = new TemplateProvider(templateFolder, DataType.Json);
            var inputContent = "{\"id\": \"0\",\"resourceType\" : \"testType\",\"extension\":{\"test\" : \"test\"}}";
            var parser = new JsonDataParser();
            var jsonData = parser.Parse(inputContent);
            var dictionary = new Dictionary<string, object> { { "msg", jsonData } };
            var context = new Context(
                environments: new List<Hash> { Hash.FromDictionary(dictionary) },
                outerScope: new Hash(),
                registers: Hash.FromDictionary(new Dictionary<string, object>() { { "file_system", templateProvider.GetTemplateFileSystem() } }),
                errorsOutputMode: ErrorsOutputMode.Rethrow,
                maxIterations: 0,
                formatProvider: CultureInfo.InvariantCulture,
                cancellationToken: CancellationToken.None);
            context.AddFilters(typeof(Filters));
            Assert.Throws<TemplateLoadException>(() => template.Render(RenderParameters.FromContext(context, CultureInfo.InvariantCulture)));
        }

        [Theory]
        [MemberData(nameof(GetInvalidValidateTemplateWithErrorSyntax))]
        public void GivenInvalidValidateTemplateWithErrorSyntax_WhenParse_ExceptionsShouldBeThrown(string templateContent)
        {
            Assert.Throws<TemplateLoadException>(() => TemplateUtility.ParseLiquidTemplate(TemplateName, templateContent));
        }
    }
}
