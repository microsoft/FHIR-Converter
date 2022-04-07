// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Text.RegularExpressions;

namespace Microsoft.Health.Fhir.Liquid.Converter.Models
{
    public class PartialDateTime
    {
        private static readonly Regex DateTimeRegex = new Regex(@"^((?<year>\d{4})((?<month>\d{2})((?<day>\d{2})(?<time>((?<hour>\d{2})((?<minute>\d{2})((?<second>\d{2})(\.(?<millisecond>\d+))?)?)?))?)?)?(?<timeZone>(?<sign>-|\+)(?<timeZoneHour>\d{2})(?<timeZoneMinute>\d{2}))?)$");
        private static readonly Regex FhirDateTimeRegex = new Regex(@"^(?<year>([0-9]([0-9]([0-9][1-9]|[1-9]0)|[1-9]00)|[1-9]000))((-(?<month>0[1-9]|1[0-2]))((-(?<day>0[1-9]|[1-2][0-9]|3[0-1]))(?<time>T(?<hour>[01][0-9]|2[0-3]):(?<minute>[0-5][0-9]):((?<second>[0-5][0-9]|60)(\.(?<millisecond>[0-9]+))?)(?<timeZone>Z|(?<sign>\+|-)((?<timeZoneHour>0[0-9]|1[0-3]):(?<timeZoneMinute>[0-5][0-9])|(?<timeZoneHour>14):(?<timeZoneMinute>00))))?)?)?$");

        public PartialDateTime(string input, DateTimeType type = DateTimeType.Fhir)
        {
            var regex = type switch
            {
                DateTimeType.Ccda => DateTimeRegex,
                DateTimeType.Hl7v2 => DateTimeRegex,
                DateTimeType.Fhir => FhirDateTimeRegex,
                _ => throw new ArgumentException(string.Format(Resources.InvalidDateTimeFormat, type.ToString())),
            };

            var matches = regex.Matches(input);
            if (matches.Count != 1 || !matches[0].Groups["year"].Success)
            {
                throw new ArgumentException(string.Format(Resources.InvalidDateTimeFormat, input));
            }

            var groups = matches[0].Groups;

            int year = int.Parse(groups["year"].Value);
            int month = groups["month"].Success ? int.Parse(groups["month"].Value) : 1;
            int day = groups["day"].Success ? int.Parse(groups["day"].Value) : 1;
            int hour = groups["hour"].Success ? int.Parse(groups["hour"].Value) : 0;
            int minute = groups["minute"].Success ? int.Parse(groups["minute"].Value) : 0;
            int second = groups["second"].Success ? int.Parse(groups["second"].Value) : 0;
            int millisecond = groups["millisecond"].Success ? int.Parse(groups["millisecond"].Value) : 0;

            var timeSpan = TimeZoneInfo.Local.GetUtcOffset(new DateTime(year, month, day, hour, minute, second, millisecond));
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

            DateTimeValue = new DateTimeOffset(year, month, day, hour, minute, second, millisecond, timeSpan);
            Precision =
                    groups["millisecond"].Success ? DateTimePrecision.Milliseconds :
                    groups["second"].Success ? DateTimePrecision.Second :
                    groups["minute"].Success ? DateTimePrecision.Minute :
                    groups["hour"].Success ? DateTimePrecision.Hour :
                    groups["day"].Success ? DateTimePrecision.Day :
                    groups["month"].Success ? DateTimePrecision.Month :
                    DateTimePrecision.Year;
            HasTimeZone = groups["timeZone"].Success;
        }

        public DateTimeOffset DateTimeValue { get; private set; }

        public bool HasTimeZone { get; private set; }

        public DateTimePrecision Precision { get; private set; }

        public PartialDateTime ConvertToDate()
        {
            Precision = Precision < DateTimePrecision.Day ? Precision : DateTimePrecision.Day;
            return this;
        }

        public PartialDateTime AddSeconds(double seconds)
        {
            DateTimeValue = DateTimeValue.AddSeconds(seconds);
            if (Precision != DateTimePrecision.Milliseconds)
            {
                Precision = DateTimeValue.Millisecond == 0 ? DateTimePrecision.Second : DateTimePrecision.Milliseconds;
            }

            return this;
        }

        public string ToFhirString(TimeZoneHandlingMethod timeZoneHandling = TimeZoneHandlingMethod.Preserve)
        {
            var resultDateTime = timeZoneHandling switch
            {
                TimeZoneHandlingMethod.Preserve => DateTimeValue,
                TimeZoneHandlingMethod.Utc => DateTimeValue.ToUniversalTime(),
                TimeZoneHandlingMethod.Local => DateTimeValue.ToLocalTime(),
                _ => throw new ArgumentException(Resources.InvalidTimeZoneHandling),
            };

            if (Precision <= DateTimePrecision.Day)
            {
                return Precision switch
                {
                    DateTimePrecision.Day => resultDateTime.ToString("yyyy-MM-dd"),
                    DateTimePrecision.Month => resultDateTime.ToString("yyyy-MM"),
                    DateTimePrecision.Year => resultDateTime.ToString("yyyy"),
                    _ => throw new ArgumentException("Invalid dateTimeObject with empty Year field.")
                };
            }

            var timeZoneSuffix = resultDateTime.Offset == TimeSpan.Zero ? "Z" : "%K";

            var dateTimeFormat = Precision < DateTimePrecision.Milliseconds ? "yyyy-MM-ddTHH:mm:ss" + timeZoneSuffix : "yyyy-MM-ddTHH:mm:ss.fff" + timeZoneSuffix;
            return resultDateTime.ToString(dateTimeFormat);
        }
    }
}
