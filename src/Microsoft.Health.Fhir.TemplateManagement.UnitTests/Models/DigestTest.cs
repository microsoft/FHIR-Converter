// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Xunit;

namespace Microsoft.Health.Fhir.TemplateManagement.UnitTests.Models
{
    public class DigestTest
    {
        public static IEnumerable<object[]> GetInputStringContainsDigests()
        {
            yield return new object[]
            {
                "sha256:d377125165eb6d770f344429a7a55379d4028774aebe267fe620cd1fcd2daab7",
                new List<Digest>
                {
                    new Digest() { Algorithm = "sha256", Hex = "d377125165eb6d770f344429a7a55379d4028774aebe267fe620cd1fcd2daab7" },
                },
            };
            yield return new object[]
            {
                "Output is sha256:d377125165eb6d770f344429a7a55379d4028774aebe267fe620cd1fcd2daab7 and sha256:123425165eb6d770f344429a7a55379d4028774aebe267fe620cd1fcd2daab7",
                new List<Digest>
                {
                    new Digest() { Algorithm = "sha256", Hex = "d377125165eb6d770f344429a7a55379d4028774aebe267fe620cd1fcd2daab7" },
                    new Digest() { Algorithm = "sha256", Hex = "123425165eb6d770f344429a7a55379d4028774aebe267fe620cd1fcd2daab7" },
                },
            };
            yield return new object[]
            {
                "Output is sha128:d377125165eb6d770f344429a7a55376 and sha256:123425165eb6d770f344429a7a55379d4028774aebe267fe620cd1fcd2daab7",
                new List<Digest>
                {
                    new Digest() { Algorithm = "sha128", Hex = "d377125165eb6d770f344429a7a55376" },
                    new Digest() { Algorithm = "sha256", Hex = "123425165eb6d770f344429a7a55379d4028774aebe267fe620cd1fcd2daab7" },
                },
            };
        }

        public static IEnumerable<object[]> GetInputStringContainsNoDigests()
        {
            yield return new object[] { "test" };
            yield return new object[] { "sha256d377125165eb6d770f344429a7a55379d4028774aebe267fe620cd1fcd2daab7" };
            yield return new object[] { "sha256:d377125165eb6d770f" };
        }

        public static IEnumerable<object[]> GetValidDigest()
        {
            yield return new object[] { "sha256:d377125165eb6d770f344429a7a55379d4028774aebe267fe620cd1fcd2daab7" };
            yield return new object[] { "sha256:123425165eb6d770f344429a7a55379d4028774aebe267fe620cd1fcd2daab7" };
            yield return new object[] { "sha128:123425165eb6d770f344429a7a55379d4028774aebe267fe620cd1fcd2daab7" };
        }

        public static IEnumerable<object[]> GetInvalidDigest()
        {
            yield return new object[] { "sha256d377125165eb6d770f344429a7a55379d4028774aebe267fe620cd1fcd2daab7" };
            yield return new object[] { "256:123425165eb6d770f344429a7a55379d4028774aebe267fe620cd1fcd2daab7" };
            yield return new object[] { "sha128:55379d4028774ae:be267fe620cd1fcd2daab7" };
            yield return new object[] { "test" };
        }

        [Theory]
        [MemberData(nameof(GetInputStringContainsDigests))]
        public void GivenAnInputStringContainsDigests_WhenGetDigest_CorrectDigestsWillBeReturned(string input, List<Digest> expectedDigests)
        {
            var results = Digest.GetDigest(input);
            var ind = 0;
            foreach (var result in results)
            {
                Assert.Equal(expectedDigests[ind].Algorithm, result.Algorithm);
                Assert.Equal(expectedDigests[ind].Hex, result.Hex);
                Assert.Equal(string.Concat(result.Algorithm, ":", result.Hex), result.Value);
                ind += 1;
            }
        }

        [Theory]
        [MemberData(nameof(GetInputStringContainsNoDigests))]
        public void GivenAnInputStringWithoutDigests_WhenGetDigest_EmptyResultWillBeReturned(string input)
        {
            var results = Digest.GetDigest(input);
            Assert.Empty(results);
        }

        [Theory]
        [MemberData(nameof(GetValidDigest))]
        public void GivenValidDigest_WhenCheckIsDigest_TrueWillBeReturned(string input)
        {
            Assert.True(Digest.IsDigest(input));
        }

        [Theory]
        [MemberData(nameof(GetInvalidDigest))]
        public void GivenInvalidDigest_WhenCheckIsDigest_FalseWillBeReturned(string input)
        {
            Assert.False(Digest.IsDigest(input));
        }
    }
}
