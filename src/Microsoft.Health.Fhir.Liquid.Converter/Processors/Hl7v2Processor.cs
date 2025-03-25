// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using DotLiquid;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Models.Hl7v2;
using Microsoft.Health.Fhir.Liquid.Converter.Parsers;
using Microsoft.Health.Fhir.Liquid.Converter.Utilities;
using Microsoft.Health.MeasurementUtility;

namespace Microsoft.Health.Fhir.Liquid.Converter.Processors
{
    public class Hl7v2Processor : BaseProcessor
    {
        private readonly IDataParser _parser = new Hl7v2DataParser();

        public Hl7v2Processor(ProcessorSettings processorSettings, ILogger<Hl7v2Processor> logger)
            : base(processorSettings, logger)
        {
        }

        protected override string DataKey { get; set; } = "hl7v2Data";

        protected override DefaultRootTemplateParentPath DefaultRootTemplateParentPath { get; set; } = DefaultRootTemplateParentPath.Hl7v2;

        protected override string InternalConvert(string data, string rootTemplate, ITemplateProvider templateProvider, TraceInfo traceInfo = null)
        {
            object hl7v2Data;
            using (ITimed inputDeserializationTime =
                Performance.TrackDuration(duration => LogTelemetry(FhirConverterMetrics.InputDeserializationDuration, duration)))
            {
                hl7v2Data = _parser.Parse(data);
            }

            return InternalConvertFromObject(hl7v2Data, rootTemplate, templateProvider, traceInfo);
        }

        protected override void CreateTraceInfo(object data, Context context, TraceInfo traceInfo)
        {
            if (traceInfo is Hl7v2TraceInfo hl7v2TraceInfo)
            {
                hl7v2TraceInfo.UnusedSegments = Hl7v2TraceInfo.CreateTraceInfo(data as Hl7v2Data).UnusedSegments;
            }
        }
    }
}
