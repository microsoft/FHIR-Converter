// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Linq;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;

namespace Microsoft.Health.Fhir.Liquid.Converter
{
    /// <summary>
    /// Filters for conversion
    /// </summary>
    public partial class Filters
    {
        public static string AddSeconds(string input, double seconds, string timeZoneHandling = "preserve")
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            if (!Enum.TryParse(timeZoneHandling, true, out TimeZoneHandlingMethod outputTimeZoneHandling))
            {
                throw new RenderException(FhirConverterErrorCode.InvalidTimeZoneHandling, Resources.InvalidTimeZoneHandling);
            }

            PartialDateTime dateTimeObject;
            try
            {
                dateTimeObject = new PartialDateTime(input, DateTimeType.Fhir);
            }
            catch (Exception)
            {
                throw new RenderException(FhirConverterErrorCode.InvalidDateTimeFormat, string.Format(Resources.InvalidDateTimeFormat, input));
            }

            dateTimeObject.AddSeconds(seconds);
            return dateTimeObject.ToFhirString(outputTimeZoneHandling);
        }

        public static string AddHyphensDate(string input, string timeZoneHandling = "preserve")
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            if (!Enum.TryParse(timeZoneHandling, true, out TimeZoneHandlingMethod outputTimeZoneHandling))
            {
                throw new RenderException(FhirConverterErrorCode.InvalidTimeZoneHandling, Resources.InvalidTimeZoneHandling);
            }

            PartialDateTime dateTimeObject;
            try
            {
                dateTimeObject = new PartialDateTime(input, DateTimeType.Hl7v2);
            }
            catch (Exception)
            {
                throw new RenderException(FhirConverterErrorCode.InvalidDateTimeFormat, string.Format(Resources.InvalidDateTimeFormat, input));
            }

            dateTimeObject.ConvertToDate();
            return dateTimeObject.ToFhirString(outputTimeZoneHandling);
        }

        public static string FormatAsDateTime(string input, string timeZoneHandling = "preserve")
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            if (!Enum.TryParse(timeZoneHandling, true, out TimeZoneHandlingMethod outputTimeZoneHandling))
            {
                throw new RenderException(FhirConverterErrorCode.InvalidTimeZoneHandling, Resources.InvalidTimeZoneHandling);
            }

            PartialDateTime dateTimeObject;
            try
            {
                dateTimeObject = new PartialDateTime(input, DateTimeType.Hl7v2);
            }
            catch (Exception)
            {
                throw new RenderException(FhirConverterErrorCode.InvalidDateTimeFormat, string.Format(Resources.InvalidDateTimeFormat, input));
            }

            return dateTimeObject.ToFhirString(outputTimeZoneHandling);
        }

        public static string Now(string input, string format = "yyyy-MM-ddTHH:mm:ss.FFFZ")
        {
            return DateTime.UtcNow.ToString(format);
        }

        /* Converts an HL7v2 time interval to the FHIR format for time
        /    HL7v2 - 2.8.35.2 Explicit time interval (ST)
        /    FHIR  - time https://build.fhir.org/datatypes.html#time
        */
        public static string FormatTimeWithColon(string input, string inputFormat = "HHmm")
        {
            // For backwards compatibility, if the input is not a 4 digit numeric value then just return the value
            if (string.IsNullOrEmpty(input) || (!(input.All(char.IsDigit) && (input.Length == 4))))
            {
                return input;
            }

            var dt = TimeSpan.Zero;
            try
            {
                dt = DateTime.ParseExact(input, inputFormat, CultureInfo.InvariantCulture).TimeOfDay;
            }
            catch (Exception)
            {
                throw new RenderException(FhirConverterErrorCode.InvalidDateTimeFormat, string.Format(Resources.InvalidDateTimeFormat, input));
            }

            return dt.ToString();
        }

        public static string FormatAsHl7v2DateTime(string input, string timeZoneHandling = "preserve")
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            if (!Enum.TryParse(timeZoneHandling, true, out TimeZoneHandlingMethod outputTimeZoneHandling))
            {
                throw new RenderException(FhirConverterErrorCode.InvalidTimeZoneHandling, Resources.InvalidTimeZoneHandling);
            }

            PartialDateTime dateTimeObject;
            try
            {
                dateTimeObject = new PartialDateTime(input, DateTimeType.Fhir);
            }
            catch (Exception)
            {
                throw new RenderException(FhirConverterErrorCode.InvalidDateTimeFormat, string.Format(Resources.InvalidDateTimeFormat, input));
            }

            return dateTimeObject.ToHl7v2Date(outputTimeZoneHandling);
        }
    }
}
