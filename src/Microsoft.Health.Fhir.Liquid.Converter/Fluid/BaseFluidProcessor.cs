// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using EnsureThat;
using Fluid;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.MeasurementUtility;

namespace Microsoft.Health.Fhir.Liquid.Converter.Fluid
{
    public abstract class BaseFluidProcessor : IFhirConverter<IFluidTemplate>
    {
        protected BaseFluidProcessor(ProcessorSettings processorSettings, ILogger<BaseFluidProcessor> logger)
        {
            Settings = EnsureArg.IsNotNull(processorSettings, nameof(processorSettings));
            Logger = EnsureArg.IsNotNull(logger, nameof(logger));
            TemplateOptions = new TemplateOptions();
        }

        public ProcessorSettings Settings { get; }

        protected ILogger<BaseFluidProcessor> Logger { get; }

        protected TemplateOptions TemplateOptions { get; }

        protected virtual DefaultRootTemplateParentPath DefaultRootTemplateParentPath { get; set; }

        public string Convert(string data, string templateName, ITemplateProvider<IFluidTemplate> templateProvider, TraceInfo traceInfo = null)
        {
            using ITimed totalConversionTime = Performance.TrackDuration(duration => LogTelemetry(FhirConverterMetrics.TotalDuration, duration));
            var template = templateProvider.GetTemplate(templateName);
            var context = CreateContext(templateProvider);
            PopulateContext(data, context, traceInfo);
            return template.Render(context);
        }

        public string Convert(string data, string templateName, ITemplateProvider<IFluidTemplate> templateProvider, CancellationToken cancellationToken, TraceInfo traceInfo = null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Convert(data, templateName, templateProvider, traceInfo);
        }

        protected abstract void PopulateContext(string data, TemplateContext context, TraceInfo traceInfo = null);

        protected virtual TemplateContext CreateContext(ITemplateProvider<IFluidTemplate> templateProvider)
        {
            return new TemplateContext()
                .SetTemplateProvider(templateProvider);
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
