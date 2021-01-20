// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using Microsoft.Health.Fhir.Liquid.Converter.Cda;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.Cda
{
    public class CdaDataParserTests
    {
        private readonly CdaDataParser _parser = new CdaDataParser();

        public static IEnumerable<object[]> GetNullOrEmptyData()
        {
            yield return new object[] { null };
            yield return new object[] { string.Empty };
        }

        [Theory]
        [MemberData(nameof(GetNullOrEmptyData))]
        public void GivenNullOrEmptyData_WhenParse_CorrectResultShouldBeReturned(string input)
        {
            var exception = Assert.Throws<DataParseException>(() => _parser.Parse(input));
            Assert.Equal(FhirConverterErrorCode.InputParsingError, exception.FhirConverterErrorCode);

            var innerException = exception.InnerException as FhirConverterException;
            Assert.True(innerException is DataParseException);
            Assert.Equal(FhirConverterErrorCode.NullOrEmptyInput, innerException.FhirConverterErrorCode);
        }

        [Fact]
        public void GivenCdaDocument_WhenParse_CorrectResultShouldBeReturned()
        {
            // Valid document
            var document = File.ReadAllText(Path.Join(Constants.SampleDataDirectory, "Cda", "CCD.cda"));
            var data = _parser.Parse(document);
            Assert.NotNull(data);
            Assert.NotNull(((Dictionary<string, object>)data).GetValueOrDefault("msg"));

            // Invalid document
            document = @"<templateId root=""2.16.840.1.113883.10.20.22.1.1""/><templateId root = ""2.16.840.1.113883.10.20.22.1.2""/>";
            var exception = Assert.Throws<DataParseException>(() => _parser.Parse(document));
            Assert.Equal(FhirConverterErrorCode.InputParsingError, exception.FhirConverterErrorCode);
        }
    }
}
