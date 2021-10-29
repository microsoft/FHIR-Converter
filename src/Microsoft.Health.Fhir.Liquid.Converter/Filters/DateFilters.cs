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
        private static readonly Regex FhirDateTimeRegex = new Regex(@"^(?<year>([0-9]([0-9]([0-9][1-9]|[1-9]0)|[1-9]00)|[1-9]000))((-(?<month>0[1-9]|1[0-2]))((-(?<day>0[1-9]|[1-2][0-9]|3[0-1]))(?<time>T(?<hour>[01][0-9]|2[0-3]):(?<minute>[0-5][0-9]):(?<second>([0-5][0-9]|60)(\.[0-9]+)?)(?<timeZone>Z|(?<sign>\+|-)((?<timeZoneHour>0[0-9]|1[0-3]):(?<timeZoneMinute>[0-5][0-9])|(?<timeZoneHour>14):(?<timeZoneMinute>00))))?)?)?$");

        public static string AddSeconds(string input, double seconds, string timeZoneHandling = "preserve")
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            var dateTimeObject = ParseDateTimeObject(input, FhirDateTimeRegex);
            var result = new DateTimeObject()
            {
                DateValue = dateTimeObject.DateValue.AddSeconds(seconds),
                HasTimeZone = dateTimeObject.HasTimeZone,
            };
            return DateTimeObjectToFhirString(input, result, timeZoneHandling);
        }

        public static string AddHyphensDate(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            var dateTimeObject = ParseDateTimeObject(input, Hl7v2DateTimeRegex);
            dateTimeObject.HasTime = false;
            return DateTimeObjectToFhirString(input, dateTimeObject, null);
        }

        public static string FormatAsDateTime(string input, string timeZoneHandling = "local")
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            var dateTimeObject = ParseDateTimeObject(input, Hl7v2DateTimeRegex);
            return DateTimeObjectToFhirString(input, dateTimeObject, timeZoneHandling);
        }

        public static string Now(string input, string format = "yyyy-MM-ddTHH:mm:ss.FFFZ")
        {
            return DateTime.UtcNow.ToString(format);
        }

        private static DateTimeObject ParseDateTimeObject(string input, Regex regex)
        {
            var matches = regex.Matches(input);
            if (matches.Count != 1)
            {
                throw new RenderException(FhirConverterErrorCode.InvalidDateTimeFormat, string.Format(Resources.InvalidDateTimeFormat, input));
            }

            var groups = matches[0].Groups;

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
                    var sign = groups["sign"].Success && groups["sign"].Value == "-" ? -1 : 1;
                    var timeZoneHour = int.Parse(groups["timeZoneHour"].Value) * sign;
                    var timeZoneMinute = int.Parse(groups["timeZoneMinute"].Value) * sign;
                    timeSpan = new TimeSpan(timeZoneHour, timeZoneMinute, 0);
                }
            }

            try
            {
                return new DateTimeObject()
                {
                    DateValue = new DateTimeOffset(year, month, day, hour, minute, second, millisecond, timeSpan),
                    HasDay = groups["day"].Success,
                    HasMonth = groups["month"].Success,
                    HasYear = groups["year"].Success,
                    HasTime = groups["time"].Success,
                    HasHour = groups["hour"].Success,
                    HasMinute = groups["minute"].Success,
                    HasSecond = groups["second"].Success,
                    HasMilliSecond = groups["millisecond"].Success,
                    HasTimeZone = groups["timeZone"].Success,
                };
            }
            catch (Exception)
            {
                throw new RenderException(FhirConverterErrorCode.InvalidDateTimeFormat, string.Format(Resources.InvalidDateTimeFormat, input));
            }
        }

        private static string DateTimeObjectToFhirString(string input, DateTimeObject dateTimeObject, string timeZoneHandling)
        {
            if (!dateTimeObject.HasTime)
            {
                return dateTimeObject.HasYear switch
                {
                    true when dateTimeObject.HasMonth && dateTimeObject.HasDay => dateTimeObject.DateValue.ToString("yyyy-MM-dd"),
                    true when dateTimeObject.HasMonth => dateTimeObject.DateValue.ToString("yyyy-MM"),
                    true => dateTimeObject.DateValue.ToString("yyyy"),
                    _ => throw new RenderException(FhirConverterErrorCode.InvalidDateTimeFormat, string.Format(Resources.InvalidDateTimeFormat, input))
                };
            }

            var resultdateTime = timeZoneHandling?.ToLower() switch
            {
                "preserve" => dateTimeObject.DateValue,
                "utc" => dateTimeObject.DateValue.ToUniversalTime(),
                "local" => dateTimeObject.DateValue.ToLocalTime(),
                _ => throw new RenderException(FhirConverterErrorCode.InvalidTimeZoneHandling, Resources.InvalidTimeZoneHandling),
            };

            var timeZoneSuffix = string.Empty;
            if (dateTimeObject.HasTimeZone || string.Equals(timeZoneHandling.ToLower(), "utc"))
            {
                // Using "Z" to represent zero timezone.
                timeZoneSuffix = resultdateTime.Offset == TimeSpan.Zero ? "Z" : "%K";
            }

            var dateTimeFormat = dateTimeObject.DateValue.Millisecond == 0 ? "yyyy-MM-ddTHH:mm:ss" + timeZoneSuffix : "yyyy-MM-ddTHH:mm:ss.fff" + timeZoneSuffix;
            return resultdateTime.ToString(dateTimeFormat);
        }
    }
}
