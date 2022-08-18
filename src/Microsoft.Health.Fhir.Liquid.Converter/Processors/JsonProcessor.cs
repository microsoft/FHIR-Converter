// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Globalization;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.Extensions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Models.Json;
using Microsoft.Health.Fhir.Liquid.Converter.Parsers;
using Newtonsoft.Json.Linq;
using NJsonSchema;

namespace Microsoft.Health.Fhir.Liquid.Converter.Processors
{
    public class JsonProcessor : BaseProcessor
    {
        private readonly IDataParser _parser = new JsonDataParser();

        public JsonProcessor(ProcessorSettings processorSettings)
            : base(processorSettings)
        {
        }

        public override string Convert(string data, string rootTemplate, ITemplateProvider templateProvider, TraceInfo traceInfo = null)
        {
            var jsonData = _parser.Parse(data);
            return Convert(jsonData, rootTemplate, templateProvider, traceInfo);
        }

        public string Convert(JObject data, string rootTemplate, ITemplateProvider templateProvider, TraceInfo traceInfo = null)
        {
            var jsonData = data.ToObject();
            return Convert(jsonData, rootTemplate, templateProvider, traceInfo);
        }

        protected override Context CreateContext(ITemplateProvider templateProvider, IDictionary<string, object> data)
        {
            // Load data and templates
            var timeout = Settings.TimeOut;
            var context = new JSchemaContext(
                environments: new List<Hash> { Hash.FromDictionary(data) },
                outerScope: new Hash(),
                registers: Hash.FromDictionary(new Dictionary<string, object> { { "file_system", templateProvider.GetTemplateFileSystem() } }),
                errorsOutputMode: ErrorsOutputMode.Rethrow,
                maxIterations: Settings.MaxIterations,
                timeout: timeout,
                formatProvider: CultureInfo.InvariantCulture)
            {
                ValidateSchemas = new List<JsonSchema>(),
            };

            // Load filters
            context.AddFilters(typeof(Filters));

            return context;
        }

        protected override void CreateTraceInfo(object data, Context context, TraceInfo traceInfo)
        {
            if ((traceInfo is JSchemaTraceInfo jsonTraceInfo) && (context is JSchemaContext jsonContext))
            {
                jsonTraceInfo.ValidateSchemas = jsonContext.ValidateSchemas;
            }
        }
    }
}
