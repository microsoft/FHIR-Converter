// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.Liquid.Converter.Models.Hl7v2
{
    public class UnusedHl7v2Component
    {
        public UnusedHl7v2Component(int start, int end, string value)
        {
            Start = start;
            End = end;
            Value = value;
        }

        public int Start { get; }

        public int End { get; }

        public string Value { get; }
    }
}
