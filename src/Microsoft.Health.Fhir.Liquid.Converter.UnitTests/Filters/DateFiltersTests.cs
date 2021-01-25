// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.FilterTests
{
    public class DateFiltersTests
    {
        public static IEnumerable<object[]> GetValidDataForAddHyphensDate()
        {
            yield return new object[] { null, null };
            yield return new object[] { string.Empty, string.Empty };
            yield return new object[] { @"2001", @"2001" };
            yield return new object[] { @"200101", @"2001-01" };
            yield return new object[] { @"19241010", @"1924-10-10" };
            yield return new object[] { @"19850101000000", @"1985-01-01" };
        }

        public static IEnumerable<object[]> GetValidDataForFormatAsDateTime()
        {
            yield return new object[] { null, null };
            yield return new object[] { string.Empty, string.Empty };
            yield return new object[] { @"2001", @"2001" };
            yield return new object[] { @"200101", @"2001-01" };
            yield return new object[] { @"20050110045253", @"2005-01-10T04:52:53Z" };
            yield return new object[] { @"20110103143428-0800", @"2011-01-03T14:34:28-08:00" };
            yield return new object[] { @"19701231115959+0600", @"1970-12-31T11:59:59+06:00" };
        }

        public static IEnumerable<object[]> GetInvalidDataForAddHyphensDate()
        {
            yield return new object[] { @"20badInput" };
            yield return new object[] { @"2020-11" };
            yield return new object[] { @"20201" };
            yield return new object[] { @"2020060" };
            yield return new object[] { @"20201301" };
            yield return new object[] { @"20200134" };
            yield return new object[] { @"20200230" };
        }

        public static IEnumerable<object[]> GetInvalidDataForFormatAsDateTime()
        {
            yield return new object[] { @"20badInput" };
            yield return new object[] { @"2020-11" };
            yield return new object[] { @"20140130080051--0500" };
            yield return new object[] { @"2014.051-0500" };
            yield return new object[] { @"20140130080051123+0500" };
            yield return new object[] { @"20201" };
            yield return new object[] { @"2020060" };
            yield return new object[] { @"20201301" };
            yield return new object[] { @"20200134" };
            yield return new object[] { @"20200230" };
            yield return new object[] { @"2020010130" };
            yield return new object[] { @"202001011080" };
            yield return new object[] { @"20200101101080" };
        }

        [Theory]
        [MemberData(nameof(GetValidDataForAddHyphensDate))]
        public void GivenAnHl7v2Date_WhenAddHyphensDate_ConvertedDateShouldBeReturned(string input, string expected)
        {
            var result = Filters.AddHyphensDate(input);
            Assert.Equal(expected, result);
        }

        [Theory]
        [MemberData(nameof(GetValidDataForFormatAsDateTime))]
        public void GivenAnHl7v2DateTime_WhenFormatAsDateTime_ConvertedDateShouldBeReturned(string input, string expected)
        {
            var result = Filters.FormatAsDateTime(input);
            Assert.Equal(expected, result);
        }

        [Theory]
        [MemberData(nameof(GetInvalidDataForAddHyphensDate))]
        public void GivenAnInvalidHl7v2DateTime_WhenAddHyphensDate_ExceptionShouldBeThrown(string input)
        {
            Assert.Throws<RenderException>(() => Filters.AddHyphensDate(input));
        }

        [Theory]
        [MemberData(nameof(GetInvalidDataForFormatAsDateTime))]
        public void GivenAnInvalidHl7v2DateTime_WhenFormatAsDateTime_ExceptionShouldBeThrown(string input)
        {
            Assert.Throws<RenderException>(() => Filters.FormatAsDateTime(input));
        }
    }
}
