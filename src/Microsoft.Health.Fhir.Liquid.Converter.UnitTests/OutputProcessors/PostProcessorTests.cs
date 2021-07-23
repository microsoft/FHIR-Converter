// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.OutputProcessors;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.OutputProcessors
{
    public class PostProcessorTests
    {
        public static IEnumerable<object[]> GetValidDataForParseJson()
        {
            // Simple parse with no error
            yield return new object[] { @"{""a"":[""b"", ""c""]}", @"{""a"":[""b"",""c""]}" };

            // Extra comma in object
            yield return new object[] { @"{,""a"":""b"",,""c"":""d"",}", @"{""a"":""b"",""c"":""d""}" };

            // Extra comma in array
            yield return new object[] { @"{""c"":[,""d"",,,""e"",,]}", @"{""c"":[""d"",""e""]}" };

            // Empty array and obj entries
            yield return new object[] { @"{""c"":[,"""",]}", @"{}" };

            // 2 empty arrays
            yield return new object[] { @"{""a"":[,],""b"":[,]}", @"{}" };

            // 2 empty objs
            yield return new object[] { @"{""a"":{,},""b"":{,}}", @"{}" };

            // Empty value
            yield return new object[] { @"{""a"":"""",""b"":""d""}", @"{""b"":""d""}" };

            // Commas and curly brace part of string
            yield return new object[] { @"{""a"":""b,,}""}", @"{""a"":""b,,}""}" };

            // Quotes inside string
            yield return new object[] { @"{""a"":""\""b""}", @"{""a"":""\""b""}" };

            // Should remove empty fields in a complex input
            yield return new object[] { @"{""b"":[""c"",],""d"":{,},}", @"{""b"":[""c""]}" };
        }

        public static IEnumerable<object[]> GetInvalidDataForParseJson()
        {
            // Corrupted data test
            yield return new object[] { @"{""a"":""b}" };

            // Missing comma between pairs
            yield return new object[] { @"{""a"":""1""""b"":""2""}" };

            // Missing comma between pairs with first value empty
            yield return new object[] { @"{""a"":""""""b"":""2""}" };

            // Missing value in middle pair
            yield return new object[] { @"{""a"":""1"",""b"":,""c"":""3""}" };

            // Missing value and comma in middle pair
            yield return new object[] { @"{""a"":""1"",""b"":""c"":""3""}" };

            // Only value in middle pair
            yield return new object[] { @"{""a"":""1"",""b"",""c"":""3""}" };

            // Middle pair with incorrect value obj
            yield return new object[] { @"{""a"":""1"",""b"":{""c""},""d"":""4""}" };

            // Missing comma in array
            yield return new object[] { @"{""a"":[123true]}" };

            // Missing quote in value
            yield return new object[] { @"{""id"":123abc""}" };

            // Wrong value
            yield return new object[] { @"{""id"":abc}" };
        }

        public static IEnumerable<object[]> GetDataForMergeJson()
        {
            // Add property
            yield return new object[]
            {
                JObject.Parse(@"{""entry"":[
                {""resource"":{""resourceType"":""Patient"",""a"":""1"",""meta"":{""dummy"":""true""}}},
                {""resource"":{""resourceType"":""Patient"",""b"":""2""}}]}"),
                JObject.Parse(@"{""entry"":[{""resource"":{""resourceType"":""Patient"",""a"":""1"",""meta"":{""dummy"":""true""},""b"":""2""}}]}"),
            };

            // Invalid input returned as is
            yield return new object[]
            {
                JObject.Parse(@"{""entry"":[{""a"":""b""}]}"),
                JObject.Parse(@"{""entry"":[{""a"":""b""}]}"),
            };

            // Invalid input returned as is
            yield return new object[]
            {
                JObject.Parse(@"{""a"":""b""}"),
                JObject.Parse(@"{""a"":""b""}"),
            };

            // Last update wins
            yield return new object[]
            {
                JObject.Parse(@"{""entry"":[
                {""resource"":{""resourceType"":""Patient"",""a"":""1""}},
                {""resource"":{""resourceType"":""Patient"",""a"":""2""}}]}"),
                JObject.Parse(@"{""entry"":[{""resource"":{""resourceType"":""Patient"",""a"":""2""}}]}"),
            };

            // Array concatenation
            yield return new object[]
            {
                JObject.Parse(@"{""entry"":[
                {""resource"":{""resourceType"":""Patient"",""a"":[""1"",""2""]}},
                {""resource"":{""resourceType"":""Patient"",""a"":[""2"",""3""]}}]}"),
                JObject.Parse(@"{""entry"":[{""resource"":{""resourceType"":""Patient"",""a"":[""1"",""2"",""3""]}}]}"),
            };

            // Additional resource if different id
            yield return new object[]
            {
                JObject.Parse(@"{""entry"":[
                {""resource"":{""resourceType"":""Patient"",""id"":""1"",""a"":""1""}},
                {""resource"":{""resourceType"":""Patient"",""id"":""1"",""a"":""2""}},
                {""resource"":{""resourceType"":""Patient"",""a"":""3""}}]}"),
                JObject.Parse(@"{""entry"":[
                {""resource"":{""resourceType"":""Patient"",""id"":""1"",""a"":""2""}},
                {""resource"":{""resourceType"":""Patient"",""a"":""3""}}]}"),
            };

            // Merging only for same resource type
            yield return new object[]
            {
                JObject.Parse(@"{""entry"":[
                {""resource"":{""resourceType"":""Patient"", ""a"":""1""}},
                {""resource"":{""resourceType"":""Observation"",""b"":""2""}}]}"),
                JObject.Parse(@"{""entry"":[
                {""resource"":{""resourceType"":""Patient"",""a"":""1""}},
                {""resource"":{""resourceType"":""Observation"",""b"":""2""}}]}"),
            };

            // Two resources with matching type, id and versionId
            yield return new object[]
            {
                JObject.Parse(@"{""entry"":[
                {""resource"":{""resourceType"":""Patient"",""id"":""1"",""meta"":{""versionId"":""1""}}},
                {""resource"":{""resourceType"":""Patient"",""id"":""1"",""meta"":{""versionId"":""1""}}}]}"),
                JObject.Parse(@"{""entry"":[{""resource"":{""resourceType"":""Patient"",""id"":""1"",""meta"":{""versionId"":""1""}}}]}"),
            };

            // Two resources with matching type and id but non-matching versionId
            yield return new object[]
            {
                JObject.Parse(@"{""entry"":[
                {""resource"":{""resourceType"":""Patient"",""id"":""1"",""meta"":{""versionId"":""1""}}},
                {""resource"":{""resourceType"":""Patient"",""id"":""1"",""meta"":{""versionId"":""2""}}}]}"),
                JObject.Parse(@"{""entry"":[
                {""resource"":{""resourceType"":""Patient"",""id"":""1"",""meta"":{""versionId"":""1""}}},
                {""resource"":{""resourceType"":""Patient"",""id"":""1"",""meta"":{""versionId"":""2""}}}]}"),
            };
        }

        [Theory]
        [MemberData(nameof(GetValidDataForParseJson))]
        public void GivenValidData_WhenParsing_ValidDataShouldBeParsed(string input, string expected)
        {
            string result = PostProcessor.ParseJson(input).ToString(Formatting.None);
            Assert.Equal(expected, result);
        }

        [Theory]
        [MemberData(nameof(GetInvalidDataForParseJson))]
        public void GivenInvalidData_WhenParsing_ValidDataShouldBeParsed(string input)
        {
            var exception = Assert.Throws<PostprocessException>(() => PostProcessor.ParseJson(input));
            Assert.Equal(FhirConverterErrorCode.JsonParsingError, exception.FhirConverterErrorCode);
        }

        [Theory]
        [MemberData(nameof(GetDataForMergeJson))]
        public void GivenTwoJObjects_WhenMerge_MergedJObjectShouldBeReturned(JObject obj, JObject expectedObject)
        {
            var resultObject = PostProcessor.MergeJson(obj);
            var result = JsonConvert.SerializeObject(resultObject);
            var expected = JsonConvert.SerializeObject(expectedObject);
            Assert.True(string.Equals(result, expected, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
