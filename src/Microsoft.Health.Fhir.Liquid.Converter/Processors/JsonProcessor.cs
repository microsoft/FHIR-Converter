// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using DotLiquid;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Fhir.Liquid.Converter.Extensions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Models.Json;
using Microsoft.Health.Fhir.Liquid.Converter.Parsers;
using Microsoft.Health.MeasurementUtility;
using Newtonsoft.Json.Linq;
using NJsonSchema;

namespace Microsoft.Health.Fhir.Liquid.Converter.Processors
{
    public class JsonProcessor : BaseProcessor
    {
        private readonly IDataParser _parser = new JsonDataParser();

        public JsonProcessor(ProcessorSettings processorSettings, ILogger<JsonProcessor> logger)
            : base(processorSettings, logger)
        {
        }

        protected override DataType DataType { get; set; } = DataType.Json;

        protected override string InternalConvert(string data, string rootTemplate, ITemplateProvider templateProvider, TraceInfo traceInfo = null)
        {
            object jsonData;
            using (ITimed inputDeserializationTime =
                Performance.TrackDuration(duration => LogTelemetry(FhirConverterMetrics.InputDeserializationDuration, duration)))
            {
                jsonData = _parser.Parse(data);
            }

            return InternalConvertFromObject(jsonData, rootTemplate, templateProvider, traceInfo);
        }

        public string Convert(JObject data, string rootTemplate, ITemplateProvider templateProvider, TraceInfo traceInfo = null)
        {
            var jsonData = data.ToObject();
            return InternalConvertFromObject(jsonData, rootTemplate, templateProvider, traceInfo);
        }

        protected override Context CreateContext(ITemplateProvider templateProvider, IDictionary<string, object> data, string rootTemplate)
        {
            // Load data and templates
            var cancellationToken = Settings.TimeOut > 0 ? new CancellationTokenSource(Settings.TimeOut).Token : CancellationToken.None;
            var context = new JSchemaContext(
                environments: new List<Hash> { Hash.FromDictionary(data) },
                outerScope: new Hash(),
                registers: Hash.FromDictionary(new Dictionary<string, object> { { "file_system", templateProvider.GetTemplateFileSystem() } }),
                errorsOutputMode: ErrorsOutputMode.Rethrow,
                maxIterations: Settings.MaxIterations,
                formatProvider: CultureInfo.InvariantCulture,
                cancellationToken: cancellationToken)
            {
                ValidateSchemas = new List<JsonSchema>(),
            };

            // Load filters
            context.AddFilters(typeof(Filters));

            // Add root template's parent path to context.
            AddRootTemplatePathScope(context, templateProvider, rootTemplate);

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
