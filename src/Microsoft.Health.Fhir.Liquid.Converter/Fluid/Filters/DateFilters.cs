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
    internal static class DateFilters
    {
        public static FilterCollection RegisterDateFilter(this FilterCollection filters)
        {
            EnsureArg.IsNotNull(filters, nameof(filters));

            filters.AddFilter("add_hyphens_date", GeneralFilters.NullFilter);
            filters.AddFilter("format_as_date_time", GeneralFilters.NullFilter);
            filters.AddFilter("now", GeneralFilters.NullFilter);
            filters.AddFilter("add_seconds", GeneralFilters.NullFilter);

            // filters.AddFilter("date", GeneralFilters.NullFilter);

            return filters;
        }
    }
}
