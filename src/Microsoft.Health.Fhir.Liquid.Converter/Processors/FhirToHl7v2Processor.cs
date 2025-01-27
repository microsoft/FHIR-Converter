// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using DotLiquid;
using EnsureThat;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Extensions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Models.Hl7v2;
using Microsoft.Health.Fhir.Liquid.Converter.Models.Json;
using Microsoft.Health.Fhir.Liquid.Converter.Parsers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;

namespace Microsoft.Health.Fhir.Liquid.Converter.Processors
{
    public class FhirToHl7v2Processor : BaseProcessor
    {
        private static readonly JsonSerializerSettings DefaultSerializerSettings = new JsonSerializerSettings()
        {
            DateParseHandling = DateParseHandling.None,
        };

        private readonly IDataParser _parser;

        private string[] _segmentsWithFieldSeparator = new string[] { "MSH", "BHS", "FHS" };

        public FhirToHl7v2Processor(ProcessorSettings processorSettings, ILogger<FhirToHl7v2Processor> logger)
            : this(processorSettings, new JsonDataParser(), logger)
        {
        }

        public FhirToHl7v2Processor(ProcessorSettings processorSettings, IDataParser parser, ILogger<FhirToHl7v2Processor> logger)
            : base(processorSettings, logger)
        {
            _parser = EnsureArg.IsNotNull(parser, nameof(parser));
        }

        protected override DefaultRootTemplateParentPath DefaultRootTemplateParentPath { get; set; } = DefaultRootTemplateParentPath.FhirToHl7v2;

        protected override string InternalConvert(string data, string rootTemplate, ITemplateProvider templateProvider, TraceInfo traceInfo = null)
        {
            object jsonData = _parser.Parse(data);
            var result = InternalConvertFromObject(jsonData, rootTemplate, templateProvider, traceInfo);

            var hl7Message = GenerateHL7Message(ConvertToJObject(result));

            var hl7String = ConvertHl7MessageToString(hl7Message);

            return hl7String;
        }

        public string Convert(JObject data, string rootTemplate, ITemplateProvider templateProvider, TraceInfo traceInfo = null)
        {
            var jsonData = data.ToObject();
            var result = InternalConvertFromObject(jsonData, rootTemplate, templateProvider, traceInfo);
            var hl7Message = GenerateHL7Message(ConvertToJObject(result));

            var hl7String = ConvertHl7MessageToString(hl7Message);
            return hl7String;
        }

        public Hl7v2Data GenerateHL7Message(JObject transformations)
        {
            var hl7Segments = new List<Hl7v2Segment>();

            // Validate that the template has a messageDefinition section
            if (transformations["messageDefinition"] == null)
            {
                throw new RenderException(
                    FhirConverterErrorCode.PropertyNotFound,
                    "The messageDefinition JSON property was not found in the conversion template");
            }

            // Create HL7v2 segments based on the liquid template the user supplied
            for (int i = 0; i < transformations["messageDefinition"].Count(); i++)
            {
                var dataResolutionForSegment = transformations["messageDefinition"][i] as JObject;

                if (dataResolutionForSegment == null)
                {
                    throw new RenderException(
                        FhirConverterErrorCode.TemplateSyntaxError,
                        $"The segment mapping in position {i} specified in the messageDefinition is not a valid object");
                }

                var segmentName = string.Empty;
                try
                {
                    segmentName = dataResolutionForSegment.Properties().First().Name;
                    segmentName = segmentName.ToUpper();
                }
                catch
                {
                    throw new RenderException(
                        FhirConverterErrorCode.TemplateSyntaxError,
                        $"The segment name in position {i} specified in the messageDefinition does not exist or is invalid");
                }

                // Validate that the name of the segment is 3 characters long
                if (!segmentName.Length.Equals(3))
                {
                    throw new RenderException(
                        FhirConverterErrorCode.TemplateSyntaxError,
                        $"The segment name {segmentName} in position {i} is invalid. It must be 3 characters long");
                }

                JObject mappingsForSegment = null;
                try
                {
                    mappingsForSegment = dataResolutionForSegment.Properties().First().Value as JObject;

                    if (mappingsForSegment == null)
                    {
                        throw new Exception("The segment mapping for the template is invalid");
                    }
                }
                catch
                {
                    throw new RenderException(
                        FhirConverterErrorCode.TemplateSyntaxError,
                        $"The segment mapping in position {i} specified in the messageDefinition is not a valid object, or is empty");
                }

                // Fill in any gaps in fields for the given segment
                //   - For example if PID-3 is populated but not PID-1 or PID-2 then we should fill in placeholders up until and including PID-3
                //   - Get highest field position in each segment and fill in lower fields with blank fields
                var fields = new List<Hl7v2Field>();

                int highestField = 0;
                foreach (var mapping in mappingsForSegment)
                {
                    var fieldPosition = 0;
                    try
                    {
                        fieldPosition = int.Parse(mapping.Key);

                        if (fieldPosition < 1)
                        {
                            throw new Exception("Invalid field number");
                        }
                    }
                    catch
                    {
                        throw new RenderException(
                            FhirConverterErrorCode.TemplateSyntaxError,
                            $"The field number for the segment {segmentName} in position {i} is not valid. The value supplied was: {mapping.Key}");
                    }

                    if (fieldPosition > highestField)
                    {
                        highestField = fieldPosition;
                    }
                }

                for (int filler = 0; filler < highestField; filler++)
                {
                    fields.Add(new Hl7v2Field() { Value = string.Empty });
                }

                // replace any field values with the actual data resolved from the template
                foreach (var mapping in mappingsForSegment)
                {
                    var fieldPosition = mapping.Key;
                    var fieldValue = mapping.Value?.ToString();
                    var fieldPositionIndex = int.Parse(fieldPosition) - 1;

                    if (!_segmentsWithFieldSeparator.Contains(segmentName))
                    {
                        fields[fieldPositionIndex] = new Hl7v2Field() { Value = fieldValue };
                    }
                    else
                    {
                        // the fields in segments with field separators should have a fieldPositionIndex of fieldPositionIndex - 1
                        if (fieldPositionIndex >= 1)
                        {
                            fields[fieldPositionIndex - 1] = new Hl7v2Field() { Value = fieldValue };
                        }

                        if (fieldPositionIndex == fields.Count - 1)
                        {
                            fields.RemoveAt(fieldPositionIndex);
                        }
                    }
                }

                hl7Segments.Add(new Hl7v2Segment(string.Empty, fields) { SegmentName = segmentName });
            }

            return new Hl7v2Data() { Data = hl7Segments, EncodingCharacters = new Hl7v2EncodingCharacters() };
        }

        public string ConvertHl7MessageToString(Hl7v2Data message)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Hl7v2Segment segment in message.Data)
            {
                sb.Append(segment.SegmentName + message.EncodingCharacters.FieldSeparator);
                foreach (Hl7v2Field field in segment.Fields)
                {
                    sb.Append(field.Value);
                    sb.Append(message.EncodingCharacters.FieldSeparator);
                }

                sb.AppendLine();
            }

            return sb.ToString();
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

            return context;
        }

        protected override void CreateTraceInfo(object data, Context context, TraceInfo traceInfo)
        {
            if ((traceInfo is JSchemaTraceInfo jsonTraceInfo) && (context is JSchemaContext jsonContext))
            {
                jsonTraceInfo.ValidateSchemas = jsonContext.ValidateSchemas;
            }
        }

        private JObject ConvertToJObject(string input)
        {
            return JsonConvert.DeserializeObject<JObject>(input, DefaultSerializerSettings);
        }
    }
}
