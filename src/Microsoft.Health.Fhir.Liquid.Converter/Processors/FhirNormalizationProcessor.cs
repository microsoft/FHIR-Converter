// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.Health.Fhir.Liquid.Converter.Models;

namespace Microsoft.Health.Fhir.Liquid.Converter.Processors
{
    public class FhirNormalizationProcessor
        : JsonProcessor
    {
        public FhirNormalizationProcessor(ProcessorSettings processorSettings, ILogger<FhirNormalizationProcessor> logger)
            : base(processorSettings, logger)
        {
        }

        protected override DefaultRootTemplateParentPath DefaultRootTemplateParentPath { get; set; } = DefaultRootTemplateParentPath.FhirNormalization;

        protected override string InternalConvert(string data, string rootTemplate, ITemplateProvider templateProvider, TraceInfo traceInfo = null)
        {
            return base.InternalConvert(data, rootTemplate, templateProvider, traceInfo);
        }
    }
}
