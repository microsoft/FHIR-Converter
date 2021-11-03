// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
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
        private static readonly Regex FhirDateTimeRegex = new Regex(@"^(?<year>([0-9]([0-9]([0-9][1-9]|[1-9]0)|[1-9]00)|[1-9]000))((-(?<month>0[1-9]|1[0-2]))((-(?<day>0[1-9]|[1-2][0-9]|3[0-1]))(?<time>T(?<hour>[01][0-9]|2[0-3]):(?<minute>[0-5][0-9]):(?<second>([0-5][0-9]|60)(\.[0-9]+)?)(?<timeZone>Z|(?<sign>\+|-)((?<timeZoneHour>0[0-9]|1[0-3]):(?<timeZoneMinute>[0-5][0-9])|(?<timeZoneHour>14):(?<timeZoneMinute>00))))?)?)?$");
        private static readonly HashSet<string> TimezoneHandlingMethods = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "preserve", "utc", "local" };

        public static string AddSeconds(string input, double seconds, string timeZoneHandling = "preserve")
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            DateTimeObject dateTimeObject;
            try
            {
                dateTimeObject = new DateTimeObject(input, FhirDateTimeRegex);
            }
            catch (Exception)
            {
                throw new RenderException(FhirConverterErrorCode.InvalidDateTimeFormat, string.Format(Resources.InvalidDateTimeFormat, input));
            }

            dateTimeObject.AddSeconds(seconds);
            return dateTimeObject.ToFhirString(timeZoneHandling);
        }

        public static string AddHyphensDate(string input, string timeZoneHandling = "preserve")
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            DateTimeObject dateTimeObject;
            try
            {
                dateTimeObject = new DateTimeObject(input, DateTimeRegex);
            }
            catch (Exception)
            {
                throw new RenderException(FhirConverterErrorCode.InvalidDateTimeFormat, string.Format(Resources.InvalidDateTimeFormat, input));
            }

            dateTimeObject.ConvertToDate();
            return dateTimeObject.ToFhirString(timeZoneHandling);
        }

        public static string FormatAsDateTime(string input, string timeZoneHandling = "local")
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            if (!TimezoneHandlingMethods.Contains(timeZoneHandling))
            {
                throw new RenderException(FhirConverterErrorCode.InvalidTimeZoneHandling, Resources.InvalidTimeZoneHandling);
            }

            DateTimeObject dateTimeObject;
            try
            {
                dateTimeObject = new DateTimeObject (input, DateTimeRegex);
            }
            catch (Exception)
            {
                throw new RenderException(FhirConverterErrorCode.InvalidDateTimeFormat, string.Format(Resources.InvalidDateTimeFormat, input));
            }

            return dateTimeObject.ToFhirString(timeZoneHandling);
        }

        public static string Now(string input, string format = "yyyy-MM-ddTHH:mm:ss.FFFZ")
        {
            return DateTime.UtcNow.ToString(format);
        }
    }
}
