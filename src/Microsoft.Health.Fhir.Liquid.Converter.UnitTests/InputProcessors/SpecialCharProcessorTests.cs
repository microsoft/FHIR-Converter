// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Health.Fhir.Liquid.Converter.InputProcessors;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.InputProcessors
{
    public class SpecialCharProcessorTests
    {
        public static IEnumerable<object[]> GetDataForEscape()
        {
            yield return new object[] { "\\E", "\\\\E" };
            yield return new object[] { "E\"", "E\\\"" };
            yield return new object[] { "\\\"E", "\\\\\\\"E" };
        }

        public static IEnumerable<object[]> GetDataForUnescape()
        {
            yield return new object[] { "\\E", "\\E" };
            yield return new object[] { "E\"", "E\"" };
            yield return new object[] { "\\\"E", "\"E" };
            yield return new object[] { "\\\\E", "\\E" };
            yield return new object[] { "\\\\\"E", "\\\"E" };
        }

        [Theory]
        [MemberData(nameof(GetDataForEscape))]
        public void GivenAString_WhenEscape_EscapedStringShouldBeReturned(string input, string expected)
        {
            var result = SpecialCharProcessor.Escape(input);
            Assert.Equal(expected, result);
        }

        [Theory]
        [MemberData(nameof(GetDataForUnescape))]
        public void GivenAString_WhenUnescape_UnescapedStringShouldBeReturned(string input, string expected)
        {
            var result = SpecialCharProcessor.Unescape(input);
            Assert.Equal(expected, result);
        }
    }
}
