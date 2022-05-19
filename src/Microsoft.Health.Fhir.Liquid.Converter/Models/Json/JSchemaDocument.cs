// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using DotLiquid;
using Newtonsoft.Json.Schema;

namespace Microsoft.Health.Fhir.Liquid.Converter.Models.Json
{
    public class JSchemaDocument : Document
    {
        public JSchemaDocument(List<JSchema> contents)
        {
            NodeList = contents.Cast<object>().ToList();
        }
    }
}
