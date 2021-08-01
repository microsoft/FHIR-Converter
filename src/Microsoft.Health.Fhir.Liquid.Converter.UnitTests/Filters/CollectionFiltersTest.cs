// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Globalization;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.FilterTests
{
    public class CollectionFiltersTest
    {
        [Fact]
        public void ToArrayTests()
        {
            Assert.Empty(Filters.ToArray(null));
            Assert.Single(Filters.ToArray(1));
            Assert.Equal(2, Filters.ToArray(new List<string> { null, string.Empty }).Count);
        }

        [Fact]
        public void ConcatTests()
        {
            Assert.Empty(Filters.Concat(null, null));
            Assert.Equal(2, Filters.Concat(new List<object> { string.Empty, null }, null).Count);
            Assert.Equal(2, Filters.Concat(new List<object> { string.Empty, null }, new List<object>()).Count);
        }

        [Fact]
        public void BatchRenderTests()
        {
            // Valid template file system and template
            var templateCollection = new List<Dictionary<string, Template>>
            {
                new Dictionary<string, Template>
                {
                    { "foo", Template.Parse("{{ i }} ") },
                },
            };

            var templateProvider = new TemplateProvider(templateCollection);
            var context = new Context(
                new List<Hash>(),
                new Hash(),
                Hash.FromDictionary(new Dictionary<string, object>() { { "file_system", templateProvider.GetTemplateFileSystem() } }),
                ErrorsOutputMode.Rethrow,
                0,
                0,
                CultureInfo.InvariantCulture);
            var collection = new List<object> { 1, 2, 3 };
            Assert.Equal("1 2 3 ", Filters.BatchRender(context, collection, "foo", "i"));

            // Valid template file system but null collection
            Assert.Equal(string.Empty, Filters.BatchRender(context, null, "foo", "i"));

            // No template file system
            context = new Context(CultureInfo.InvariantCulture);
            var exception = Assert.Throws<RenderException>(() => Filters.BatchRender(context, null, "foo", "bar"));
            Assert.Equal(FhirConverterErrorCode.TemplateNotFound, exception.FhirConverterErrorCode);

            // Valid template file system but non-existing template
            exception = Assert.Throws<RenderException>(() => Filters.BatchRender(context, collection, "bar", "i"));
            Assert.Equal(FhirConverterErrorCode.TemplateNotFound, exception.FhirConverterErrorCode);
        }
    }
}
