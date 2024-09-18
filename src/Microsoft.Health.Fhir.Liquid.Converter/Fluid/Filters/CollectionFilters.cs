// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using Fluid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Liquid.Converter.Fluid.Filters
{
    internal static class CollectionFilters
    {
        public static FilterCollection RegisterCollectionFilter(this FilterCollection filters)
        {
            EnsureArg.IsNotNull(filters, nameof(filters));

            filters.AddFilter("to_array", GeneralFilters.NullFilter);
            filters.AddFilter("concat", GeneralFilters.NullFilter);
            filters.AddFilter("batch_render", GeneralFilters.NullFilter);
            //filters.AddFilter("", GeneralFilters.NullFilter);
            //filters.AddFilter("", GeneralFilters.NullFilter);
            //filters.AddFilter("", GeneralFilters.NullFilter);
            //filters.AddFilter("", GeneralFilters.NullFilter);

            return filters;
        }
    }
}
