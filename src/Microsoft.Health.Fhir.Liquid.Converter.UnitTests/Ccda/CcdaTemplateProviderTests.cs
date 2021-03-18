// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.Ccda;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.Ccda
{
    public class CcdaTemplateProviderTests
    {
        [Fact]
        public void GivenATemplateDirectory_WhenLoadTemplates_CorrectResultsShouldBeReturned()
        {
            // Valid template directory
            var templateProvider = new CcdaTemplateProvider(Constants.CcdaTemplateDirectory);
            Assert.NotNull(templateProvider.GetTemplate("CCD"));

            // Invalid template directory
            Assert.Throws<ConverterInitializeException>(() => new CcdaTemplateProvider(string.Empty));
            Assert.Throws<ConverterInitializeException>(() => new CcdaTemplateProvider(Path.Join("a", "b", "c")));

            // Template collection
            var collection = new List<Dictionary<string, Template>>()
            {
                new Dictionary<string, Template>()
                {
                    { "foo", Template.Parse("bar") },
                },
            };
            templateProvider = new CcdaTemplateProvider(collection);
            Assert.NotNull(templateProvider.GetTemplate("foo"));
        }
    }
}
