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

namespace Microsoft.Health.Fhir.Liquid.Converter.Fluid.Filters
{
    internal static class MathFilters
    {
        public static FilterCollection RegisterMathFilter(this FilterCollection filters)
        {
            EnsureArg.IsNotNull(filters, nameof(filters));

            filters.AddFilter("is_nan", GeneralFilters.NullFilter);
            filters.AddFilter("abs", GeneralFilters.NullFilter);
            filters.AddFilter("pow", GeneralFilters.NullFilter);
            filters.AddFilter("random", GeneralFilters.NullFilter);
            filters.AddFilter("sign", GeneralFilters.NullFilter);
            filters.AddFilter("truncate_number", GeneralFilters.NullFilter);
            filters.AddFilter("divide", GeneralFilters.NullFilter);

            return filters;
        }
    }
}
