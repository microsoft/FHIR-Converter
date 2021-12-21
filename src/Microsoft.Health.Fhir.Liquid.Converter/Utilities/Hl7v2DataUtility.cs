// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Health.Fhir.Liquid.Converter.Utilities
{
    public static class Hl7v2DataUtility
    {
        private static readonly string[] SegmentSeparators = { "\r\n", "\r", "\n" };

        public static string[] SplitMessageToSegments(string message)
        {
            var segments = message.Split(SegmentSeparators, StringSplitOptions.RemoveEmptyEntries);
            return segments;
        }
    }
}
