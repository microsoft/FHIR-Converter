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
            yield return new object[] { File.ReadAllText(Path.Join(TestConstants.TestTemplateDirectory, @"ValidMergeDiffTemplates/ValidExample1.liquid")), File.ReadAllText(Path.Join(TestConstants.ExpectedDirectory, @"MergeDiffResults/ValidExample1.json")) };
            yield return new object[] { File.ReadAllText(Path.Join(TestConstants.TestTemplateDirectory, @"ValidMergeDiffTemplates/ValidExample2.liquid")), File.ReadAllText(Path.Join(TestConstants.ExpectedDirectory, @"MergeDiffResults/ValidExample2.json")) };
            yield return new object[] { File.ReadAllText(Path.Join(TestConstants.TestTemplateDirectory, @"ValidMergeDiffTemplates/ValidExample3.liquid")), File.ReadAllText(Path.Join(TestConstants.ExpectedDirectory, @"MergeDiffResults/ValidExample3.json")) };
            yield return new object[] { File.ReadAllText(Path.Join(TestConstants.TestTemplateDirectory, @"ValidMergeDiffTemplates/EmptyDiffContent.liquid")), File.ReadAllText(Path.Join(TestConstants.ExpectedDirectory, @"MergeDiffResults/EmptyDiffContent.json")) };
            yield return new object[] { File.ReadAllText(Path.Join(TestConstants.TestTemplateDirectory, @"ValidMergeDiffTemplates/EmptyDiffContent2.liquid")), File.ReadAllText(Path.Join(TestConstants.ExpectedDirectory, @"MergeDiffResults/EmptyDiffContent.json")) };
        }

        public static IEnumerable<object[]> GetInvalidMergeDiffBlockContentTypes()
        {
            yield return new object[] { File.ReadAllText(Path.Join(TestConstants.TestTemplateDirectory, @"InvalidMergeDiffTemplates/InvalidDiffBlockString.liquid")) };
            yield return new object[] { File.ReadAllText(Path.Join(TestConstants.TestTemplateDirectory, @"InvalidMergeDiffTemplates/InvalidDiffBlockArray.liquid")) };
            yield return new object[] { File.ReadAllText(Path.Join(TestConstants.TestTemplateDirectory, @"InvalidMergeDiffTemplates/InvalidDiffBlockInvalidJson.liquid")) };
        }

        public static IEnumerable<object[]> GetInvalidMergeDiffTemplateWithErrorSyntax()
        {
            yield return new object[] { File.ReadAllText(Path.Join(TestConstants.TestTemplateDirectory, @"InvalidMergeDiffTemplates/ErrorSyntax.liquid")) };
            yield return new object[] { File.ReadAllText(Path.Join(TestConstants.TestTemplateDirectory, @"InvalidMergeDiffTemplates/ErrorSyntax2.liquid")) };
        }

        [Theory]
        [MemberData(nameof(GetValidMergeDiffTemplateContents))]
        public void GivenValidMergeDiffTemplateContent_WhenParseAndRender_CorrectResultShouldBeReturned(string templateContent, string expectedResult)
        {
            // Template should be parsed correctly
            var template = TemplateUtility.ParseLiquidTemplate(TemplateName, templateContent);
            Assert.True(template.Root.NodeList.Count > 0);

            // Template should be rendered correctly
            var templateFolder = Path.Join(TestConstants.TestTemplateDirectory, @"ValidMergeDiffTemplates");
            var parser = new JsonDataParser();
            var inputContent = "{\"id\": \"0\",\"valueReference\" : \"testReference\",\"resourceType\" : \"oldType\",\"extension\":{\"test\" : \"test\"}}";

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
        [MemberData(nameof(GetInvalidMergeDiffBlockContentTypes))]
        public void GivenInvalidDiffBlockContent_WhenParseAndRender_ExceptionsShouldBeThrown(string templateContent)
        {
            var template = TemplateUtility.ParseLiquidTemplate(TemplateName, templateContent);

            var templateFolder = Path.Join(TestConstants.TestTemplateDirectory, @"InvalidMergeDiffTemplates");
            var templateProvider = new TemplateProvider(templateFolder, DataType.Json);
            var inputContent = "{\"id\": \"\\\"0\",\"resourceType\" : \"oldType\",\"extension\":{\"test\" : \"test\"}}";
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
        public void GivenInvalidDiffBlockContentWithTooMuchRecursive_WhenParseAndRender_ExceptionsShouldBeThrown()
        {
            var templateContent = File.ReadAllText(Path.Join(TestConstants.TestTemplateDirectory, @"InvalidMergeDiffTemplates/RecursiveTooMuchTemplate.liquid"));
            var template = TemplateUtility.ParseLiquidTemplate(TemplateName, templateContent);

            var templateFolder = Path.Join(TestConstants.TestTemplateDirectory, @"InvalidMergeDiffTemplates");
            var templateProvider = new TemplateProvider(templateFolder, DataType.Json);
            var inputContent = "{\"id\": \"\\\"0\",\"resourceType\" : \"oldType\",\"extension\":{\"test\" : \"test\"}}";
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
            Assert.Throws<DotLiquid.Exceptions.StackLevelException>(() => template.Render(RenderParameters.FromContext(context, CultureInfo.InvariantCulture)));
        }

        [Theory]
        [MemberData(nameof(GetInvalidMergeDiffTemplateWithErrorSyntax))]
        public void GivenInvalidMergeDiffTemplateWithErrorSyntax_WhenParse_ExceptionsShouldBeThrown(string templateContent)
        {
            Assert.Throws<TemplateLoadException>(() => TemplateUtility.ParseLiquidTemplate(TemplateName, templateContent));
        }

        [Fact]
        public void GivenValidDiffBlockContent_WhenInputIsInvalidJson_WhenParseAndRender_ExceptionsShouldBeThrown()
        {
            var templateContent = File.ReadAllText(Path.Join(TestConstants.TestTemplateDirectory, @"ValidMergeDiffTemplates/ValidExample2.liquid"));
            var template = TemplateUtility.ParseLiquidTemplate(TemplateName, templateContent);

            var templateFolder = Path.Join(TestConstants.TestTemplateDirectory, @"ValidMergeDiffTemplates");
            var templateProvider = new TemplateProvider(templateFolder, DataType.Json);

            var data = new List<string>() { "invalidJson" };
            var dictionary = new Dictionary<string, object> { { "msg", data } };
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
    }
}
