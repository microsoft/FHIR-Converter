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
    internal static class StringFilters
    {
        public static FilterCollection RegisterStringFilter(this FilterCollection filters)
        {
            EnsureArg.IsNotNull(filters, nameof(filters));

            filters.AddFilter("char_at", GeneralFilters.NullFilter);
            filters.AddFilter("contains", GeneralFilters.NullFilter);
            filters.AddFilter("escape_special_chars", GeneralFilters.NullFilter);
            filters.AddFilter("unescape_special_chars", GeneralFilters.NullFilter);
            filters.AddFilter("match", GeneralFilters.NullFilter);
            filters.AddFilter("to_json_string", GeneralFilters.NullFilter);
            filters.AddFilter("to_double", GeneralFilters.NullFilter);
            filters.AddFilter("base64_encode", GeneralFilters.NullFilter);
            filters.AddFilter("base64_decode", GeneralFilters.NullFilter);
            filters.AddFilter("sha1_hash", GeneralFilters.NullFilter);
            filters.AddFilter("gzip", GeneralFilters.NullFilter);
            filters.AddFilter("gunzip_base64_string", GeneralFilters.NullFilter);

            return filters;
        }
    }
}
