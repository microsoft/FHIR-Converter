// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Extensions;
using Microsoft.Health.Fhir.Liquid.Converter.InputProcessors;

namespace Microsoft.Health.Fhir.Liquid.Converter.Models.Hl7v2
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

        public static Hl7v2TraceInfo CreateTraceInfo(Hl7v2Data hl7v2Data)
        {
            var unusedSegments = new List<UnusedHl7v2Segment>();
            try
            {
                for (var i = 0; i < hl7v2Data?.Data?.Count; ++i)
                {
                    var segmentHeader = hl7v2Data.Meta[i];
                    var segment = hl7v2Data.Data[i];
                    var unusedSegment = new UnusedHl7v2Segment(i);
                    unusedSegment.Type = segmentHeader;

                    for (var j = 0; j < segment?.Fields?.Count; ++j)
                    {
                        // Field separator and encoding characters field are treated as accessed
                        if (i == 0 && j <= 2)
                        {
                            continue;
                        }

                        if (j > 0 && segment.Fields[j] is Hl7v2Field field)
                        {
                            var unusedComponents = new List<UnusedHl7v2Component>();
                            for (var k = 0; k < field.Components.Count; ++k)
                            {
                                if (field.Components[k] is Hl7v2Component component && component.IsAccessed == false)
                                {
                                    var indexInSegment = FindOffsetInSegment(segmentHeader, segment.Value, hl7v2Data.EncodingCharacters, j, k - 1);
                                    var unusedComponent = new UnusedHl7v2Component(indexInSegment, indexInSegment + component.Value.Length, component.Value);
                                    unusedComponents.Add(unusedComponent);
                                }
                            }

                            if (unusedComponents.Count > 0)
                            {
                                unusedSegment.Components.AddRange(unusedComponents);
                            }
                        }
                    }

                    if (unusedSegment.Components.Count > 0)
                    {
                        unusedSegments.Add(unusedSegment);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new PostprocessException(FhirConverterErrorCode.TraceInfoError, string.Format(Resources.TraceInfoError, ex.Message));
            }

            return new Hl7v2TraceInfo(unusedSegments);
        }

        private static int FindOffsetInSegment(string segmentHeader, string segmentValue, Hl7v2EncodingCharacters encodingCharacters, int fieldIndex, int componentIndex)
        {
            // All values($segmentValue and $fieldValue) need to be unescaped firstly (from "\\" back to "\"),
            // or the length will be incorrectly calculated
            segmentValue = SpecialCharProcessor.Unescape(segmentValue);

            // MSH segment should be treated separately since the first '|' in MSH segment is a special field
            if (segmentHeader.Equals("MSH"))
            {
                fieldIndex--;
            }

            var startFieldIndex = segmentValue.IndexOfNthOccurrence(encodingCharacters.FieldSeparator, fieldIndex) + 1;
            var endFieldIndex = segmentValue.IndexOfNthOccurrence(encodingCharacters.FieldSeparator, fieldIndex + 1);
            if (endFieldIndex == -1)
            {
                endFieldIndex = segmentValue.Length;
            }

            var fieldValue = SpecialCharProcessor.Unescape(segmentValue.Substring(startFieldIndex, endFieldIndex - startFieldIndex));
            return startFieldIndex + fieldValue.IndexOfNthOccurrence(encodingCharacters.ComponentSeparator, componentIndex) + 1;
        }
    }
}
