// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Parsers;

namespace Microsoft.Health.Fhir.Liquid.Converter.Fluid
{
    public class FluidHl7v2Processor : BaseFluidProcessor
    {
        private readonly IDataParser _parser = new Hl7v2DataParser();

        public FluidHl7v2Processor(ProcessorSettings processorSettings, Microsoft.Extensions.Logging.ILogger<BaseFluidProcessor> logger)
            : base(processorSettings, logger)
        {
        }

        protected override IDictionary<string, object> CreateDataModel(string data, TraceInfo traceInfo = null)
        {
            object parsedHl7Data = _parser.Parse(data);
            return new Dictionary<string, object>
            {
                { "hl7v2Data", parsedHl7Data },
            };
        }
    }
}
