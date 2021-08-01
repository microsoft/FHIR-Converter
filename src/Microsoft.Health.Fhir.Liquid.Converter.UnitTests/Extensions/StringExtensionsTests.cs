// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Health.Fhir.Liquid.Converter.Extensions;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.Extensions
{
    public class StringExtensionsTests
    {
        public static IEnumerable<object[]> GetIndexOfNthOccurrenceData()
        {
            // Null or empty string or char
            yield return new object[] { null, null, 1, -1 };
            yield return new object[] { string.Empty, null, 1, -1 };
            yield return new object[] { "abc|abc", null, 1, -1 };

            // Nth occurrence smaller than one
            yield return new object[] { "abc|abc", '|', 0, -1 };
            yield return new object[] { "abc|abc", '|', -1, -1 };

            // Nth occurrence hit
            yield return new object[] { "abc|abc", '|', 1, 3 };
            yield return new object[] { "abc|abc|abc", '|', 2, 7 };

            // Nth occurrence not hit
            yield return new object[] { "abc|abc", '|', 3, -1 };
            yield return new object[] { "abc|abc", '^', 1, -1 };
        }

        [Theory]
        [MemberData(nameof(GetIndexOfNthOccurrenceData))]
        public void IndexOfNthOccurrenceTests(string s, char c, int n, int expected)
        {
            Assert.Equal(expected, s.IndexOfNthOccurrence(c, n));
        }
    }
}
