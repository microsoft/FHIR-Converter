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
            Assert.NotNull(templateProvider.GetTemplate("ADT_A01"));

            // Invalid template directory
            Assert.Throws<TemplateLoadException>(() => new Hl7v2TemplateProvider(string.Empty));
            Assert.Throws<TemplateLoadException>(() => new Hl7v2TemplateProvider(Path.Join("a", "b", "c")));

            // Template collection
            var collection = new List<Dictionary<string, Template>>()
            {
                new Dictionary<string, Template>()
                {
                    { "foo", Template.Parse("bar") },
                },
            };
            templateProvider = new Hl7v2TemplateProvider(collection);
            Assert.NotNull(templateProvider.GetTemplate("foo"));
        }
    }
}
