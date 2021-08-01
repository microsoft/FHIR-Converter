// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Globalization;
using DotLiquid;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.FilterTests
{
    public class FiltersRenderingTests
    {
        private const string TestTemplate = @"
{{ 'foo' | char_at: 0 }}
{{ 'foo' | contains: 'fo' }}
{{ '\E' | escape_special_chars }}
{{ '\\E' | unescape_special_chars }}
{{ true | is_nan }}
{{ -2019.6 | abs }}
{{ 3 | pow: 3 }}
{{ -5 | sign }}
{{ -34.53 | truncate_number }}
{{ 5 | divide: 2 }}";

        private const string Expected = @"
f
true
\\E
\E
true
2019.6
27
-1
-34
2.5";

        [Fact]
        public void FiltersRenderingTest()
        {
            var template = Template.Parse(TestTemplate);
            var context = new Context(CultureInfo.InvariantCulture);
            context.AddFilters(typeof(Filters));

            var actual = template.Render(RenderParameters.FromContext(context, CultureInfo.InvariantCulture));
            Assert.Equal(Expected, actual);
        }
    }
}
