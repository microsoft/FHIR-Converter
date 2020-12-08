// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Globalization;
using DotLiquid;
using DotLiquid.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Hl7v2;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.DotLiquids
{
    public class EvaluateTests
    {
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
            var templateProvider = new Hl7v2TemplateProvider(@"..\..\..\..\..\data\Templates\Hl7v2");
            var template = Template.Parse(templateContent);
            Assert.True(template.Root.NodeList.Count > 0);

            // Template should be rendered correctly
            var context = new Context(
                environments: new List<Hash>(),
                outerScope: new Hash(),
                registers: Hash.FromAnonymousObject(new { file_system = templateProvider }),
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
            var templateProvider = new Hl7v2TemplateProvider(@"..\..\..\..\..\data\Templates\Hl7v2");
            Assert.Throws<SyntaxException>(() => Template.Parse(templateContent));
        }

        [Fact]
        public void GivenInvalidSnippet_WhenRender_ExceptionsShouldBeThrown()
        {
            var templateProvider = new Hl7v2TemplateProvider(@"..\..\..\..\..\data\Templates\Hl7v2");

            // No template file system
            var template = Template.Parse(@"{% evaluate bundleId using 'ID/Bundle' Data: hl7v2Data -%}");
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
            template = Template.Parse(@"{% evaluate bundleId using 'ID/Foo' Data: hl7v2Data -%}");
            context = new Context(
                environments: new List<Hash>(),
                outerScope: new Hash(),
                registers: Hash.FromAnonymousObject(new { file_system = templateProvider }),
                errorsOutputMode: ErrorsOutputMode.Rethrow,
                maxIterations: 0,
                timeout: 0,
                formatProvider: CultureInfo.InvariantCulture);
            Assert.Throws<Exceptions.RenderException>(() => template.Render(RenderParameters.FromContext(context, CultureInfo.InvariantCulture)));
        }
    }
}
