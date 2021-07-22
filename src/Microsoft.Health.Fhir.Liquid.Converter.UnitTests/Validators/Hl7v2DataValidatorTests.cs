// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Validators;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.Validators
{
    public class Hl7v2DataValidatorTests
    {
        private Hl7v2DataValidator _validator = new Hl7v2DataValidator();

        public static IEnumerable<object[]> GetIncompleteHeader()
        {
            yield return new object[] { @"M" };
            yield return new object[] { @"abc" };
            yield return new object[] { @"MSQ|||" };
        }

        [Theory]
        [MemberData(nameof(GetIncompleteHeader))]
        public void GivenIncompleteHeader_WhenParse_ExceptionsShouldBeThrown(string input)
        {
            var exception = Assert.Throws<DataParseException>(() => _validator.ValidateMessageHeader(input));
            var segmentId = input.Length < 3 ? input : input.Substring(0, 3);
            Assert.Equal($"The HL7 v2 message is invalid, first segment id = {segmentId}.", exception.Message);
        }

        [Fact]
        public void GivenIncompleteSeparators_WhenParse_ExceptionsShouldBeThrown()
        {
            var exception = Assert.Throws<DataParseException>(() => _validator.ValidateMessageHeader(@"MSH|||"));
            Assert.Equal("MSH segment misses separators.", exception.Message);
        }

        [Fact]
        public void GivenDuplicateSeparators_WhenParse_ExceptionsShouldBeThrown()
        {
            var exception = Assert.Throws<DataParseException>(() => _validator.ValidateMessageHeader(@"MSH|^~^#|"));
            Assert.Equal("MSH segment contains duplicate separators.", exception.Message);
        }

        [Fact]
        public void GivenInvalidEsacpeCharacter_WhenParse_ExceptionsShouldBeThrown()
        {
            var exception = Assert.Throws<DataParseException>(() => _validator.ValidateMessageHeader(@"MSH|^~#&|NES|NINTENDO|"));
            Assert.Equal("Escape character should be backslash.", exception.Message);
        }
    }
}
