// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.Liquid.Converter.Hl7v2.Models
{
    public class UnusedHl7v2Component
    {
        public UnusedHl7v2Component(int index, int offset, string value)
        {
            Index = index;
            OffsetInSegment = offset;
            Value = value;
        }

        public int Index { get; set; }

        public int OffsetInSegment { get; set; }

        public string Value { get; set; }
    }
}
