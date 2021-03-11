// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using DotLiquid;

namespace Microsoft.Health.Fhir.Liquid.Converter.Models
{
    public class CodeMappingDocument : Document
    {
        public CodeMappingDocument(List<CodeMapping> mapping)
        {
            NodeList = mapping.Cast<object>().ToList();
        }
    }
}
