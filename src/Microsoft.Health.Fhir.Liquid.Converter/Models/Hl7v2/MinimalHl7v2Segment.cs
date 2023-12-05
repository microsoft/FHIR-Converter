// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Health.Fhir.Liquid.Converter.Models.Hl7v2
{
    public class MinimalHl7v2Segment
    {
        public MinimalHl7v2Segment(string segmentName, List<string> fields)
        {
            SegmentName = segmentName;
            Fields = fields;
        }

        public List<string> Fields { get; set; }

        public string SegmentName { get; set; }
    }
}
