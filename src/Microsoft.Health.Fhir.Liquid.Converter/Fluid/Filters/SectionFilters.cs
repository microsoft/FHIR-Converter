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
    internal static class SectionFilters
    {
        public static FilterCollection RegisterSectionFilter(this FilterCollection filters)
        {
            EnsureArg.IsNotNull(filters, nameof(filters));

            filters.AddFilter("get_first_ccda_sections", GeneralFilters.NullFilter);
            filters.AddFilter("get_ccda_section_lists", GeneralFilters.NullFilter);
            filters.AddFilter("get_first_ccda_sections_by_template_id", GeneralFilters.NullFilter);

            return filters;
        }
    }
}
