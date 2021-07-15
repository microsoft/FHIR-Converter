// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.Models;

namespace Microsoft.Health.Fhir.Liquid.Converter.Json
{
    public class JsonTemplateProvider : TemplateProvider
    {
        public JsonTemplateProvider(string templateDirectory)
            : base(templateDirectory, DataType.Json)
        {
        }

        public JsonTemplateProvider(List<Dictionary<string, Template>> templateCollection)
            : base(templateCollection)
        {
        }
    }
}