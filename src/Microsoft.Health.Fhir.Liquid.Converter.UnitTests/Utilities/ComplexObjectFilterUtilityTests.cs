// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using Microsoft.Health.Fhir.Liquid.Converter.Extensions;
using Microsoft.Health.Fhir.Liquid.Converter.Utilities;
using Newtonsoft.Json.Linq;
using Xunit;

using TestCase = Microsoft.Health.Fhir.Liquid.Converter.UnitTests.Models.ComplexObjectFilterUtilityTestCase;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.Utilities
{
    public class ComplexObjectFilterUtilityTests
    {
        [Fact]
        public void GivenObjectAndPath_SelectCorrectObjectsFromArray()
        {
            var testContent = File.ReadAllText("./TestData/ComplexObject.json");
            var testData = (object[])JToken.Parse(testContent).ToObject();
            var tests = new TestCase[]
            {
                new TestCase(testData, "age", new string[] { "27" }, new object[] { testData[0] }),
                new TestCase(testData, "upgrades.vision.system", new string[] { "http://infrared.org/other" }, new object[] { testData[6] }),
                new TestCase(testData, "friends[0].name", new string[] { "Holland Ochoa" }, new object[] { testData[3] }),
                new TestCase(testData, "friends[0].parents[1].tags[].nothere", new string[] { }, new object[] { }),
                new TestCase(testData, "friends[].parents[].tags[2]", new string[] { "ex" }, new object[] { testData[3] }),
                new TestCase(testData, "friends[].parents[].tags[2]", new string[] { "hex" }, new object[] { }),
                new TestCase(testData, "upgrades.bonemarrow.system", new string[] { "http://illuminated.party" }, testData),
                new TestCase(testData, "friends[0].parents[1].tags[]", new string[] { "magna" }, new object[] { testData[6] }),
                new TestCase(testData, "friends[0].parents[1].name", new string[] { "Harry Potter" }, new object[] { }),
                new TestCase(testData, "friends[0].parents[1].tags[]", new string[] { }, testData),
                new TestCase(testData, "age", new string[] { "27" }, new object[] { testData[0] }),
                new TestCase(testData, "age", new string[] { "27", "40" }, new object[] { testData[0], testData[1], testData[5] }),
                new TestCase(testData, "upgrades.*.augmentCode", new string[] { "chameleon" }, new object[] { testData[0], testData[1], testData[4], testData[5] }),
                new TestCase(testData, "upgrades.vision.*", new string[] { "strawberry" }, new object[] { testData[1], testData[2], testData[3], testData[5] }),
            };

            foreach (TestCase testCase in tests)
            {
                var expected = testCase.Expected;
                var actualData = ComplexObjectFilterUtility.Select(testCase.Input, testCase.Path, testCase.Values);
                Assert.Equal(expected, actualData);
            }
        }

        [Theory]
        [InlineData("test.path[].to[5].value", new string[] { "test", "path", "[]", "to", "[5]", "value" })]
        [InlineData("test",                    new string[] { "test" })]
        [InlineData("test.path",               new string[] { "test", "path" })]
        [InlineData("[].[5].test.path[5]",     new string[] { "[]", "[5]", "test", "path", "[5]" })]
        [InlineData("[5]",                     new string[] { "[5]" })]
        [InlineData("[]",                      new string[] { "[]" })]
        [InlineData("[559]",                   new string[] { "[559]" })]
        [InlineData("data.*.hello",            new string[] { "data", "*", "hello" })]
        public void GivenPath_WhenSplitToParts_CorrectPartsShouldBeReturned(string path, string[] expected)
        {
            var expectedQueue = new Queue<string>(expected);
            var actual = ComplexObjectFilterUtility.SplitObjectPath(path);
            Assert.Equal(expectedQueue, actual);
        }
    }
}
