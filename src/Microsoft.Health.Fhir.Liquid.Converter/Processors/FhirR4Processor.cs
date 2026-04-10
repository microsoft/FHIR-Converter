// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using DotLiquid;
using EnsureThat;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.OutputProcessors;
using Microsoft.Health.Fhir.Liquid.Converter.Parsers;
using Microsoft.Health.MeasurementUtility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Liquid.Converter.Processors
{
    public class FhirR4Processor : JsonProcessor, IFhirConverterWithVariables
    {
        private static readonly Regex ValidVariableNameRegex = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]*$", RegexOptions.Compiled);
        private static readonly HashSet<string> ReservedVariableNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "msg", "vars" };

        private const int MaxVariableNameLength = 128;
        private const int MaxVariableValueLength = 1048576;
        private const int MaxVariableCount = 100;

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

        public string Convert(string data, string rootTemplate, ITemplateProvider templateProvider, IDictionary<string, string> variables, TraceInfo traceInfo = null)
        {
            string result;
            using (ITimed totalConversionTime =
                Performance.TrackDuration(duration => LogTelemetry(FhirConverterMetrics.TotalDuration, duration)))
            {
                result = InternalConvertWithVariables(data, rootTemplate, templateProvider, variables, traceInfo);
            }

            return result;
        }

        public string Convert(string data, string rootTemplate, ITemplateProvider templateProvider, IDictionary<string, string> variables, CancellationToken cancellationToken, TraceInfo traceInfo = null)
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

            // Validate variables
            if (variables != null)
            {
                if (variables.Count > MaxVariableCount)
                {
                    throw new RenderException(FhirConverterErrorCode.InvalidVariableName, string.Format(Resources.TooManyVariables, MaxVariableCount, variables.Count));
                }

                foreach (var kvp in variables)
                {
                    ValidateVariableName(kvp.Key);

                    if (kvp.Value != null && kvp.Value.Length > MaxVariableValueLength)
                    {
                        throw new RenderException(FhirConverterErrorCode.InvalidVariableName, string.Format(Resources.VariableValueTooLong, kvp.Key, MaxVariableValueLength));
                    }
                }
            }

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

            // Build context with msg + vars as sibling keys
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

        public static void ValidateVariableName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new RenderException(FhirConverterErrorCode.InvalidVariableName, Resources.InvalidVariableName);
            }

            if (name.Length > MaxVariableNameLength)
            {
                throw new RenderException(FhirConverterErrorCode.InvalidVariableName, string.Format(Resources.VariableNameTooLong, MaxVariableNameLength));
            }

            if (!ValidVariableNameRegex.IsMatch(name))
            {
                throw new RenderException(FhirConverterErrorCode.InvalidVariableName, string.Format(Resources.InvalidVariableNameFormat, name));
            }

            if (ReservedVariableNames.Contains(name))
            {
                throw new RenderException(FhirConverterErrorCode.InvalidVariableName, string.Format(Resources.ReservedVariableName, name));
            }
        }
    }
}
