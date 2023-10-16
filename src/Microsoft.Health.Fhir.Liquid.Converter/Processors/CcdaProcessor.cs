// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Parsers;
using Microsoft.Health.Fhir.Liquid.Converter.Telemetry;
using Microsoft.Health.Logging.Telemetry;

namespace Microsoft.Health.Fhir.Liquid.Converter.Processors
{
    public class CcdaProcessor : BaseProcessor
    {
        private readonly IDataParser _parser = new CcdaDataParser();

        public CcdaProcessor(ProcessorSettings processorSettings, ITelemetryLogger telemetryLogger)
            : base(processorSettings, telemetryLogger)
        {
        }

        protected override string InternalConvert(string data, string rootTemplate, ITemplateProvider templateProvider, TraceInfo traceInfo = null)
        {
            object ccdaData;
            using (ITimed inputDeserializationTime = TelemetryLogger.TrackDuration(ConverterMetrics.InputDeserializationDuration))
            {
                ccdaData = _parser.Parse(data);
            }

            return InternalConvertFromObject(ccdaData, rootTemplate, templateProvider, traceInfo);
        }

        protected override Context CreateContext(ITemplateProvider templateProvider, IDictionary<string, object> data)
        {
            // Load value set mapping
            var context = base.CreateContext(templateProvider, data);
            var codeMapping = templateProvider.GetTemplate("ValueSet/ValueSet");
            if (codeMapping?.Root?.NodeList?.First() != null)
            {
                context["CodeMapping"] = codeMapping.Root.NodeList.First();
            }

            return context;
        }
    }
}
