﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
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
            // TimeZoneHandling does not affect dateTime without time
            yield return new object[] { null, "preserve", null };
            yield return new object[] { null, "utc", null };
            yield return new object[] { null, "local", null };
            yield return new object[] { string.Empty, "preserve", string.Empty };
            yield return new object[] { string.Empty, "utc", string.Empty };
            yield return new object[] { string.Empty, "local", string.Empty };
            yield return new object[] { @"2001", "preserve", @"2001" };
            yield return new object[] { @"2001", "utc", @"2001" };
            yield return new object[] { @"2001", "local", @"2001" };
            yield return new object[] { @"200101", "preserve", @"2001-01" };
            yield return new object[] { @"200101", "utc", @"2001-01" };
            yield return new object[] { @"200101", "local", @"2001-01" };

            // If no time zone provided, it is treated as local
            yield return new object[] { @"20050110045253", "preserve", @"2005-01-10T04:52:53" };
            yield return new object[] { @"20050110045253", "local", @"2005-01-10T04:52:53" };

            // If time zone provided, it should be formatted according to TimeZoneHandling
            yield return new object[] { @"20110103143428-0800", "preserve", @"2011-01-03T14:34:28-08:00" };
            yield return new object[] { @"20110103143428-0800", "utc", @"2011-01-03T22:34:28Z" };
            yield return new object[] { @"19701231115959+0600", "preserve", @"1970-12-31T11:59:59+06:00" };
            yield return new object[] { @"19701231115959+0600", "utc", @"1970-12-31T05:59:59Z" };

            // Skip this test in pipeline, as the local time zone is different
            // yield return new object[] { @"20050110045253", "utc", @"2005-01-09T20:52:53Z" };
            // yield return new object[] { @"20110103143428-0800", "local", @"2011-01-04T06:34:28+08:00" };
            // yield return new object[] { @"19701231115959+0600", "local", @"1970-12-31T13:59:59+08:00" };
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

        public static IEnumerable<object[]> GetInvalidTimeZoneHandling()
        {
            yield return new object[] { @"20050110045253", null };
            yield return new object[] { @"20110103143428-0800", string.Empty };
            yield return new object[] { @"19701231115959+0600", "abc" };
        }

        [Theory]
        [MemberData(nameof(GetValidDataForAddHyphensDate))]
        public void GivenADate_WhenAddHyphensDate_ConvertedDateShouldBeReturned(string input, string expected)
        {
            var result = Filters.AddHyphensDate(input);
            Assert.Equal(expected, result);
        }

        [Theory]
        [MemberData(nameof(GetValidDataForFormatAsDateTime))]
        public void GivenADateTime_WhenFormatAsDateTime_ConvertedDateShouldBeReturned(string input, string timeZoneHandling, string expected)
        {
            var result = Filters.FormatAsDateTime(input, timeZoneHandling);
            Assert.Equal(expected, result);
        }

        [Theory]
        [MemberData(nameof(GetInvalidDataForAddHyphensDate))]
        public void GivenAnInvalidDateTime_WhenAddHyphensDate_ExceptionShouldBeThrown(string input)
        {
            var exception = Assert.Throws<RenderException>(() => Filters.AddHyphensDate(input));
            Assert.Equal(FhirConverterErrorCode.InvalidDateTimeFormat, exception.FhirConverterErrorCode);
        }

        [Theory]
        [MemberData(nameof(GetInvalidDataForFormatAsDateTime))]
        public void GivenAnInvalidDateTime_WhenFormatAsDateTime_ExceptionShouldBeThrown(string input)
        {
            var exception = Assert.Throws<RenderException>(() => Filters.FormatAsDateTime(input));
            Assert.Equal(FhirConverterErrorCode.InvalidDateTimeFormat, exception.FhirConverterErrorCode);
        }

        [Theory]
        [MemberData(nameof(GetInvalidTimeZoneHandling))]
        public void GivenAnInvalidTimeZoneHandling_WhenFormatAsDateTime_ExceptionShouldBeThrown(string input, string timeZoneHandling)
        {
            var exception = Assert.Throws<RenderException>(() => Filters.FormatAsDateTime(input, timeZoneHandling));
            Assert.Equal(FhirConverterErrorCode.InvalidTimeZoneHandling, exception.FhirConverterErrorCode);
        }

        [Fact]
        public void NowTest()
        {
            // FHIR DateTime format
            var dateTime = DateTime.Parse(Filters.Now(string.Empty));
            Assert.True(dateTime.Year > 2020);
            Assert.True(dateTime.Month >= 1 && dateTime.Month < 13);
            Assert.True(dateTime.Day >= 1 && dateTime.Day < 32);

            // Standard DateTime format, "d" stands for short day pattern
            var nowWithStandardFormat = Filters.Now(string.Empty, "d");
            Assert.Contains("/", nowWithStandardFormat);

            // Customized DateTime format
            var days = new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
            var nowWithCustomizedFormat = Filters.Now(string.Empty, "dddd, dd MMMM yyyy HH:mm:ss");
            Assert.Contains(days, day => nowWithCustomizedFormat.StartsWith(day));

            // Null and empty format will lead to default format, which is short day with long time
            dateTime = DateTime.Parse(Filters.Now(string.Empty, null));
            Assert.True(dateTime.Year > 2020);
            Assert.True(dateTime.Month >= 1 && dateTime.Month < 13);
            Assert.True(dateTime.Day >= 1 && dateTime.Day < 32);
            dateTime = DateTime.Parse(Filters.Now(string.Empty, string.Empty));
            Assert.True(dateTime.Year > 2020);
            Assert.True(dateTime.Month >= 1 && dateTime.Month < 13);
            Assert.True(dateTime.Day >= 1 && dateTime.Day < 32);

            // Invalid DateTime format
            Assert.Throws<FormatException>(() => Filters.Now(string.Empty, "a"));
        }
    }
}
