// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnsureThat;
using Fluid;
using Fluid.Ast;
using Fluid.Values;

namespace Microsoft.Health.Fhir.Liquid.Converter.Fluid.Filters
{
    internal static class GeneralFilters
    {
        public static FilterCollection RegisterGeneralFilter(this FilterCollection filters)
        {
            EnsureArg.IsNotNull(filters, nameof(filters));

            filters.AddFilter("get_property", NullFilter);
            filters.AddFilter("generate_uuid", NullFilter);
            filters.AddFilter("generate_id_input", NullFilter);

            return filters;
        }

        internal static ValueTask<FluidValue> NullFilter(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            return new ValueTask<FluidValue>(new StringValue(string.Empty));
        }
    }
}
