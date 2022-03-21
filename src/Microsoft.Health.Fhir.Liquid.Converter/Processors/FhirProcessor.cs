// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Models.Hl7v2;
using Microsoft.Health.Fhir.Liquid.Converter.Parsers;

namespace Microsoft.Health.Fhir.Liquid.Converter.Processors
{
    public class FhirProcessor : BaseProcessor
    {
        private readonly IDataParser _parser = new FhirDataParser();

        public FhirProcessor(ProcessorSettings processorSettings = null)
            : base(processorSettings)
        {
        }

        protected override string DataKey { get; set; } = "fhirData";

        public override string Convert(string data, string rootTemplate, ITemplateProvider templateProvider, TraceInfo traceInfo = null)
        {
            var fhirData = _parser.Parse(data);
            return Convert(fhirData, rootTemplate, templateProvider, traceInfo);
        }
    }
}