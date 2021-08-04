// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.InputProcessors;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Models.Hl7v2;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.InputProcessors
{
    public class Hl7v2EscapeSequenceProcessorTests
    {
        private static readonly Hl7v2EncodingCharacters EncodingCharacters = new Hl7v2EncodingCharacters
        {
            FieldSeparator = 'A',
            ComponentSeparator = 'B',
            SubcomponentSeparator = 'C',
            RepetitionSeparator = 'D',
        };

        public static IEnumerable<object[]> GetDataForUnescape()
        {
            yield return new object[] { @"\F\n", EncodingCharacters.FieldSeparator.ToString() + "n" };
            yield return new object[] { @"\S\n", EncodingCharacters.ComponentSeparator.ToString() + "n" };
            yield return new object[] { @"\T\n", EncodingCharacters.SubcomponentSeparator.ToString() + "n" };
            yield return new object[] { @"\R\n", EncodingCharacters.RepetitionSeparator.ToString() + "n" };
            yield return new object[] { @"\E\n", @"\n" };
            yield return new object[] { @"\.br\n", @"\nn" };
            yield return new object[] { @"\X6566\n", @"efn" };
            yield return new object[] { @"\X\n", @"\X\n" };
        }

        [Theory]
        [MemberData(nameof(GetDataForUnescape))]
        public void GivenAString_WhenUnescape_UnescapedStringShouldBeReturned(string input, string expected)
        {
            var result = Hl7v2EscapeSequenceProcessor.Unescape(input, EncodingCharacters);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GivenAnInvalidHexadecimalNumber_WhenUnescape_ExceptionsShouldBeThrown()
        {
            var exception = Assert.Throws<DataParseException>(() => Hl7v2EscapeSequenceProcessor.Unescape(@"\X656\n", EncodingCharacters));
            Assert.Equal(FhirConverterErrorCode.InvalidHexadecimalNumber, exception.FhirConverterErrorCode);
            Assert.Equal("The hexadecimal number is invalid: 656.", exception.Message);
        }
    }
}
