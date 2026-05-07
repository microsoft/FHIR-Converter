// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using DotLiquid;
using EnsureThat;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Extensions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.OutputProcessors;
using Microsoft.Health.Fhir.Liquid.Converter.Parsers;
using Microsoft.Health.Fhir.Liquid.Converter.Utilities;
using Microsoft.Health.MeasurementUtility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Liquid.Converter.Processors
{
    public class FhirR4Processor : JsonProcessor
    {
        private readonly IDataParser _dataParser;

        public FhirR4Processor(ProcessorSettings processorSettings, ILogger<FhirR4Processor> logger)
            : this(processorSettings, new JsonDataParser(), logger)
        {
        }

        private FhirR4Processor(ProcessorSettings processorSettings, IDataParser parser, ILogger<FhirR4Processor> logger)
            : base(processorSettings, parser, logger)
        {
            _dataParser = EnsureArg.IsNotNull(parser, nameof(parser));
        }

        protected override DefaultRootTemplateParentPath DefaultRootTemplateParentPath { get; set; } = DefaultRootTemplateParentPath.FhirR4;

        protected override string InternalConvert(string data, string rootTemplate, ITemplateProvider templateProvider, TraceInfo traceInfo = null)
        {
            return base.InternalConvert(data, rootTemplate, templateProvider, traceInfo);
        }

        public override string Convert(string data, string rootTemplate, ITemplateProvider templateProvider, IDictionary<string, string> variables, TraceInfo traceInfo = null)
        {
            string result;
            using (ITimed totalConversionTime =
                Performance.TrackDuration(duration => LogTelemetry(FhirConverterMetrics.TotalDuration, duration)))
            {
                result = InternalConvertWithVariables(data, rootTemplate, templateProvider, variables, traceInfo);
            }

            return result;
        }

        public override string Convert(string data, string rootTemplate, ITemplateProvider templateProvider, IDictionary<string, string> variables, CancellationToken cancellationToken, TraceInfo traceInfo = null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string result;
            using (ITimed totalConversionTime =
                Performance.TrackDuration(duration => LogTelemetry(FhirConverterMetrics.TotalDuration, duration)))
            {
                result = InternalConvertWithVariables(data, rootTemplate, templateProvider, variables, traceInfo);
            }

            return result;
        }

        private string InternalConvertWithVariables(string data, string rootTemplate, ITemplateProvider templateProvider, IDictionary<string, string> variables, TraceInfo traceInfo = null)
        {
            if (string.IsNullOrEmpty(rootTemplate))
            {
                throw new RenderException(FhirConverterErrorCode.NullOrEmptyRootTemplate, Resources.NullOrEmptyRootTemplate);
            }

            if (templateProvider == null)
            {
                throw new RenderException(FhirConverterErrorCode.NullTemplateProvider, Resources.NullTemplateProvider);
            }

            VariableValidator.ValidateVariables(variables);

            // Step: Parse input
            object jsonData;
            using (ITimed inputDeserializationTime =
                Performance.TrackDuration(duration => LogTelemetry(FhirConverterMetrics.InputDeserializationDuration, duration)))
            {
                jsonData = _dataParser.Parse(data);
            }

            rootTemplate = templateProvider.IsDefaultTemplateProvider ? string.Format("{0}/{1}", DefaultRootTemplateParentPath, rootTemplate) : rootTemplate;

            // Step: Retrieve Template
            Template template;
            using (ITimed templateRetrievalTime =
                Performance.TrackDuration(duration => LogTelemetry(FhirConverterMetrics.TemplateRetrievalDuration, duration)))
            {
                template = templateProvider.GetTemplate(rootTemplate);
            }

            if (template == null)
            {
                throw new RenderException(FhirConverterErrorCode.TemplateNotFound, string.Format(Resources.TemplateNotFound, rootTemplate));
            }

            // Build context with msg + vars as sibling keys.
            // Hash.FromDictionary requires Dictionary<string, object>; Hash is needed for Liquid member access (e.g. vars.dragonSystem).
            var dictionary = new Dictionary<string, object> { { DataKey, jsonData } };
            if (variables != null && variables.Count > 0)
            {
                dictionary["vars"] = Hash.FromDictionary(variables.ToDictionary(kv => kv.Key, kv => (object)kv.Value));
            }

            var context = CreateContext(templateProvider, dictionary, rootTemplate);

            // Step: Render Template
            string rawResult;
            using (ITimed templateRenderTime =
                Performance.TrackDuration(duration => LogTelemetry(FhirConverterMetrics.TemplateRenderDuration, duration)))
            {
                rawResult = RenderTemplates(template, context);
            }

            // Step: Post-Process
            JObject result;
            using (ITimed postProcessTime =
                Performance.TrackDuration(duration => LogTelemetry(FhirConverterMetrics.PostProcessDuration, duration)))
            {
                result = PostProcessor.Process(rawResult);
            }

            CreateTraceInfo(jsonData, context, traceInfo);

            return result.ToString(Formatting.Indented);
        }

        public override string Convert(string data, string rootTemplate, ITemplateProvider templateProvider, IList<VariableDefinition> variables, TraceInfo traceInfo = null)
        {
            string result;
            using (ITimed totalConversionTime =
                Performance.TrackDuration(duration => LogTelemetry(FhirConverterMetrics.TotalDuration, duration)))
            {
                result = InternalConvertWithTypedVariables(data, rootTemplate, templateProvider, variables, traceInfo);
            }

            return result;
        }

        public override string Convert(string data, string rootTemplate, ITemplateProvider templateProvider, IList<VariableDefinition> variables, CancellationToken cancellationToken, TraceInfo traceInfo = null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string result;
            using (ITimed totalConversionTime =
                Performance.TrackDuration(duration => LogTelemetry(FhirConverterMetrics.TotalDuration, duration)))
            {
                result = InternalConvertWithTypedVariables(data, rootTemplate, templateProvider, variables, traceInfo);
            }

            return result;
        }

        private string InternalConvertWithTypedVariables(string data, string rootTemplate, ITemplateProvider templateProvider, IList<VariableDefinition> variables, TraceInfo traceInfo = null)
        {
            if (string.IsNullOrEmpty(rootTemplate))
            {
                throw new RenderException(FhirConverterErrorCode.NullOrEmptyRootTemplate, Resources.NullOrEmptyRootTemplate);
            }

            if (templateProvider == null)
            {
                throw new RenderException(FhirConverterErrorCode.NullTemplateProvider, Resources.NullTemplateProvider);
            }

            VariableValidator.ValidateTypedVariables(variables);

            // Step: Parse input
            object jsonData;
            using (ITimed inputDeserializationTime =
                Performance.TrackDuration(duration => LogTelemetry(FhirConverterMetrics.InputDeserializationDuration, duration)))
            {
                jsonData = _dataParser.Parse(data);
            }

            rootTemplate = templateProvider.IsDefaultTemplateProvider ? string.Format("{0}/{1}", DefaultRootTemplateParentPath, rootTemplate) : rootTemplate;

            // Step: Retrieve Template
            Template template;
            using (ITimed templateRetrievalTime =
                Performance.TrackDuration(duration => LogTelemetry(FhirConverterMetrics.TemplateRetrievalDuration, duration)))
            {
                template = templateProvider.GetTemplate(rootTemplate);
            }

            if (template == null)
            {
                throw new RenderException(FhirConverterErrorCode.TemplateNotFound, string.Format(Resources.TemplateNotFound, rootTemplate));
            }

            // Build context with msg + vars as sibling keys, normalizing typed values.
            var dictionary = new Dictionary<string, object> { { DataKey, jsonData } };
            if (variables != null && variables.Count > 0)
            {
                var varsDict = NormalizeTypedVariables(variables);
                dictionary["vars"] = Hash.FromDictionary(varsDict);
            }

            var context = CreateContext(templateProvider, dictionary, rootTemplate);

            // Step: Render Template
            string rawResult;
            using (ITimed templateRenderTime =
                Performance.TrackDuration(duration => LogTelemetry(FhirConverterMetrics.TemplateRenderDuration, duration)))
            {
                rawResult = RenderTemplates(template, context);
            }

            // Step: Post-Process
            JObject result;
            using (ITimed postProcessTime =
                Performance.TrackDuration(duration => LogTelemetry(FhirConverterMetrics.PostProcessDuration, duration)))
            {
                result = PostProcessor.Process(rawResult);
            }

            CreateTraceInfo(jsonData, context, traceInfo);

            return result.ToString(Formatting.Indented);
        }

        private static Dictionary<string, object> NormalizeTypedVariables(IList<VariableDefinition> variables)
        {
            var varsDict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var variable in variables)
            {
                object normalizedValue;
                switch (variable.Type)
                {
                    case VariableType.String:
                        normalizedValue = variable.Value;
                        break;
                    case VariableType.Numeric:
                        if (long.TryParse(variable.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long longVal))
                        {
                            normalizedValue = longVal;
                        }
                        else
                        {
                            normalizedValue = double.Parse(variable.Value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture);
                        }

                        break;
                    case VariableType.Complex:
                        var jToken = JToken.Parse(variable.Value);
                        normalizedValue = jToken.ToObject();
                        break;
                    default:
                        normalizedValue = variable.Value;
                        break;
                }

                varsDict[variable.Name] = normalizedValue;
            }

            return varsDict;
        }
    }
}
