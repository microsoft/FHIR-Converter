// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Parsers;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.Parsers
{
    public class JsonDataParserTests
    {
        private readonly IDataParser _parser = new JsonDataParser();

        public static IEnumerable<object[]> GetNullOrEmptyJsonData()
        {
            yield return new object[] { null };
            yield return new object[] { string.Empty };
            yield return new object[] { " " };
            yield return new object[] { "\n" };
        }

        public static IEnumerable<object[]> GetInvalidJsonDataDueToJsonReaderException()
        {
            yield return new object[] { "abc" };
            yield return new object[] { "}" };
            yield return new object[] { "]" };

            // Invalid JSON strings
            yield return new object[] { @"{""value"":""I say ""hello!"""" ]" };

            // Invalid JSON numbers
            yield return new object[] { @"{""value"": 99 * 0.6 ]" };

            // Invalid JSON arrays
            yield return new object[] { @"[""a""}" };
            yield return new object[] { @"[""a"":""b""]" };
            yield return new object[] { @"[a, b, c]" };

            // Invalid JSON objects
            yield return new object[] { @"{{}}" };
            yield return new object[] { @"{""a""}" };
            yield return new object[] { @"{""a"":}" };
            yield return new object[] { @"{""a"":""b""]" };
        }

        public static IEnumerable<object[]> GetInvalidJsonDataDueToJsonSerializationException()
        {
            yield return new object[] { "{" };
        }

        public static IEnumerable<object[]> GetValidJsonData()
        {
            yield return new object[] { "{}" };
            yield return new object[] { "[]" };

            // Trailing commas are OK
            yield return new object[] { @"{""a"":""b"",}" };
            yield return new object[] { @"[""a"",]" };

            yield return new object[] { @"{""a"":[""b"",""c""] }" };
            yield return new object[] { @"[{""a"":""b""}, {""c"":""d""}]" };
        }

        [Theory]
        [MemberData(nameof(GetNullOrEmptyJsonData))]
        public void GivenNullOrEmptyData_WhenParse_ExceptionShouldBeThrown(string input)
        {
            var exception = Assert.Throws<DataParseException>(() => _parser.Parse(input));
            Assert.Equal(FhirConverterErrorCode.NullOrWhiteSpaceInput, exception.FhirConverterErrorCode);
        }

        [Theory]
        [MemberData(nameof(GetInvalidJsonDataDueToJsonReaderException))]
        public void GivenInvalidJsonDataDueToJsonReaderException_WhenParse_ExceptionShouldBeThrown(string input)
        {
            var exception = Assert.Throws<DataParseException>(() => _parser.Parse(input));
            Assert.Equal(FhirConverterErrorCode.InputParsingError, exception.FhirConverterErrorCode);

            // Verify exception is thrown by default JsonConverter as well
            Assert.Throws<JsonReaderException>(() => JsonConvert.DeserializeObject(input));
        }

        [Theory]
        [MemberData(nameof(GetInvalidJsonDataDueToJsonSerializationException))]
        public void GivenInvalidJsonDataDueToJsonSerializationException_WhenParse_ExceptionShouldBeThrown(string input)
        {
            var exception = Assert.Throws<DataParseException>(() => _parser.Parse(input));
            Assert.Equal(FhirConverterErrorCode.InputParsingError, exception.FhirConverterErrorCode);

            // Verify exception is thrown by default JsonConverter as well
            Assert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject(input));
        }

        [Theory]
        [MemberData(nameof(GetValidJsonData))]
        public void GivenValidJsonData_WhenParse_DataShouldBeParsedCorrectly(string input)
        {
            var data = _parser.Parse(input);
            Assert.NotNull(data);

            // Verify data is parsed correctly by default JsonConverter as well
            Assert.NotNull(JsonConvert.DeserializeObject(input));
        }
    }
}
