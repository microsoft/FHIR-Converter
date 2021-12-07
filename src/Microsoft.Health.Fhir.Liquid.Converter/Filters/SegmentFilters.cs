// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Health.Fhir.Liquid.Converter.Models.Hl7v2;

namespace Microsoft.Health.Fhir.Liquid.Converter
{
    /// <summary>
    /// Filters for conversion
    /// </summary>
    public partial class Filters
    {
        public static Dictionary<string, Hl7v2Segment> GetFirstSegments(Hl7v2Data hl7v2Data, string segmentIdContent)
        {
            var result = new Dictionary<string, Hl7v2Segment>();
            var segmentIds = segmentIdContent.Split(@"|");
            for (var i = 0; i < hl7v2Data.Meta.Count; ++i)
            {
                if (segmentIds.Contains(hl7v2Data.Meta[i]) && !result.ContainsKey(hl7v2Data.Meta[i]))
                {
                    result[hl7v2Data.Meta[i]] = hl7v2Data.Data[i];
                }
            }

            return result;
        }

        public static Dictionary<string, List<Hl7v2Segment>> GetSegmentLists(Hl7v2Data hl7v2Data, string segmentIdContent)
        {
            var segmentIds = segmentIdContent.Split(@"|");
            return GetSegmentListsInternal(hl7v2Data, segmentIds);
        }

        public static Dictionary<string, List<Hl7v2Segment>> GetRelatedSegmentList(Hl7v2Data hl7v2Data, Hl7v2Segment parentSegment, string childSegmentId)
        {
            var result = new Dictionary<string, List<Hl7v2Segment>>();
            List<Hl7v2Segment> segments = new List<Hl7v2Segment>();
            var parentFound = false;
            var childIndex = -1;

            for (var i = 0; i < hl7v2Data.Meta.Count; ++i)
            {
                if (ReferenceEquals(hl7v2Data.Data[i], parentSegment))
                {
                    parentFound = true;
                }
                else if (string.Equals(hl7v2Data.Meta[i], childSegmentId, StringComparison.InvariantCultureIgnoreCase) && parentFound)
                {
                    childIndex = i;
                    break;
                }
            }

            if (childIndex > -1)
            {
                while (childIndex < hl7v2Data.Meta.Count && string.Equals(hl7v2Data.Meta[childIndex], childSegmentId, StringComparison.InvariantCultureIgnoreCase))
                {
                    segments.Add(hl7v2Data.Data[childIndex]);
                    childIndex++;
                }

                result[childSegmentId] = segments;
            }

            return result;
        }

        public static Dictionary<string, Hl7v2Segment> GetParentSegment(Hl7v2Data hl7v2Data, string childSegmentId, int childIndex, string parentSegmentId)
        {
            var result = new Dictionary<string, Hl7v2Segment>();
            var targetChildIndex = -1;
            var foundChildSegmentCount = -1;

            for (var i = 0; i < hl7v2Data.Meta.Count; ++i)
            {
                if (string.Equals(hl7v2Data.Meta[i], childSegmentId, StringComparison.InvariantCultureIgnoreCase))
                {
                    foundChildSegmentCount++;
                    if (foundChildSegmentCount == childIndex)
                    {
                        targetChildIndex = i;
                        break;
                    }
                }
            }

            for (var i = targetChildIndex; i > -1; i--)
            {
                if (string.Equals(hl7v2Data.Meta[i], parentSegmentId, StringComparison.InvariantCultureIgnoreCase))
                {
                    result[parentSegmentId] = hl7v2Data.Data[i];
                    break;
                }
            }

            return result;
        }

        public static bool HasSegments(Hl7v2Data hl7v2Data, string segmentIdContent)
        {
            var segmentIds = segmentIdContent.Split(@"|");
            var segmentLists = GetSegmentListsInternal(hl7v2Data, segmentIds);
            return segmentIds.All(segmentLists.ContainsKey);
        }

        private static Dictionary<string, List<Hl7v2Segment>> GetSegmentListsInternal(Hl7v2Data hl7v2Data, string[] segmentIds)
        {
            var result = new Dictionary<string, List<Hl7v2Segment>>();
            for (var i = 0; i < hl7v2Data.Meta.Count; ++i)
            {
                if (segmentIds.Contains(hl7v2Data.Meta[i]))
                {
                    if (result.ContainsKey(hl7v2Data.Meta[i]))
                    {
                        result[hl7v2Data.Meta[i]].Add(hl7v2Data.Data[i]);
                    }
                    else
                    {
                        result[hl7v2Data.Meta[i]] = new List<Hl7v2Segment> { hl7v2Data.Data[i] };
                    }
                }
            }

            return result;
        }

        public static List<Hl7v2Data> SliceDataBySegments(Hl7v2Data hl7v2Data, List<Hl7v2Segment> segmentList, Hl7v2Segment endSegment = null)
        {
            var results = new List<Hl7v2Data>();
            var result = new Hl7v2Data();
            string[] segmentSeparators = { "\r\n", "\r", "\n" };
            var valueList = hl7v2Data.Value.Split(segmentSeparators, StringSplitOptions.RemoveEmptyEntries);
            var startSegmentFound = false;
            var segmentIndex = 0;
            for (var i = 0; i < hl7v2Data.Meta.Count; ++i)
            {
                if (segmentIndex == 0 && segmentList.Count > 0 && ReferenceEquals(hl7v2Data.Data[i], segmentList[segmentIndex]))
                {
                    startSegmentFound = true;
                    segmentIndex++;
                }
                else if (segmentIndex > 0)
                {
                    if (endSegment != null && ReferenceEquals(hl7v2Data.Data[i], endSegment))
                    {
                        break;
                    }
                    else if (segmentIndex < segmentList.Count && ReferenceEquals(hl7v2Data.Data[i], segmentList[segmentIndex]))
                    {
                        results.Add(result);
                        result = new Hl7v2Data();
                        segmentIndex++;
                    }
                }

                if (startSegmentFound)
                {
                    result.Meta.Add(hl7v2Data.Meta[i]);
                    result.Data.Add(hl7v2Data.Data[i]);
                    result.Value += valueList[i] + System.Environment.NewLine;
                }
            }

            if (result.Meta.Count > 0)
            {
                results.Add(result);
            }

            return results;
        }
    }
}
