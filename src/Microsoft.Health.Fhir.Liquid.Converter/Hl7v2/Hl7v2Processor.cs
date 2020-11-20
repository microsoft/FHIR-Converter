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
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Hl7v2.OutputProcessor;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Liquid.Converter.Hl7v2
{
    public class Hl7v2Processor : IFhirConverter
    {
        private readonly Hl7v2DataParser _dataParser = new Hl7v2DataParser();
        private readonly ProcessorSettings _settings;

        public Hl7v2Processor(ProcessorSettings processorSettings = null)
        {
            _settings = processorSettings;
        }

        public string Convert(string data, string rootTemplate, ITemplateProvider templateProvider, TraceInfo traceInfo = null)
        {
            if (string.IsNullOrEmpty(rootTemplate))
            {
                throw new RenderException(FhirConverterErrorCode.NullOrEmptyRootTemplate, Resources.NullOrEmptyRootTemplate);
            }

            if (templateProvider == null)
            {
                throw new RenderException(FhirConverterErrorCode.NullTemplateProvider, Resources.NullTemplateProvider);
            }

            var template = templateProvider.GetTemplate(rootTemplate);
            if (template == null)
            {
                throw new RenderException(FhirConverterErrorCode.TemplateNotFound, string.Format(Resources.TemplateNotFound, rootTemplate));
            }

            var context = CreateContext(templateProvider, data);
            var rawResult = RenderTemplates(template, context);
            var result = PostProcessor.Process(rawResult);
            return result.ToString(Formatting.Indented);
        }

        public string Convert(string data, string rootTemplate, ITemplateProvider templateProvider, CancellationToken cancellationToken, TraceInfo traceInfo = null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Convert(data, rootTemplate, templateProvider, traceInfo);
        }

        private Context CreateContext(ITemplateProvider templateProvider, string data)
        {
            // Load data and templates
            var hl7v2Data = _dataParser.Parse(data);
            var timeout = _settings != null ? _settings.TimeOut : 0;
            var context = new Context(
                environments: new List<Hash>() { Hash.FromAnonymousObject(new { hl7v2Data }) },
                outerScope: new Hash(),
                registers: Hash.FromAnonymousObject(new { file_system = templateProvider }),
                errorsOutputMode: ErrorsOutputMode.Rethrow,
                maxIterations: 0,
                timeout: timeout,
                formatProvider: CultureInfo.InvariantCulture);

            // Load code system mapping
            var codeSystemMapping = templateProvider.GetTemplate("CodeSystem/CodeSystem");
            if (codeSystemMapping?.Root?.NodeList?.First() != null)
            {
                context["CodeSystemMapping"] = codeSystemMapping.Root.NodeList.First();
            }

            // Load filters
            context.AddFilters(typeof(Filters));

            return context;
        }

        private string RenderTemplates(Template template, Context context)
        {
            try
            {
                template.MakeThreadSafe();
                return template.Render(new RenderParameters(CultureInfo.InvariantCulture) { Context = context });
            }
            catch (Exception ex)
            {
                throw new RenderException(FhirConverterErrorCode.TemplateRenderingError, string.Format(Resources.TemplateRenderingError, ex.Message), ex);
            }
        }
    }
}
