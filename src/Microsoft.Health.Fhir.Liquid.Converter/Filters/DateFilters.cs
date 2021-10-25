// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Text.RegularExpressions;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;

namespace Microsoft.Health.Fhir.Liquid.Converter
{
    /// <summary>
    /// Filters for conversion
    /// </summary>
    public partial class Filters
    {
        private static readonly Regex Hl7v2DateTimeRegex = new Regex(@"^((?<year>\d{4})((?<month>\d{2})((?<day>\d{2})(?<time>((?<hour>\d{2})((?<minute>\d{2})((?<second>\d{2})(\.(?<millisecond>\d+))?)?)?))?)?)?(?<timeZone>(?<sign>-|\+)(?<timeZoneHour>\d{2})(?<timeZoneMinute>\d{2}))?)$");
        private static readonly Regex FhirDateTimeRegex = new Regex(@"^(?<year>([0-9]([0-9]([0-9][1-9]|[1-9]0)|[1-9]00)|[1-9]000))((-(?<month>0[1-9]|1[0-2]))((-(?<day>0[1-9]|[1-2][0-9]|3[0-1]))(?<time>T(?<hour>[01][0-9]|2[0-3]):(?<minute>[0-5][0-9]):(?<second>([0-5][0-9]|60)(\.[0-9]+)?)(?<timeZone>Z|(?<sign>\+|-)((?<timeZoneHour>0[0-9]|1[0-3]):(?<timeZoneMinute>[0-5][0-9]|14:00))))?)?)?$");

        public static string AddSeconds(string input, double seconds, string timeZoneHandling = "preserve")
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            var groups = RegexParsing(input, FhirDateTimeRegex);
            var dateTimeOffset = ParseDateTimeOffset(input, groups);
            dateTimeOffset = dateTimeOffset.AddSeconds(seconds);
            return DateTimeToFhirString(dateTimeOffset, groups["timeZone"].Success, timeZoneHandling);
        }

        public static string AddHyphensDate(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            var groups = RegexParsing(input, Hl7v2DateTimeRegex);
            return ConvertDate(input, groups);
        }

        public static string FormatAsDateTime(string input, string timeZoneHandling = "local")
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            var groups = RegexParsing(input, Hl7v2DateTimeRegex);
            if (!groups["time"].Success)
            {
                return ConvertDate(input, groups);
            }

            // Convert DateTime with time zone
            return ConvertDateTime(input, groups, timeZoneHandling);
        }

        public static string Now(string input, string format = "yyyy-MM-ddTHH:mm:ss.FFFZ")
        {
            return DateTime.UtcNow.ToString(format);
        }

        private static string ConvertDate(string input, GroupCollection groups)
        {
            var dateTime = ParseDate(input, groups);
            return groups["year"].Success switch
            {
                true when groups["month"].Success && groups["day"].Success => dateTime.ToString("yyyy-MM-dd"),
                true when groups["month"].Success => dateTime.ToString("yyyy-MM"),
                true => dateTime.ToString("yyyy"),
                _ => throw new RenderException(FhirConverterErrorCode.InvalidDateTimeFormat, string.Format(Resources.InvalidDateTimeFormat, input))
            };
        }

        private static string ConvertDateTime(string input, GroupCollection groups, string timeZoneHandling)
        {
            var dateTimeOffset = ParseDateTimeOffset(input, groups);
            return DateTimeToFhirString(dateTimeOffset, groups["timeZone"].Success, timeZoneHandling);
        }

        private static string DateTimeToFhirString(DateTimeOffset dateTimeOffset, bool hasTimezone, string timeZoneHandling)
        {
            var resultdateTime = timeZoneHandling?.ToLower() switch
            {
                "preserve" => dateTimeOffset,
                "utc" => dateTimeOffset.ToUniversalTime(),
                "local" => dateTimeOffset.ToLocalTime(),
                _ => throw new RenderException(FhirConverterErrorCode.InvalidTimeZoneHandling, Resources.InvalidTimeZoneHandling),
            };
            var timeZoneSuffix = hasTimezone || string.Equals(timeZoneHandling.ToLower(), "utc") ? (resultdateTime.Offset == TimeSpan.Zero ? "Z" : "%K") : string.Empty;
            var dateTimeFormat = dateTimeOffset.Millisecond == 0 ? "yyyy-MM-ddTHH:mm:ss" + timeZoneSuffix : "yyyy-MM-ddTHH:mm:ss.fff" + timeZoneSuffix;
            return resultdateTime.ToString(dateTimeFormat);
        }

        private static GroupCollection RegexParsing(string input, Regex regex)
        {
            var matches = regex.Matches(input);
            if (matches.Count != 1)
            {
                throw new RenderException(FhirConverterErrorCode.InvalidDateTimeFormat, string.Format(Resources.InvalidDateTimeFormat, input));
            }

            return matches[0].Groups;
        }

        private static DateTime ParseDate(string input, GroupCollection groups)
        {
            var year = groups["year"].Success ? int.Parse(groups["year"].Value) : 1;
            var month = groups["month"].Success ? int.Parse(groups["month"].Value) : 1;
            var day = groups["day"].Success ? int.Parse(groups["day"].Value) : 1;
            try
            {
                return new DateTime(year, month, day);
            }
            catch (Exception)
            {
                throw new RenderException(FhirConverterErrorCode.InvalidDateTimeFormat, string.Format(Resources.InvalidDateTimeFormat, input));
            }
        }

        private static DateTimeOffset ParseDateTimeOffset(string input, GroupCollection groups)
        {
            int year = groups["year"].Success ? int.Parse(groups["year"].Value) : 1;
            int month = groups["month"].Success ? int.Parse(groups["month"].Value) : 1;
            int day = groups["day"].Success ? int.Parse(groups["day"].Value) : 1;
            int hour = groups["hour"].Success ? int.Parse(groups["hour"].Value) : 0;
            int minute = groups["minute"].Success ? int.Parse(groups["minute"].Value) : 0;
            int second = groups["second"].Success ? int.Parse(groups["second"].Value) : 0;
            int millisecond = groups["millisecond"].Success ? int.Parse(groups["millisecond"].Value) : 0;

            var timeSpan = TimeSpan.FromHours(TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).Hours);
            if (groups["timeZone"].Success)
            {
                if (groups["timeZone"].Value == "Z")
                {
                    timeSpan = TimeSpan.Zero;
                }
                else
                {
                    var sign = groups["sign"].Value == "+" ? 1 : -1;
                    var timeZoneHour = int.Parse(groups["timeZoneHour"].Value) * sign;
                    var timeZoneMinute = int.Parse(groups["timeZoneMinute"].Value) * sign;
                    timeSpan = new TimeSpan(timeZoneHour, timeZoneMinute, 0);
                }
            }

            try
            {
                return new DateTimeOffset(year, month, day, hour, minute, second, millisecond, timeSpan);
            }
            catch (Exception)
            {
                throw new RenderException(FhirConverterErrorCode.InvalidDateTimeFormat, string.Format(Resources.InvalidDateTimeFormat, input));
            }
        }
    }
}
