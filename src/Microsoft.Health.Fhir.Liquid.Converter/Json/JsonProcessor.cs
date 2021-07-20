// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Fhir.Liquid.Converter.Models;

namespace Microsoft.Health.Fhir.Liquid.Converter.Json
{
    public class JsonProcessor : BaseProcessor
    {
        private readonly JsonDataParser _dataParser = new JsonDataParser();

        public JsonProcessor(ProcessorSettings processorSettings = null)
            : base(processorSettings)
        {
        }

        public override string Convert(string data, string rootTemplate, ITemplateProvider templateProvider, TraceInfo traceInfo = null)
        {
            var jsonData = _dataParser.Parse(data);
            return Convert(jsonData, rootTemplate, templateProvider, traceInfo);
        }
    }
}
