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
    public class MergeDiffTests
    {
        public const string TemplateName = "TemplateName";

        public static IEnumerable<object[]> GetValidMergeDiffTemplateContents()
        {
            yield return new object[] { File.ReadAllText(@"TestTemplates/ValidMergeDiffTemplates/ValidTemplate1.liquid"), File.ReadAllText(@"ExpectedMergeDiffResults/result1.json") };
            yield return new object[] { File.ReadAllText(@"TestTemplates/ValidMergeDiffTemplates/ValidTemplate2.liquid"), File.ReadAllText(@"ExpectedMergeDiffResults/result2.json") };
            yield return new object[] { File.ReadAllText(@"TestTemplates/ValidMergeDiffTemplates/ValidTemplate3.liquid"), File.ReadAllText(@"ExpectedMergeDiffResults/result3.json") };
            yield return new object[] { File.ReadAllText(@"TestTemplates/ValidMergeDiffTemplates/EmptyDiffContent.liquid"), File.ReadAllText(@"ExpectedMergeDiffResults/result4.json") };
        }

        public static IEnumerable<object[]> GetInvalidMergeDiffBlockContentTypes()
        {
            yield return new object[] { File.ReadAllText(@"TestTemplates/InvalidMergeDiffTemplates/InvalidDiffBlockString.liquid") };
            yield return new object[] { File.ReadAllText(@"TestTemplates/InvalidMergeDiffTemplates/InvalidDiffBlockArray.liquid") };
            yield return new object[] { File.ReadAllText(@"TestTemplates/InvalidMergeDiffTemplates/InvalidDiffBlockInvalidJson.liquid") };
        }

        public static IEnumerable<object[]> GetInvalidMergeDiffTemplateWithErrorSyntax()
        {
            yield return new object[] { File.ReadAllText(@"TestTemplates/InvalidMergeDiffTemplates/ErrorSyntax.liquid") };
            yield return new object[] { File.ReadAllText(@"TestTemplates/InvalidMergeDiffTemplates/ErrorSyntax2.liquid") };
        }

        [Theory]
        [MemberData(nameof(GetValidMergeDiffTemplateContents))]
        public void GivenValidEvaluateTemplateContent_WhenParseAndRender_CorrectResultShouldBeReturned(string templateContent, string expectedResult)
        {
            // Template should be parsed correctly
            var template = TemplateUtility.ParseTemplate(TemplateName, templateContent);
            Assert.True(template.Root.NodeList.Count > 0);

            // Template should be rendered correctly
            var templateFolder = @"TestTemplates/ValidMergeDiffTemplates";
            var parser = new JsonDataParser();
            var inputContent = "{\r\n\t\"id\": \"0\",\r\n\t\"valueReference\" : \"testReference\",\r\n\t\"resourceType\" : \"oldType\",\r\n\t\"extension\":{\r\n\t\t\"test\" : \"test\"\r\n\t}\r\n}";

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
        [MemberData(nameof(GetInvalidMergeDiffBlockContentTypes))]
        public void GivenInvalidDiffBlockContent_WhenParseAndRender_ExceptionsShouldBeThrown(string templateContent)
        {
            var template = TemplateUtility.ParseTemplate(TemplateName, templateContent);

            var templateFolder = @"TestTemplates/ValidMergeDiffTemplates";
            var templateProvider = new TemplateProvider(templateFolder, DataType.Json);
            var inputContent = "{\r\n\t\"id\": \"0\",\r\n\t\"resourceType\" : \"oldType\",\r\n\t\"extension\":{\r\n\t\t\"test\" : \"test\"\r\n\t}\r\n}";
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
        [MemberData(nameof(GetInvalidMergeDiffTemplateWithErrorSyntax))]
        public void GivenInvalidMergeDiffTemplateWithErrorSyntax_WhenParse_ExceptionsShouldBeThrown(string templateContent)
        {
            Assert.Throws<TemplateLoadException>(() => TemplateUtility.ParseTemplate(TemplateName, templateContent));
        }

        [Fact]
        public void GivenValidDiffBlockContent_WhenInputIsInvalidJson_WhenParseAndRender_ExceptionsShouldBeThrown()
        {
            var templateContent = File.ReadAllText(@"TestTemplates/ValidMergeDiffTemplates/ValidTemplate2.liquid");
            var template = TemplateUtility.ParseTemplate(TemplateName, templateContent);

            var templateFolder = @"TestTemplates/ValidMergeDiffTemplates";
            var templateProvider = new TemplateProvider(templateFolder, DataType.Json);

            var data = new List<string>() { "invalidJson" };
            var dictionary = new Dictionary<string, object> { { "msg", data } };
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
    }
}
