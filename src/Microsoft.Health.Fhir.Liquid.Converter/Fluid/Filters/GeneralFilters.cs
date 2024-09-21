// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using EnsureThat;
using Fluid;
using Fluid.Values;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using FilterImpl = Microsoft.Health.Fhir.Liquid.Converter;

namespace Microsoft.Health.Fhir.Liquid.Converter.Fluid.Filters
{
    internal static class GeneralFilters
    {
        public static FilterCollection RegisterGeneralFilter(this FilterCollection filters)
        {
            EnsureArg.IsNotNull(filters, nameof(filters));

            filters.AddFilter("get_property", GetProperty);
            filters.AddFilter("generate_uuid", GenerateUuid);
            filters.AddFilter("generate_id_input", GenerateIdInput);

            return filters;
        }

        internal static ValueTask<FluidValue> NullFilter(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            return new ValueTask<FluidValue>(new StringValue(string.Empty));
        }

        private static ValueTask<FluidValue> GetProperty(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            // Example {{ PID.8.Value | get_property: 'CodeSystem/Gender', 'code' }}

            var originalCode = input.ToStringValue();
            var mapping = arguments.At(0).ToStringValue();
            var property = arguments.Count < 1 ? "code" : arguments.At(1).ToStringValue();

            var map = context.GetCodeMapping()?.Mapping?.GetValueOrDefault(mapping, null);
            var codeMapping = map?.GetValueOrDefault(originalCode, null) ?? map?.GetValueOrDefault("__default__", null);
            var retVal = codeMapping?.GetValueOrDefault(property, null)
                ?? ((property.Equals("code") || property.Equals("display")) ? originalCode : null);

            return new ValueTask<FluidValue>(new StringValue(retVal));
        }

        private static ValueTask<FluidValue> GenerateUuid(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            // Example {{ identifiers | generate_id_input: 'Task', true, baseId | generate_uuid }}

            var value = input.ToStringValue();

            return new ValueTask<FluidValue>(new ObjectValue(FilterImpl.Filters.GenerateUUID(value)));
        }

        private static ValueTask<FluidValue> GenerateIdInput(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            // Example {{ identifiers | generate_id_input: 'Task', true, baseId | generate_uuid }}

            var segment = input.ToStringValue();
            var resourceType = arguments.At(0).ToStringValue();
            var isBaseIdRequired = arguments.At(1).ToBooleanValue();
            string baseId = arguments.Count < 3 ? null : arguments.At(2).ToStringValue();

            return new ValueTask<FluidValue>(new ObjectValue(FilterImpl.Filters.GenerateIdInput(segment, resourceType, isBaseIdRequired, baseId)));
        }
    }
}
