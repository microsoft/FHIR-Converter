// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Globalization;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.DotLiquids;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.DotLiquids
{
    public class TemplateLocalFileSystemTests
    {
        [Fact]
        public void GivenAValidTemplateDirectory_WhenGetTemplate_CorrectResultsShouldBeReturned()
        {
            var templateLocalFileSystem = new TemplateLocalFileSystem(TestConstants.Hl7v2TemplateDirectory, DataType.Hl7v2);
            var context = new Context(CultureInfo.InvariantCulture);

            // Template exists
            Assert.NotNull(templateLocalFileSystem.GetTemplate("ADT_A01"));

            // Template does not exist
            Assert.Null(templateLocalFileSystem.GetTemplate("Foo"));
        }

        [Fact]
        public void GivenAValidTemplateDirectory_WhenGetTemplateWithContext_CorrectResultsShouldBeReturned()
        {
            var templateLocalFileSystem = new TemplateLocalFileSystem(TestConstants.Hl7v2TemplateDirectory, DataType.Hl7v2);
            var context = new Context(CultureInfo.InvariantCulture);

            // Template exists
            context["ADT_A01"] = "ADT_A01";
            Assert.NotNull(templateLocalFileSystem.GetTemplate(context, "ADT_A01"));

            // Template does not exist
            context["Foo"] = "Foo";
            Assert.Throws<RenderException>(() => templateLocalFileSystem.GetTemplate(context, "Foo"));
            Assert.Throws<RenderException>(() => templateLocalFileSystem.GetTemplate(context, "Bar"));
        }
    }
}
