// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Health.Fhir.Liquid.Converter.Models;

namespace Microsoft.Health.Fhir.Liquid.Converter.Hl7v2.Models
{
    public class Hl7v2TraceInfo : TraceInfo
    {
        public Hl7v2TraceInfo()
        {
        }

        public Hl7v2TraceInfo(List<UnusedHl7v2Segment> unusedHl7V2Segments)
        {
            UnusedSegments = unusedHl7V2Segments;
        }

        public List<UnusedHl7v2Segment> UnusedSegments { get; set; }

        public static Hl7v2TraceInfo CreateTraceInfo(Hl7v2Data hl7V2Data)
        {
            var unusedSegments = new List<UnusedHl7v2Segment>();
            for (var i = 0; i < hl7V2Data?.Data?.Count; ++i)
            {
                var segment = hl7V2Data.Data[i];
                var unusedSegment = new UnusedHl7v2Segment(i);
                for (var j = 0; j < segment?.Fields?.Count; ++j)
                {
                    // Encoding characters field is treated as accessed
                    if (i == 0 && j == 1)
                    {
                        continue;
                    }

                    // Segment id field is treated as accessed
                    if (j == 0 && segment.Fields[j] is Hl7v2Field segmentIdField)
                    {
                        unusedSegment.Type = segmentIdField.Value;
                        continue;
                    }

                    if (segment.Fields[j] is Hl7v2Field field)
                    {
                        var unusedField = new UnusedHl7v2Field(j);
                        for (var k = 0; k < field.Components.Count; ++k)
                        {
                            if (field.Components[k] is Hl7v2Component component && component.IsAccessed == false)
                            {
                                var unusedComponent = new UnusedHl7v2Component(k, component.Value);
                                unusedField.Component.Add(unusedComponent);
                            }
                        }

                        if (unusedField.Component.Count > 0)
                        {
                            unusedSegment.Field.Add(unusedField);
                        }
                    }
                }

                if (unusedSegment.Field.Count > 0)
                {
                    unusedSegments.Add(unusedSegment);
                }
            }

            return new Hl7v2TraceInfo(unusedSegments);
        }
    }
}
