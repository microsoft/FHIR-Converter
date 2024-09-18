// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using EnsureThat;
using Fluid;
using Fluid.Values;
using Microsoft.Health.Fhir.Liquid.Converter.Models.Hl7v2;
using FilterImpl = Microsoft.Health.Fhir.Liquid.Converter;

namespace Microsoft.Health.Fhir.Liquid.Converter.Fluid.Filters
{
    internal static class SegmentFilters
    {
        public static FilterCollection RegisterSegmentFilter(this FilterCollection filters)
        {
            EnsureArg.IsNotNull(filters, nameof(filters));

            filters.AddFilter("get_first_segments", GetFirstSegments);
            filters.AddFilter("get_segment_lists", GetSegmentLists);
            filters.AddFilter("get_related_segment_list", GetRelatedSegmentList);
            filters.AddFilter("get_parent_segment", GetParentSegment);
            filters.AddFilter("has_segments", HasSegments);
            filters.AddFilter("split_data_by_segments", SplitDataBySegments);

            return filters;
        }

        private static ValueTask<FluidValue> GetFirstSegments(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var hl7v2Data = input.ToObjectValue() as Hl7v2Data;
            var segementIdContent = arguments.At(0).ToStringValue();

            return new ValueTask<FluidValue>(new ObjectValue(FilterImpl.Filters.GetFirstSegments(hl7v2Data, segementIdContent)));
        }

        private static ValueTask<FluidValue> GetSegmentLists(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var hl7v2Data = input.ToObjectValue() as Hl7v2Data;
            var segementIdContent = arguments.At(0).ToStringValue();

            return new ValueTask<FluidValue>(new ObjectValue(FilterImpl.Filters.GetSegmentLists(hl7v2Data, segementIdContent)));
        }

        private static ValueTask<FluidValue> GetRelatedSegmentList(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var hl7v2Data = input.ToObjectValue() as Hl7v2Data;
            var parentSegment = arguments.At(0).ToObjectValue() as Hl7v2Segment;
            var childSegmentId = arguments.At(1).ToStringValue();

            return new ValueTask<FluidValue>(new ObjectValue(FilterImpl.Filters.GetRelatedSegmentList(hl7v2Data, parentSegment, childSegmentId)));
        }

        private static ValueTask<FluidValue> GetParentSegment(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var hl7v2Data = input.ToObjectValue() as Hl7v2Data;
            var childSegmentId = arguments.At(0).ToStringValue();
            var childIndex = Convert.ToInt32(arguments.At(1).ToNumberValue());
            var parentSegmentId = arguments.At(2).ToStringValue();

            return new ValueTask<FluidValue>(new ObjectValue(FilterImpl.Filters.GetParentSegment(hl7v2Data, childSegmentId, childIndex, parentSegmentId)));
        }

        private static ValueTask<FluidValue> HasSegments(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var hl7v2Data = input.ToObjectValue() as Hl7v2Data;
            var segmentIdContent = arguments.At(0).ToStringValue();

            return new ValueTask<FluidValue>(new ObjectValue(FilterImpl.Filters.HasSegments(hl7v2Data, segmentIdContent)));
        }

        private static ValueTask<FluidValue> SplitDataBySegments(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var hl7v2Data = input.ToObjectValue() as Hl7v2Data;
            var segmentIdSeparators = arguments.At(0).ToStringValue();

            return new ValueTask<FluidValue>(new ObjectValue(FilterImpl.Filters.SplitDataBySegments(hl7v2Data, segmentIdSeparators)));
        }
    }
}
