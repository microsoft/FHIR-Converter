// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using DotLiquid;

namespace Microsoft.Health.Fhir.Liquid.Converter.Hl7v2.Models
{
    public class CodeSystemMappingDocument : Document
    {
        public CodeSystemMappingDocument(List<CodeSystemMapping> mapping)
        {
            NodeList = mapping.Cast<object>().ToList();
        }
    }
}
