// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using DotLiquid;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Liquid.Converter.Models.Json
{
    public class JsonContentDocument : Document
    {
        public JsonContentDocument(List<JObject> contents)
        {
            NodeList = contents.Cast<object>().ToList();
        }
    }
}
