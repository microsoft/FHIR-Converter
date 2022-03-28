// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
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
    }
}
