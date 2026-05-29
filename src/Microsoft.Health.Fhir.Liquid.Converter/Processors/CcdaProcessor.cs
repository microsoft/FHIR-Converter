// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using DotLiquid;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Parsers;
using Microsoft.Health.Fhir.Liquid.Converter.Utilities;
using Microsoft.Health.MeasurementUtility;

namespace Microsoft.Health.Fhir.Liquid.Converter.Processors
{
    public class CcdaProcessor : BaseProcessor
    {
        private readonly IDataParser _parser = new CcdaDataParser();

        public CcdaProcessor(ProcessorSettings processorSettings, ILogger<CcdaProcessor> logger)
            : base(processorSettings, logger)
        {
        }

        protected override DefaultRootTemplateParentPath DefaultRootTemplateParentPath { get; set; } = DefaultRootTemplateParentPath.Ccda;

        protected override string InternalConvert(string data, string rootTemplate, ITemplateProvider templateProvider, TraceInfo traceInfo = null)
        {
            object ccdaData;
            using (ITimed inputDeserializationTime =
                Performance.TrackDuration(duration => LogTelemetry(FhirConverterMetrics.InputDeserializationDuration, duration)))
            {
                ccdaData = _parser.Parse(data);
            }

            return InternalConvertFromObject(ccdaData, rootTemplate, templateProvider, traceInfo);
        }
    }
}
