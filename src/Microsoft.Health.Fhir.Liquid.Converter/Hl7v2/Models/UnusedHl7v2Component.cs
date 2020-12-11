// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.Liquid.Converter.Hl7v2.Models
{
    public class UnusedHl7v2Component
    {
        public UnusedHl7v2Component(int offset, string value)
        {
            Offset = offset;
            Value = value;
        }

        public int Offset { get; set; }

        public string Value { get; set; }
    }
}
