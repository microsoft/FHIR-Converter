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
        private static readonly Regex DateTimeRegex = new Regex(@"^((?<year>\d{4})((?<month>\d{2})((?<day>\d{2})(?<time>((?<hour>\d{2})((?<minute>\d{2})((?<second>\d{2})(\.(?<millisecond>\d+))?)?)?))?)?)?(?<timeZone>(?<sign>-|\+)(?<timeZoneHour>\d{2})(?<timeZoneMinute>\d{2}))?)$");

        public static string AddHyphensDate(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            var matches = DateTimeRegex.Matches(input);
            if (matches.Count != 1)
            {
                throw new RenderException(FhirConverterErrorCode.InvalidDateTimeFormat, string.Format(Resources.InvalidDateTimeFormat, input));
            }

            var groups = matches[0].Groups;
            return ConvertDate(input, groups);
        }

        public static string FormatAsDateTime(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            var matches = DateTimeRegex.Matches(input);
            if (matches.Count != 1)
            {
                throw new RenderException(FhirConverterErrorCode.InvalidDateTimeFormat, string.Format(Resources.InvalidDateTimeFormat, input));
            }

            var groups = matches[0].Groups;
            if (groups["time"].Success)
            {
                // Convert DateTime with time zone
                string result = ConvertDateTime(input, groups);
                if (groups["timeZone"].Success)
                {
                    int timeZoneHour = int.Parse(groups["timeZoneHour"].Value);
                    int timeZoneMinute = int.Parse(groups["timeZoneMinute"].Value);
                    result += groups["sign"].Value + new TimeSpan(timeZoneHour, timeZoneMinute, 0).ToString(@"hh\:mm");
                }
                else
                {
                    result += "Z";
                }

                return result;
            }
            else
            {
                // Convert Date
                return ConvertDate(input, groups);
            }
        }

        public static string Now(string input)
        {
            return DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.FFFZ");
        }

        private static string ConvertDate(string input, GroupCollection groups)
        {
            var year = groups["year"].Success ? int.Parse(groups["year"].Value) : 1;
            var month = groups["month"].Success ? int.Parse(groups["month"].Value) : 1;
            var day = groups["day"].Success ? int.Parse(groups["day"].Value) : 1;

            try
            {
                var dateTime = new DateTime(year, month, day);
                return groups["year"].Success switch
                {
                    true when groups["month"].Success && groups["day"].Success => dateTime.ToString("yyyy-MM-dd"),
                    true when groups["month"].Success => dateTime.ToString("yyyy-MM"),
                    true => dateTime.ToString("yyyy"),
                    _ => throw new RenderException(FhirConverterErrorCode.InvalidDateTimeFormat, string.Format(Resources.InvalidDateTimeFormat, input))
                };
            }
            catch (Exception)
            {
                throw new RenderException(FhirConverterErrorCode.InvalidDateTimeFormat, string.Format(Resources.InvalidDateTimeFormat, input));
            }
        }

        private static string ConvertDateTime(string input, GroupCollection groups)
        {
            int year = groups["year"].Success ? int.Parse(groups["year"].Value) : 0;
            int month = groups["month"].Success ? int.Parse(groups["month"].Value) : 0;
            int day = groups["day"].Success ? int.Parse(groups["day"].Value) : 0;
            int hour = groups["hour"].Success ? int.Parse(groups["hour"].Value) : 0;
            int minute = groups["minute"].Success ? int.Parse(groups["minute"].Value) : 0;
            int second = groups["second"].Success ? int.Parse(groups["second"].Value) : 0;
            int millisecond = groups["millisecond"].Success ? int.Parse(groups["millisecond"].Value) : 0;

            try
            {
                return new DateTime(year, month, day, hour, minute, second, millisecond).ToString("yyyy-MM-ddTHH:mm:ss.FFF");
            }
            catch (Exception)
            {
                throw new RenderException(FhirConverterErrorCode.InvalidDateTimeFormat, string.Format(Resources.InvalidDateTimeFormat, input));
            }
        }
    }
}
