// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Fhir.Liquid.Converter.Hl7v2.InputProcessor;

namespace Microsoft.Health.Fhir.Liquid.Converter
{
    /// <summary>
    /// Filters for conversion
    /// </summary>
    public partial class Filters
    {
        public static char CharAt(string dataString, int index)
        {
            return dataString[index];
        }

        public static bool Contains(string parentString, string childString)
        {
            if (parentString == null)
            {
                return false;
            }

            return parentString.Contains(childString);
        }

        public static string EscapeSpecialChars(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return data;
            }

            return SpecialCharProcessor.Escape(data);
        }

        public static string UnescapeSpecialChars(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return data;
            }

            return SpecialCharProcessor.Unescape(data);
        }
    }
}
