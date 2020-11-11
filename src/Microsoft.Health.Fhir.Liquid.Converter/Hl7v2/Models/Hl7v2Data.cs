// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using DotLiquid;

namespace Microsoft.Health.Fhir.Liquid.Converter.Hl7v2.Models
{
    public class Hl7v2Data : Drop
    {
        public Hl7v2Data()
        {
            Meta = new List<string>();
            Data = new List<Hl7v2Segment>();
        }

        public List<string> Meta { get; set; }

        public List<Hl7v2Segment> Data { get; set; }
    }
}
