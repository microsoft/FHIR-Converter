// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using DotLiquid;
using EnsureThat;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.OutputProcessors;
using Microsoft.Health.Fhir.Liquid.Converter.Telemetry;
using Microsoft.Health.Logging.Telemetry;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Liquid.Converter.Processors
{
    public abstract class BaseProcessor : IFhirConverter
    {
        private readonly ITelemetryLogger _telemetryLogger;

        protected BaseProcessor(ProcessorSettings processorSettings, ITelemetryLogger telemetryLogger)
        {
            Settings = EnsureArg.IsNotNull(processorSettings, nameof(processorSettings));
            _telemetryLogger = EnsureArg.IsNotNull(telemetryLogger, nameof(telemetryLogger));
        }

        public ProcessorSettings Settings { get; }

        protected ITelemetryLogger TelemetryLogger { get => _telemetryLogger; }

        protected virtual string DataKey { get; set; } = "msg";

        public string Convert(string data, string rootTemplate, ITemplateProvider templateProvider, CancellationToken cancellationToken, TraceInfo traceInfo = null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string result;
            using (ITimed totalConversionTime = TelemetryLogger.TrackDuration(ConverterMetrics.TotalDuration))
            {
                result = InternalConvert(data, rootTemplate, templateProvider, traceInfo);
            }

            return result;
        }

        public string Convert(string data, string rootTemplate, ITemplateProvider templateProvider, TraceInfo traceInfo = null)
        {
            string result;
            using (ITimed totalConversionTime = TelemetryLogger.TrackDuration(ConverterMetrics.TotalDuration))
            {
                result = InternalConvert(data, rootTemplate, templateProvider, traceInfo);
            }

            return result;
        }

        protected abstract string InternalConvert(string data, string rootTemplate, ITemplateProvider templateProvider, TraceInfo traceInfo = null);

        protected virtual Context CreateContext(ITemplateProvider templateProvider, IDictionary<string, object> data)
        {
            // Load data and templates
            var cancellationToken = Settings.TimeOut > 0 ? new CancellationTokenSource(Settings.TimeOut).Token : CancellationToken.None;
            var context = new Context(
                environments: new List<Hash> { Hash.FromDictionary(data) },
                outerScope: new Hash(),
                registers: Hash.FromDictionary(new Dictionary<string, object> { { "file_system", templateProvider.GetTemplateFileSystem() } }),
                errorsOutputMode: ErrorsOutputMode.Rethrow,
                maxIterations: Settings.MaxIterations,
                formatProvider: CultureInfo.InvariantCulture,
                cancellationToken: cancellationToken);

            // Load filters
            context.AddFilters(typeof(Filters));

            return context;
        }

        protected virtual void CreateTraceInfo(object data, Context context, TraceInfo traceInfo)
        {
        }

        protected string InternalConvertFromObject(object data, string rootTemplate, ITemplateProvider templateProvider, TraceInfo traceInfo = null)
        {
            if (string.IsNullOrEmpty(rootTemplate))
            {
                throw new RenderException(FhirConverterErrorCode.NullOrEmptyRootTemplate, Resources.NullOrEmptyRootTemplate);
            }

            if (templateProvider == null)
            {
                throw new RenderException(FhirConverterErrorCode.NullTemplateProvider, Resources.NullTemplateProvider);
            }

            // Step: Retrieve Template
            Template template;
            using (ITimed templateRetrievalTime = TelemetryLogger.TrackDuration(ConverterMetrics.TemplateRetrievalDuration))
            {
               template = templateProvider.GetTemplate(rootTemplate);
            }

            if (template == null)
            {
                throw new RenderException(FhirConverterErrorCode.TemplateNotFound, string.Format(Resources.TemplateNotFound, rootTemplate));
            }

            var dictionary = new Dictionary<string, object> { { DataKey, data } };
            var context = CreateContext(templateProvider, dictionary);

            // Step: Render Template
            string rawResult;
            using (ITimed templateRenderTime = TelemetryLogger.TrackDuration(ConverterMetrics.TemplateRenderDuration))
            {
                rawResult = RenderTemplates(template, context);
            }

            // Step: Post-Process
            JObject result;
            using (ITimed postProcessTime = TelemetryLogger.TrackDuration(ConverterMetrics.PostProcessDuration))
            {
                result = PostProcessor.Process(rawResult);
            }

            CreateTraceInfo(data, context, traceInfo);

            return result.ToString();
        }

        protected string RenderTemplates(Template template, Context context)
        {
            try
            {
                template.MakeThreadSafe();
                return template.Render(RenderParameters.FromContext(context, CultureInfo.InvariantCulture));
            }
            catch (TimeoutException ex)
            {
                throw new RenderException(FhirConverterErrorCode.TimeoutError, Resources.TimeoutError, ex);
            }
            catch (OperationCanceledException ex)
            {
                throw new RenderException(FhirConverterErrorCode.TimeoutError, Resources.TimeoutError, ex);
            }
            catch (RenderException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new RenderException(FhirConverterErrorCode.TemplateRenderingError, string.Format(Resources.TemplateRenderingError, ex.Message), ex);
            }
        }
    }
}
