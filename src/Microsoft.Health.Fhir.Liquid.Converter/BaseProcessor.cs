// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;

namespace Microsoft.Health.Fhir.Liquid.Converter
{
    public class BaseProcessor : IFhirConverter
    {
        private readonly ProcessorSettings _settings;

        public BaseProcessor(ProcessorSettings processorSettings = null)
        {
            _settings = processorSettings;
        }

        public string Convert(string data, string rootTemplate, ITemplateProvider templateProvider, CancellationToken cancellationToken, TraceInfo traceInfo = null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Convert(data, rootTemplate, templateProvider, traceInfo);
        }

        public virtual string Convert(string data, string rootTemplate, ITemplateProvider templateProvider, TraceInfo traceInfo = null)
        {
            throw new NotImplementedException();
        }

        protected virtual Context CreateContext(ITemplateProvider templateProvider, object data)
        {
            // Load data and templates
            var timeout = _settings?.TimeOut ?? 0;
            var dataDictionary = data is IDictionary<string, object> dictionary
                ? dictionary
                : new Dictionary<string, object>() { { "hl7v2Data", data } };
            var context = new Context(
                environments: new List<Hash>() { Hash.FromDictionary(dataDictionary) },
                outerScope: new Hash(),
                registers: Hash.FromDictionary(new Dictionary<string, object>() { { "file_system", templateProvider.GetTemplateFileSystem() } }),
                errorsOutputMode: ErrorsOutputMode.Rethrow,
                maxIterations: 0,
                timeout: timeout,
                formatProvider: CultureInfo.InvariantCulture);

            // Load filters
            context.AddFilters(typeof(Filters));

            return context;
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
