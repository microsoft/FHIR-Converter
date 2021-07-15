// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.Models;

namespace Microsoft.Health.Fhir.Liquid.Converter.Hl7v2
{
    public class Hl7v2TemplateProvider : TemplateProvider
    {
        public Hl7v2TemplateProvider(string templateDirectory)
            : base(templateDirectory, DataType.Hl7v2)
        {
        }

        public Hl7v2TemplateProvider(List<Dictionary<string, Template>> templateCollection)
            : base(templateCollection)
        {
        }
    }
}
