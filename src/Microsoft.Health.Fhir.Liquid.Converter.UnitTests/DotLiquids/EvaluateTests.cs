// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Globalization;
using DotLiquid;
using DotLiquid.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Utilities;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.DotLiquids
{
    public class EvaluateTests
    {
        public const string TemplateName = "TemplateName";

        public static IEnumerable<object[]> GetValidEvaluateTemplateContents()
        {
            yield return new object[] { @"{% evaluate bundleId using 'ID/Bundle' -%}" };
            yield return new object[] { @"{% evaluate bundleId using 'ID/Bundle' Data: hl7v2Data -%}" };
            yield return new object[] { @"{% evaluate patientId using 'ID/Patient' PID: firstSegments.PID, type: 'First' -%}" };
        }

        public static IEnumerable<object[]> GetInvalidEvaluateTemplateContents()
        {
            // Missing "-%"
            yield return new object[] { @"{% evaluate bundleId using 'ID/Bundle' }" };

            // Missing "using"
            yield return new object[] { @"{% evaluate bundleId 'ID/Bundle' Data: hl7v2Data -%}" };

            // Missing variable
            yield return new object[] { @"{% evaluate using 'ID/Bundle' Data: hl7v2Data -%}" };

            // Missing template
            yield return new object[] { @"{% evaluate bundleId using Data: hl7v2Data -%}" };
        }

        [Theory]
        [MemberData(nameof(GetValidEvaluateTemplateContents))]
        public void GivenValidEvaluateTemplateContent_WhenParseAndRender_CorrectResultShouldBeReturned(string templateContent)
        {
            // Template should be parsed correctly
            var template = TemplateUtility.ParseLiquidTemplate(TemplateName, templateContent);
            Assert.True(template.Root.NodeList.Count > 0);

            // Template should be rendered correctly
            var templateProvider = new TemplateProvider(TestConstants.Hl7v2TemplateDirectory, DataType.Hl7v2);
            var context = new Context(
                environments: new List<Hash>(),
                outerScope: new Hash(),
                registers: Hash.FromDictionary(new Dictionary<string, object>() { { "file_system", templateProvider.GetTemplateFileSystem() } }),
                errorsOutputMode: ErrorsOutputMode.Rethrow,
                maxIterations: 0,
                timeout: 0,
                formatProvider: CultureInfo.InvariantCulture);

            Assert.Empty(template.Render(RenderParameters.FromContext(context, CultureInfo.InvariantCulture)));
        }

        [Theory]
        [MemberData(nameof(GetInvalidEvaluateTemplateContents))]
        public void GivenInvalidEvaluateTemplateContent_WhenParse_ExceptionsShouldBeThrown(string templateContent)
        {
            Assert.Throws<TemplateLoadException>(() => TemplateUtility.ParseLiquidTemplate(TemplateName, templateContent));
        }

        [Fact]
        public void GivenInvalidSnippet_WhenRender_ExceptionsShouldBeThrown()
        {
            // No template file system
            var template = TemplateUtility.ParseLiquidTemplate(TemplateName, @"{% evaluate bundleId using 'ID/Bundle' Data: hl7v2Data -%}");
            var context = new Context(
                environments: new List<Hash>(),
                outerScope: new Hash(),
                registers: new Hash(),
                errorsOutputMode: ErrorsOutputMode.Rethrow,
                maxIterations: 0,
                timeout: 0,
                formatProvider: CultureInfo.InvariantCulture);
            Assert.Throws<FileSystemException>(() => template.Render(RenderParameters.FromContext(context, CultureInfo.InvariantCulture)));

            // Valid template file system but no such template
            template = TemplateUtility.ParseLiquidTemplate(TemplateName, @"{% evaluate bundleId using 'ID/Foo' Data: hl7v2Data -%}");
            var templateProvider = new TemplateProvider(TestConstants.Hl7v2TemplateDirectory, DataType.Hl7v2);
            context = new Context(
                environments: new List<Hash>(),
                outerScope: new Hash(),
                registers: Hash.FromDictionary(new Dictionary<string, object>() { { "file_system", templateProvider.GetTemplateFileSystem() } }),
                errorsOutputMode: ErrorsOutputMode.Rethrow,
                maxIterations: 0,
                timeout: 0,
                formatProvider: CultureInfo.InvariantCulture);
            Assert.Throws<Exceptions.RenderException>(() => template.Render(RenderParameters.FromContext(context, CultureInfo.InvariantCulture)));
        }
    }
}
