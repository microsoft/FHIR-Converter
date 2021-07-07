// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using Microsoft.Health.Fhir.Liquid.Converter.Ccda;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.Ccda
{
    public class CcdaDataParserTests
    {
        public static IEnumerable<object[]> GetNullOrEmptyCcdaDocument()
        {
            yield return new object[] { null };
            yield return new object[] { string.Empty };
        }

        public static IEnumerable<object[]> GetInvalidCcdaDocument()
        {
            yield return new object[] { "\n" };
            yield return new object[] { "abc" };
            yield return new object[] { @"<templateId root=""2.16.840.1.113883.10.20.22.1.1""/><templateId root = ""2.16.840.1.113883.10.20.22.1.2""/>" };
        }

        [Theory]
        [MemberData(nameof(GetNullOrEmptyCcdaDocument))]
        public void GivenNullOrEmptyData_WhenParse_ExceptionShouldBeThrown(string input)
        {
            var exception = Assert.Throws<DataParseException>(() => CcdaDataParser.Parse(input));
            Assert.Equal(FhirConverterErrorCode.NullOrEmptyInput, exception.FhirConverterErrorCode);
        }

        [Theory]
        [MemberData(nameof(GetInvalidCcdaDocument))]
        public void GivenInvalidCcdaDocument_WhenParse_ExceptionShouldBeThrown(string input)
        {
            var exception = Assert.Throws<DataParseException>(() => CcdaDataParser.Parse(input));
            Assert.Equal(FhirConverterErrorCode.InputParsingError, exception.FhirConverterErrorCode);
        }

        [Fact]
        public void GivenValidCcdaDocument_WhenParse_CorrectResultShouldBeReturned()
        {
            // Sample CCD document
            var document = File.ReadAllText(Path.Join(Constants.SampleDataDirectory, "Ccda", "CCD.ccda"));
            var data = CcdaDataParser.Parse(document);
            Assert.NotNull(data);
            Assert.NotNull(((Dictionary<string, object>)data).GetValueOrDefault("msg"));

            // Document that contains redundant namespaces "xmlns:cda"
            // It is removed in the parsed data
            document = "<ClinicalDocument xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns = \"urn:hl7-org:v3\" xmlns:cda = \"urn:hl7-org:v3\" xmlns:sdtc = \"urn:hl7-org:sdtc\">" +
                       "</ClinicalDocument>";
            data = CcdaDataParser.Parse(document);
            var contents =
                ((data as Dictionary<string, object>)
                ?.GetValueOrDefault("msg") as Dictionary<string, object>)
                ?.GetValueOrDefault("ClinicalDocument") as Dictionary<string, object>;
            Assert.Equal(3, contents?.Count);
            Assert.Equal("http://www.w3.org/2001/XMLSchema-instance", contents?["xmlns:xsi"]);
            Assert.Equal("urn:hl7-org:v3", contents?["xmlns"]);
            Assert.Equal("urn:hl7-org:sdtc", contents?["xmlns:sdtc"]);

            // Document that contains non-default namespace prefix "sdtc" in elements
            // "sdtc:raceCode" is parsed into "sdtc_raceCode"
            document = "<ClinicalDocument xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns = \"urn:hl7-org:v3\" xmlns:sdtc = \"urn:hl7-org:sdtc\">" +
                       "<sdtc:raceCode code=\"2076-8\" displayName=\"Hawaiian or Other Pacific Islander\" codeSystem=\"2.16.840.1.113883.6.238\" codeSystemName=\"Race &amp; Ethnicity - CDC\"/>" +
                       "</ClinicalDocument>";
            data = CcdaDataParser.Parse(document);
            contents =
                ((data as Dictionary<string, object>)
                    ?.GetValueOrDefault("msg") as Dictionary<string, object>)
                ?.GetValueOrDefault("ClinicalDocument") as Dictionary<string, object>;
            Assert.NotNull(contents?["sdtc_raceCode"]);
        }
    }
}
