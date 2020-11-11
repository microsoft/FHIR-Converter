// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.FilterTests
{
    public class StringFiltersTests
    {
        [Fact]
        public void CharAtTest()
        {
            Assert.Equal('b', Filters.CharAt("abc", 1));

            Assert.Throws<NullReferenceException>(() => Filters.CharAt(null, 2));
            Assert.Throws<IndexOutOfRangeException>(() => Filters.CharAt("A", 2));
        }

        [Fact]
        public void ContainsTest()
        {
            Assert.False(Filters.Contains(null, "bc"));
            Assert.False(Filters.Contains(string.Empty, "bc"));
            Assert.True(Filters.Contains("abcd", "bc"));

            Assert.Throws<ArgumentNullException>(() => Filters.Contains("abcd", null));
        }

        [Fact]
        public void EscapeSpecialCharsTest()
        {
            Assert.Equal("\\\"", Filters.EscapeSpecialChars("\""));
            Assert.Equal(string.Empty, Filters.EscapeSpecialChars(string.Empty));
            Assert.Null(Filters.EscapeSpecialChars(null));
        }

        [Fact]
        public void UnescapeSpecialCharsTest()
        {
            Assert.Equal("\"", Filters.UnescapeSpecialChars("\\\""));
            Assert.Equal(string.Empty, Filters.UnescapeSpecialChars(string.Empty));
            Assert.Null(Filters.UnescapeSpecialChars(null));
        }
    }
}
