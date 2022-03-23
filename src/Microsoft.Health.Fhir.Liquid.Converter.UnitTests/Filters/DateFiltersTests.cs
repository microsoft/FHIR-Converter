// -------------------------------------------------------------------------------------------------
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
            yield return new object[] { null, "local", null };
            yield return new object[] { string.Empty, "local", string.Empty };
            yield return new object[] { @"2001", "preserve", @"2001" };
            yield return new object[] { @"200101", "preserve", @"2001-01" };
            yield return new object[] { @"19241010", "local", @"1924-10-10" };
            yield return new object[] { @"19850101000000", "local", @"1985-01-01" };
        }

        // We assume the local timezone is +08:00.
        public static IEnumerable<object[]> GetValidDataWithoutTimeZoneForAddHyphensDateWithUtcTimeZoneHandling()
        {
            yield return new object[] { @"200101", "utc", new DateTime(2001, 1, 1) };
            yield return new object[] { @"20010102", "utc", new DateTime(2001, 1, 2) };
            yield return new object[] { @"19880101000000", "utc", new DateTime(1988, 1, 1, 0, 0, 0) };
        }

        public static IEnumerable<object[]> GetValidDataForAddHyphensDateWithDefaultTimeZoneHandling()
        {
            yield return new object[] { null, null };
            yield return new object[] { string.Empty, string.Empty };
            yield return new object[] { @"2001", @"2001" };
            yield return new object[] { @"200101", @"2001-01" };
            yield return new object[] { @"19241010", @"1924-10-10" };
            yield return new object[] { @"19850101000000", @"1985-01-01" };
        }

        public static IEnumerable<object[]> GetValidDataForAddSeconds()
        {
            yield return new object[] { null, 60, "local", null };
            yield return new object[] { string.Empty, 60, "local", string.Empty };
            yield return new object[] { @"1970-01-01T00:01:00.000+10:00", -60, "utc", @"1969-12-31T14:00:00.000Z" };
            yield return new object[] { @"1970-01-01T00:01:00.000+05:30", -60, "utc", @"1969-12-31T18:30:00.000Z" };
            yield return new object[] { @"1970-01-01T00:01:00Z", 60.123, "preserve", @"1970-01-01T00:02:00.123Z" };
            yield return new object[] { @"1970-01-01T00:01:00+06:00", 60.000, "preserve", @"1970-01-01T00:02:00+06:00" };
            yield return new object[] { @"1970-01-01T00:01:00+06:30", 60.000, "preserve", @"1970-01-01T00:02:00+06:30" };
            yield return new object[] { @"1970-01-01T00:01:00+14:00", 60, "utc", @"1969-12-31T10:02:00Z" };

            // Skip this test in pipeline, as the local time zone is different
            // yield return new object[] { @"2001-01", 60, "preserve", @"2001-01-01T00:01:00+08:00" };
            // yield return new object[] { @"1924-10-10", 60000, "utc", @"1924-10-10T08:40:00Z" };
            // yield return new object[] { @"1970-01-01T00:01:00+06:00", 60, "local", @"1970-01-01T02:02:00+08:00" };
            // yield return new object[] { @"1924-10-10", 60000, "local", @"1924-10-10T16:40:00" };
        }

        // We assume the local timezone is +08:00.
        public static IEnumerable<object[]> GetValidDataWithoutTimeZoneForAddSecondsWithUtcTimeZoneHandling()
        {
            yield return new object[] { @"1988-10-10", 60000, "utc", @"1988-10-10T08:40:00Z", new DateTime(1988, 10, 10) };
            yield return new object[] { @"1970-01-01", 60, "utc", @"1969-12-31T16:01:00Z", new DateTime(1970, 1, 1) };
        }

        public static IEnumerable<object[]> GetValidDataForAddSecondsWithDefaultTimeZoneHandling()
        {
            yield return new object[] { null, 60, null };
            yield return new object[] { string.Empty, 60, string.Empty };
            yield return new object[] { @"1970-01-01T00:01:00Z", 60.123, @"1970-01-01T00:02:00.123Z" };
            yield return new object[] { @"1970-01-01T00:01:00+06:00", 60.000, @"1970-01-01T00:02:00+06:00" };

            // Skip this test in pipeline, as the local time zone is different
            // yield return new object[] { @"2001-01", 60, @"2001-01-01T00:01:00+08:00" };
            // yield return new object[] { @"1924-10-10", 60000, @"1924-10-10T16:40:00+08:00" };
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
            yield return new object[] { @"2001", "local", @"2001" };
            yield return new object[] { @"200101", "preserve", @"2001-01" };
            yield return new object[] { @"200101", "local", @"2001-01" };

            // If time zone provided, it should be formatted according to TimeZoneHandling
            yield return new object[] { @"20110103143428-0800", "preserve", @"2011-01-03T14:34:28-08:00" };
            yield return new object[] { @"20110103143428-0845", "preserve", @"2011-01-03T14:34:28-08:45" };
            yield return new object[] { @"20110103143428-0800", "utc", @"2011-01-03T22:34:28Z" };
            yield return new object[] { @"20110103143428-0845", "utc", @"2011-01-03T23:19:28Z" };

            yield return new object[] { @"19701231115959+0600", "preserve", @"1970-12-31T11:59:59+06:00" };
            yield return new object[] { @"19701231115959+0600", "utc", @"1970-12-31T05:59:59Z" };
            yield return new object[] { @"19701231115959+0630", "utc", @"1970-12-31T05:29:59Z" };

            // Skip this test in pipeline, as the local time zone is different
            // yield return new object[] { @"20110103143428-0800", "local", @"2011-01-04T06:34:28+08:00" };
            // yield return new object[] { @"19701231115959+0600", "local", @"1970-12-31T13:59:59+08:00" };

            // If no time zone provided, it is treated as local
            // yield return new object[] { @"2001", "utc", @"2000" };
            // yield return new object[] { @"20050110045253", "utc", @"2005-01-09T20:52:53Z" };
            // yield return new object[] { @"20050110045253", "preserve", @"2005-01-10T04:52:53+08:00" };
            // yield return new object[] { @"20050110045253", "local", @"2005-01-10T04:52:53+08:00" };
        }

        // We assume the local timezone is +08:00.
        public static IEnumerable<object[]> GetValidDataWithoutTimeZoneForFormatAsDateTimeWithUtcTimeZoneHandling()
        {
            yield return new object[] { @"20050110045253", "utc", @"2005-01-09T20:52:53Z", new DateTime(2005, 1, 10, 4, 52, 53) };
            yield return new object[] { @"19880103143428", "utc", @"1988-01-03T06:34:28Z", new DateTime(1988, 1, 3, 14, 34, 28) };
            yield return new object[] { @"19701231115959", "utc", @"1970-12-31T03:59:59Z", new DateTime(1970, 12, 31, 11, 59, 59) };
        }

        public static IEnumerable<object[]> GetValidDataForFormatAsDateTimeWithDefaultTimeZoneHandling()
        {
            yield return new object[] { @"200101", @"2001-01" };
            yield return new object[] { @"20110103143428-0800", @"2011-01-03T14:34:28-08:00" };
            yield return new object[] { @"19701231115959+0600", @"1970-12-31T11:59:59+06:00" };
            yield return new object[] { @"19701231115959+0600", @"1970-12-31T11:59:59+06:00" };
            yield return new object[] { @"19701231115959+0000", @"1970-12-31T11:59:59Z" };
            yield return new object[] { @"19701231115959-0000", @"1970-12-31T11:59:59Z" };

            // If no time zone provided, it is treated as local
            // yield return new object[] { @"20050110045253", @"2005-01-10T04:52:53+08:00" };
            // yield return new object[] { @"20110103143428", @"2011-01-03T14:34:28+08:00" };
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

        public static IEnumerable<object[]> GetInvalidDataForAddSeconds()
        {
            yield return new object[] { @"20badInput" };
            yield return new object[] { @"20140130080051--0500" };
            yield return new object[] { @"2014.051-0500" };
            yield return new object[] { @"20140130080051123+0500" };
            yield return new object[] { @"20201" };
            yield return new object[] { @"2020060" };
            yield return new object[] { @"1970-01-01T00:01:00" };
            yield return new object[] { @"1970-01-01T00:01" };
            yield return new object[] { @"2001-01T" };
        }

        [Theory]
        [MemberData(nameof(GetValidDataForAddSeconds))]
        public void GivenSeconds_WhenAddOnValidDateTime_CorrectDateTimeStringShouldBeReturned(string originalDateTime, double seconds, string timeZoneHandling, string expectedDateTime)
        {
            var result = Filters.AddSeconds(originalDateTime, seconds, timeZoneHandling);
            Assert.Equal(expectedDateTime, result);
        }

        [Theory]
        [MemberData(nameof(GetValidDataWithoutTimeZoneForAddSecondsWithUtcTimeZoneHandling))]
        public void GivenSeconds_WhenAddOnValidDataWithoutTimeZone_CorrectDateTimeShouldBeReturned(string originalDateTime, double seconds, string timeZoneHandling, string expectedDateTime, DateTime inputDateTime)
        {
            var result = Filters.AddSeconds(originalDateTime, seconds, timeZoneHandling);
            var dateTimeOffset = DateTimeOffset.Parse(result);
            dateTimeOffset = dateTimeOffset.AddHours(TimeZoneInfo.Local.GetUtcOffset(inputDateTime).TotalHours - 8);
            var dateTimeString = dateTimeOffset.ToString("yyyy-MM-ddTHH:mm:ssZ");
            Assert.Equal(expectedDateTime, dateTimeString);
        }

        [Theory]
        [MemberData(nameof(GetValidDataForAddSecondsWithDefaultTimeZoneHandling))]
        public void GivenAValidData_WhenAddSecondsWithDefaultTimeZoneHandling_ConvertedDateTimeShouldBeReturned(string originalDateTime, double seconds, string expectedDateTime)
        {
            var result = Filters.AddSeconds(originalDateTime, seconds);
            Assert.Equal(expectedDateTime, result);
        }

        [Theory]
        [MemberData(nameof(GetInvalidDataForAddSeconds))]
        public void GivenSeconds_WhenAddOnInvalidDateTime_ExceptionShouldBeThrow(string originalDateTime)
        {
            var exception = Assert.Throws<RenderException>(() => Filters.AddSeconds(originalDateTime, 0));
            Assert.Equal(FhirConverterErrorCode.InvalidDateTimeFormat, exception.FhirConverterErrorCode);
        }

        [Theory]
        [MemberData(nameof(GetValidDataForAddHyphensDate))]
        public void GivenADate_WhenAddHyphensDate_ConvertedDateShouldBeReturned(string input, string timeZoneHandling, string expected)
        {
            var result = Filters.AddHyphensDate(input, timeZoneHandling);
            Assert.Equal(expected, result);
        }

        [Theory]
        [MemberData(nameof(GetValidDataWithoutTimeZoneForAddHyphensDateWithUtcTimeZoneHandling))]
        public void GivenAValidDataWithoutTimeZone_WhenAddHyphensDate_CorrectDateTimeShouldBeReturned(string input, string timeZoneHandling, DateTime inputDateTime)
        {
            var result = Filters.AddHyphensDate(input, timeZoneHandling);
            var dateTimeOffset = new DateTimeOffset(inputDateTime);
            var dateTimeString = dateTimeOffset.ToUniversalTime().ToString("yyyy-MM-dd");
            Assert.Contains(result, dateTimeString);
        }

        [Theory]
        [MemberData(nameof(GetValidDataForAddHyphensDateWithDefaultTimeZoneHandling))]
        public void GivenAValidData_WhenAddHyphensDateWithDefaultTimeZoneHandling_ConvertedDateTimeShouldBeReturned(string input, string expectedDateTime)
        {
            var result = Filters.AddHyphensDate(input);
            Assert.Equal(expectedDateTime, result);
        }

        [Theory]
        [MemberData(nameof(GetValidDataForFormatAsDateTime))]
        public void GivenADateTime_WhenFormatAsDateTime_ConvertedDateTimeStringShouldBeReturned(string input, string timeZoneHandling, string expected)
        {
            var result = Filters.FormatAsDateTime(input, timeZoneHandling);
            Assert.Equal(expected, result);
        }

        [Theory]
        [MemberData(nameof(GetValidDataWithoutTimeZoneForFormatAsDateTimeWithUtcTimeZoneHandling))]
        public void GivenAValidDataWithoutTimeZone_WhenFormatAsDateTime_ConvertedDateTimeShouldBeReturned(string input, string timeZoneHandling, string expectedDateTime, DateTime inputDateTime)
        {
            var result = Filters.FormatAsDateTime(input, timeZoneHandling);
            var dateTimeOffset = DateTimeOffset.Parse(result);
            dateTimeOffset = dateTimeOffset.AddHours(TimeZoneInfo.Local.GetUtcOffset(inputDateTime).TotalHours - 8);
            var dateTimeString = dateTimeOffset.ToString("yyyy-MM-ddTHH:mm:ssZ");
            Assert.Contains(expectedDateTime, dateTimeString);
        }

        [Theory]
        [MemberData(nameof(GetValidDataForFormatAsDateTimeWithDefaultTimeZoneHandling))]
        public void GivenAValidData_WhenFormatAsDateTimeWithDefaultTimeZoneHandling_ConvertedDateTimeShouldBeReturned(string input, string expectedDateTime)
        {
            var result = Filters.FormatAsDateTime(input);
            Assert.Equal(expectedDateTime, result);
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
