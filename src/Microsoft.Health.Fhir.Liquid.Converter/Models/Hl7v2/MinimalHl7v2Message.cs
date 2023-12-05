// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Health.Fhir.Liquid.Converter.Models.Hl7v2
{
    public class MinimalHl7v2Message
    {
        public MinimalHl7v2Message(List<MinimalHl7v2Segment> segments)
        {
            Segments = segments;
        }

        public MinimalHl7v2Message()
        {
            Segments = new List<MinimalHl7v2Segment>();
        }

        public List<MinimalHl7v2Segment> Segments { get; set; }

        public Hl7v2EncodingCharacters Hl7v2EncodingCharacters { get; set; } = new Hl7v2EncodingCharacters();
    }
}
