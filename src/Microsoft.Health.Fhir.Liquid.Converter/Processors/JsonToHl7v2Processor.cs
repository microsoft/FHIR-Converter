// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Extensions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Models.Hl7v2;
using Microsoft.Health.Fhir.Liquid.Converter.Models.Json;
using Microsoft.Health.Fhir.Liquid.Converter.Parsers;
using Microsoft.Health.Fhir.Liquid.Converter.Telemetry;
using Microsoft.Health.Logging.Telemetry;
using Newtonsoft.Json.Linq;
using NJsonSchema;

namespace Microsoft.Health.Fhir.Liquid.Converter.Processors
{
    public class JsonToHl7v2Processor : BaseProcessor
    {
        private readonly IDataParser _parser = new JsonDataParser();

        public JsonToHl7v2Processor(ProcessorSettings processorSettings, ITelemetryLogger telemetryLogger)
            : base(processorSettings, telemetryLogger)
        {
        }

        protected override string InternalConvert(string data, string rootTemplate, ITemplateProvider templateProvider, TraceInfo traceInfo = null)
        {
            object jsonData;
            using (ITimed inputDeserializationTime = TelemetryLogger.TrackDuration(ConverterMetrics.InputDeserializationDuration))
            {
                jsonData = _parser.Parse(data);
            }

            var result = InternalConvertFromObject(jsonData, rootTemplate, templateProvider, traceInfo);

            var hl7Message = GenerateHL7Message(JObject.Parse(result));

            var hl7String = ConvertHl7MessageToString(hl7Message);

            return hl7String;
        }

        public string Convert(JObject data, string rootTemplate, ITemplateProvider templateProvider, TraceInfo traceInfo = null)
        {
            var jsonData = data.ToObject();
            var result = InternalConvertFromObject(jsonData, rootTemplate, templateProvider, traceInfo);
            return result.ToString();
        }

        public MinimalHl7v2Message GenerateHL7Message(JObject transformations)
        {
            var hl7Segments = new List<MinimalHl7v2Segment>();

            // validate that the template has a messageDefinition section
            if (transformations["messageDefinition"] == null)
            {
                throw new RenderException(FhirConverterErrorCode.TemplateRenderingError, "The messageDefinition section was not found in the conversion template");
            }

            // create HL7v2 segments
            for (int i = 0; i < transformations["messageDefinition"].Count(); i++)
            {
                var dataResolution = transformations["messageDefinition"][i] as JObject;
                var segmentName = dataResolution.Properties().First().Name;

                var fields = new List<string>();

                JObject mappingsForSegment = dataResolution.Properties().First().Value as JObject;

                // Fill in any gaps in fields for the given segment
                //   - For example if PID-3 is populated but not PID-1 or PID-2 then we should fill in placeholders up until and including PID-3
                //   - Get highest field position in each segment and fill in lower fields with blank fields
                int highestField = 0;
                foreach (var mapping in mappingsForSegment)
                {
                    var fieldPosition = int.Parse(mapping.Key);
                    if (fieldPosition > highestField)
                    {
                        highestField = fieldPosition;
                    }
                }

                for (int filler = 0; filler < highestField; filler++)
                {
                    fields.Add(string.Empty);
                }

                // replace any field values with the actual data resolved from the template
                foreach (var mapping in mappingsForSegment)
                {
                    var fieldPosition = mapping.Key;
                    var fieldValue = mapping.Value?.ToString();
                    var fieldPositionIndex = int.Parse(fieldPosition) - 1;

                    fields[fieldPositionIndex] = fieldValue;
                }

                hl7Segments.Add(new MinimalHl7v2Segment(segmentName, fields));
            }

            return new MinimalHl7v2Message(hl7Segments);
        }

        public string ConvertHl7MessageToString(MinimalHl7v2Message message)
        {
            StringBuilder sb = new StringBuilder();
            foreach (MinimalHl7v2Segment segment in message.Segments)
            {
                sb.Append(segment.SegmentName + message.Hl7v2EncodingCharacters.FieldSeparator);
                foreach (var field in segment.Fields)
                {
                    sb.Append(field);
                    sb.Append(message.Hl7v2EncodingCharacters.FieldSeparator);
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        protected override Context CreateContext(ITemplateProvider templateProvider, IDictionary<string, object> data)
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
