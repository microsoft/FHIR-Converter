// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Globalization;

namespace Microsoft.Health.Fhir.Liquid.Converter
{
    /// <summary>
    /// Filters for conversion
    /// </summary>
    public partial class Filters
    {
        private static readonly Random RandomGenerator = new Random();

        public static bool IsNaN(object data)
        {
            return double.TryParse(Convert.ToString(data, CultureInfo.InvariantCulture), NumberStyles.Any, NumberFormatInfo.InvariantInfo, out _);
        }

        public static double Abs(double data)
        {
            return Math.Abs(data);
        }

        public static double Pow(double x, double y)
        {
            return Math.Pow(x, y);
        }

        public static int Random(int maxValue)
        {
            return RandomGenerator.Next(maxValue);
        }

        public static double Sign(double data)
        {
            return Math.Sign(data);
        }

        public static double TruncateNumber(double data)
        {
            return Math.Truncate(data);
        }

        public static double Divide(double x, double y)
        {
            return x / y;
        }
    }
}