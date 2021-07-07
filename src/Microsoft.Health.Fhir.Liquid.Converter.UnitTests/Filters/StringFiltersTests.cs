// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
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

        [Fact]
        public void MatchTest()
        {
            Assert.Empty(Filters.Match(string.Empty, "[0-9]"));
            Assert.Empty(Filters.Match(null, "[0-9]"));
            Assert.Single(Filters.Match("foo1", "[0-9]"));

            Assert.Throws<ArgumentNullException>(() => Filters.Match("foo1", null));
            Assert.ThrowsAny<ArgumentException>(() => Filters.Match("foo1", "[a-z"));
        }

        [Fact]
        public void ToJsonStringTests()
        {
            Assert.Null(Filters.ToJsonString(null));
            Assert.Equal(@"[""a"",""b""]", Filters.ToJsonString(new List<string>() { "a", "b" }));
        }

        [Fact]
        public void GzipTest()
        {
            Assert.Equal("H4sIAAAAAAAACivNS87PLShKLS5OTQEA3a5CsQwAAAA=", Filters.Gzip("uncompressed"));
            Assert.Equal("uncompressed", Filters.GunzipBase64String(Filters.Gzip("uncompressed")));
            Assert.Equal(string.Empty, Filters.Gzip(string.Empty));

            Assert.Throws<ArgumentNullException>(() => Filters.Gzip(null));
        }

        public void GunzipBase64StringTest()
        {
            Assert.Equal("uncompressed", Filters.GunzipBase64String("H4sIAAAAAAAACivNS87PLShKLS5OTQEA3a5CsQwAAAA="));
            Assert.Equal("H4sIAAAAAAAACivNS87PLShKLS5OTQEA3a5CsQwAAAA=", Filters.Gzip(Filters.GunzipBase64String("H4sIAAAAAAAAEyvNS87PLShKLS5OTQEA3a5CsQwAAAA=")));
            Assert.Equal(string.Empty, Filters.GunzipBase64String(string.Empty));

            Assert.Throws<ArgumentNullException>(() => Filters.GunzipBase64String(null));
        }

        [Fact]
        public void Sha1HashTest()
        {
            Assert.Equal("a9993e364706816aba3e25717850c26c9cd0d89d", Filters.Sha1Hash("abc"));
            Assert.Equal("da39a3ee5e6b4b0d3255bfef95601890afd80709", Filters.Sha1Hash(string.Empty));

            Assert.Throws<ArgumentNullException>(() => Filters.Sha1Hash(null));
        }

        [Fact]
        public void Base64EncodeTest()
        {
            Assert.Equal("YSJi", Filters.Base64Encode(@"a""b"));
            Assert.Equal(string.Empty, Filters.Base64Encode(string.Empty));

            Assert.Throws<ArgumentNullException>(() => Filters.Base64Encode(null));
        }

        [Fact]
        public void Base64DecodeTest()
        {
            Assert.Equal(@"a""b", Filters.Base64Decode("YSJi"));
            Assert.Equal(string.Empty, Filters.Base64Decode(string.Empty));

            Assert.Throws<ArgumentNullException>(() => Filters.Base64Decode(null));
        }
    }
}
