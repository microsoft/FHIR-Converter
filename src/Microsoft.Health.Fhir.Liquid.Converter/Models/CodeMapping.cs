// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using DotLiquid;

namespace Microsoft.Health.Fhir.Liquid.Converter.Models
{
    public class CodeMapping : Drop
    {
        public CodeMapping(Dictionary<string, Dictionary<string, Dictionary<string, string>>> mapping)
        {
            Mapping = mapping;
        }

        public Dictionary<string, Dictionary<string, Dictionary<string, string>>> Mapping { get; set; }
    }
}
