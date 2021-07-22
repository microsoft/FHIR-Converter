// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests
{
    public class TemplateProviderTests
    {
        public static IEnumerable<object[]> GetValidTemplateProvider()
        {
            var collection = new List<Dictionary<string, Template>>
            {
                new Dictionary<string, Template>
                {
                    { "foo", Template.Parse("bar") },
                },
            };

            yield return new object[] { new TemplateProvider(TestConstants.Hl7v2TemplateDirectory, DataType.Hl7v2), new TemplateProvider(collection), "ADT_A01" };
            yield return new object[] { new TemplateProvider(TestConstants.CcdaTemplateDirectory, DataType.Ccda), new TemplateProvider(collection), "CCD" };
            yield return new object[] { new TemplateProvider(TestConstants.JsonTemplateDirectory, DataType.Json), new TemplateProvider(collection), "ExamplePatient" };
        }

        public static IEnumerable<object[]> GetInvalidTemplateDirectory()
        {
            yield return new object[] { string.Empty };
            yield return new object[] { Path.Join("a", "b", "c") };
        }

        [Theory]
        [MemberData(nameof(GetValidTemplateProvider))]
        public void GivenAValidTemplateProvider_WhenGetTemplate_CorrectResultsShouldBeReturned(ITemplateProvider directoryTemplateProvider, ITemplateProvider collectionTemplateProvider, string rootTemplate)
        {
            Assert.NotNull(directoryTemplateProvider.GetTemplate(rootTemplate));
            Assert.NotNull(collectionTemplateProvider.GetTemplate("foo"));
        }

        [Theory]
        [MemberData(nameof(GetInvalidTemplateDirectory))]
        public void GivenInvalidTemplateDirectory_WhenCreateTemplateProvider_ExceptionShouldBeReturned(string templateDirectory)
        {
            Assert.Throws<TemplateLoadException>(() => new TemplateProvider(templateDirectory, DataType.Hl7v2));
            Assert.Throws<TemplateLoadException>(() => new TemplateProvider(templateDirectory, DataType.Ccda));
            Assert.Throws<TemplateLoadException>(() => new TemplateProvider(templateDirectory, DataType.Json));
        }
    }
}