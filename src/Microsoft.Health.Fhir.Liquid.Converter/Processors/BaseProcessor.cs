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
using Microsoft.Extensions.Logging;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.OutputProcessors;
using Microsoft.Health.Fhir.Liquid.Converter.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Liquid.Converter.Processors
{
    public abstract class BaseProcessor : IFhirConverter
    {
        protected BaseProcessor(ProcessorSettings processorSettings, ILogger<BaseProcessor> logger)
        {
            Settings = EnsureArg.IsNotNull(processorSettings, nameof(processorSettings));
            Logger = EnsureArg.IsNotNull(logger, nameof(logger));
        }

        public ProcessorSettings Settings { get; }

        protected ILogger<BaseProcessor> Logger { get; }

        protected virtual string DataKey { get; set; } = "msg";

        protected virtual DefaultRootTemplateParentPath DefaultRootTemplateParentPath { get; set; }

        public string Convert(string data, string rootTemplate, ITemplateProvider templateProvider, CancellationToken cancellationToken, TraceInfo traceInfo = null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string result = InternalConvert(data, rootTemplate, templateProvider, traceInfo);
            return result;
        }

        public string Convert(string data, string rootTemplate, ITemplateProvider templateProvider, TraceInfo traceInfo = null)
        {
            string result = InternalConvert(data, rootTemplate, templateProvider, traceInfo);
            return result;
        }

        protected abstract string InternalConvert(string data, string rootTemplate, ITemplateProvider templateProvider, TraceInfo traceInfo = null);

        protected virtual Context CreateContext(ITemplateProvider templateProvider, IDictionary<string, object> data, string rootTemplate)
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

            // Add root template's parent path to context.
            AddRootTemplatePathScope(context, templateProvider, rootTemplate);

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

            rootTemplate = templateProvider.IsDefaultTemplateProvider ? string.Format("{0}/{1}", DefaultRootTemplateParentPath, rootTemplate) : rootTemplate;

            // Step: Retrieve Template
            Template template = templateProvider.GetTemplate(rootTemplate);
            if (template == null)
            {
                throw new RenderException(FhirConverterErrorCode.TemplateNotFound, string.Format(Resources.TemplateNotFound, rootTemplate));
            }

            var dictionary = new Dictionary<string, object> { { DataKey, data } };
            var context = CreateContext(templateProvider, dictionary, rootTemplate);

            // Step: Render Template
            string rawResult = RenderTemplates(template, context);

            // Step: Post-Process
            JObject result = PostProcessor.Process(rawResult);
            CreateTraceInfo(data, context, traceInfo);

            return result.ToString(Formatting.Indented);
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
            catch (TemplateLoadException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ex: {1} StackTrace: '{0}'", Environment.StackTrace, ex);
                throw new RenderException(FhirConverterErrorCode.TemplateRenderingError, string.Format(Resources.TemplateRenderingError, ex.Message), ex);
            }
        }

        protected void AddRootTemplatePathScope(Context context, ITemplateProvider templateProvider, string rootTemplate)
        {
            // Add path to root template's parent. In case of default template provider, use the data type as the parent path, else use the parent path set in the context.
            context[TemplateUtility.RootTemplateParentPathScope] = templateProvider.IsDefaultTemplateProvider ? DefaultRootTemplateParentPath : TemplateUtility.GetRootTemplateParentPath(rootTemplate);
        }

        protected void LogTelemetry(string telemetryName, double duration)
        {
            if (Settings.EnableTelemetryLogger)
            {
                Logger.LogInformation("{Metric}: {Duration} milliseconds.", telemetryName, duration);
            }
        }
    }
}
