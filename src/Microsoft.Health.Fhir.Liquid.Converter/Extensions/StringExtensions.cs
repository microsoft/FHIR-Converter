// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Linq;

namespace Microsoft.Health.Fhir.Liquid.Converter.Extensions
{
    public static class StringExtensions
    {
        public static int IndexOfNthOccurrence(this string s, char c, int n)
        {
            if (n <= 0)
            {
                return -1;
            }

            var result = s?
                .Select((c, i) => new { Char = c, Index = i })
                .Where(item => item.Char == c)
                .Skip(n - 1)
                .FirstOrDefault();

            return result?.Index ?? -1;
        }
    }
}
