// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Hl7v2;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.Hl7v2
{
    public class Hl7v2TemplateProviderTests
    {
        [Fact]
        public void GivenATemplateDirectory_WhenLoadTemplates_CorrectResultsShouldBeReturned()
        {
            // Valid template directory
            var templateProvider = new Hl7v2TemplateProvider(Constants.Hl7v2TemplateDirectory);

            // Invalid template directory
            Assert.Throws<ConverterInitializeException>(() => new Hl7v2TemplateProvider(string.Empty));
            Assert.Throws<ConverterInitializeException>(() => new Hl7v2TemplateProvider(Path.Join("a", "b", "c")));

            // Template collection
            templateProvider = new Hl7v2TemplateProvider(new List<Dictionary<string, Template>>());
        }
    }
}
