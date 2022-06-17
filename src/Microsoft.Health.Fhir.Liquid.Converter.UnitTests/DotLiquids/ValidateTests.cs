// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
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
            yield return new object[] { File.ReadAllText(Path.Join(TestConstants.TestTemplateDirectory, @"ValidValidateTemplates/ValidMatchedTemplate.liquid")), File.ReadAllText(Path.Join(TestConstants.ExpectedDirectory, @"ValidateResults/ValidateResult.json")) };
            yield return new object[] { File.ReadAllText(Path.Join(TestConstants.TestTemplateDirectory, @"ValidValidateTemplates/EmptyJsonContent.liquid")), File.ReadAllText(Path.Join(TestConstants.ExpectedDirectory, @"ValidateResults/EmptyValidateResult.json")) };
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
            yield return new object[] { File.ReadAllText(Path.Join(TestConstants.TestTemplateDirectory, @"InvalidValidateTemplates/InvalidRefSchema.liquid")) };
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
            var template = TemplateUtility.ParseTemplate(TemplateName, templateContent);
            Assert.True(template.Root.NodeList.Count > 0);

            // Template should be rendered correctly
            var templateFolder = Path.Join(TestConstants.TestTemplateDirectory, @"ValidValidateTemplates");
            var parser = new JsonDataParser();
            var inputContent = "{\"id\": \"0\",\"valueReference\" : \"testReference\",\"resourceType\" : \"Patient\",\"extension\":{\"valueString\" : \"valueString\"}}";

            var templateProvider = new TemplateProvider(templateFolder, DataType.Json);
            var jsonData = parser.Parse(inputContent);
            var dictionary = new Dictionary<string, object> { { "msg", jsonData } };
            var context = new Context(
                environments: new List<Hash> { Hash.FromDictionary(dictionary) },
                outerScope: new Hash(),
                registers: Hash.FromDictionary(new Dictionary<string, object>() { { "file_system", templateProvider.GetTemplateFileSystem() } }),
                errorsOutputMode: ErrorsOutputMode.Rethrow,
                maxIterations: 0,
                timeout: 0,
                formatProvider: CultureInfo.InvariantCulture);
            context.AddFilters(typeof(Filters));
            var result = template.Render(RenderParameters.FromContext(context, CultureInfo.InvariantCulture));
            var expectedObject = JObject.Parse(expectedResult);
            var actualObject = JObject.Parse(result);
            Assert.True(JToken.DeepEquals(expectedObject, actualObject));
        }

        [Theory]
        [MemberData(nameof(GetValidValidateUnmatchedTemplateContents))]
        public void GivenValidValidateUnmatchedContent_WhenParseAndRender_ExceptionsShouldBeThrown(string templateContent)
        {
            var template = TemplateUtility.ParseTemplate(TemplateName, templateContent);

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
                timeout: 0,
                formatProvider: CultureInfo.InvariantCulture);
            context.AddFilters(typeof(Filters));
            Assert.Throws<RenderException>(() => template.Render(RenderParameters.FromContext(context, CultureInfo.InvariantCulture)));
        }

        [Theory]
        [MemberData(nameof(GetInvalidValidateBlockContentTypes))]
        public void GivenInvalidValidateContent_WhenParseAndRender_ExceptionsShouldBeThrown(string templateContent)
        {
            var template = TemplateUtility.ParseTemplate(TemplateName, templateContent);

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
                timeout: 0,
                formatProvider: CultureInfo.InvariantCulture);
            context.AddFilters(typeof(Filters));
            Assert.Throws<RenderException>(() => template.Render(RenderParameters.FromContext(context, CultureInfo.InvariantCulture)));
        }

        [Theory]
        [MemberData(nameof(GetInvalidValidateTemplateWithErrorSyntax))]
        public void GivenInvalidValidateTemplateWithErrorSyntax_WhenParse_ExceptionsShouldBeThrown(string templateContent)
        {
            Assert.Throws<TemplateLoadException>(() => TemplateUtility.ParseTemplate(TemplateName, templateContent));
        }

    }
}
