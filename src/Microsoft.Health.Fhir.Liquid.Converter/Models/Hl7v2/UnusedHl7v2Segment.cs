// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Health.Fhir.Liquid.Converter.Models.Hl7v2
{
    public class UnusedHl7v2Segment
    {
        public UnusedHl7v2Segment(int line)
        {
            Type = string.Empty;
            Line = line;
            Components = new List<UnusedHl7v2Component>();
        }

        public string Type { get; set; }

        public int Line { get; set; }

        public List<UnusedHl7v2Component> Components { get; set; }
    }
}
