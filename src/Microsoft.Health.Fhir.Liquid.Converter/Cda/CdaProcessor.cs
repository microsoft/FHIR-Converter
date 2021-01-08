// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Globalization;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.OutputProcessor;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Liquid.Converter.Cda
{
    public class CdaProcessor : BaseProcessor
    {
        private readonly CdaDataParser _dataParser = new CdaDataParser();
        private readonly ProcessorSettings _settings;

        public CdaProcessor(ProcessorSettings processorSettings = null)
        {
            _settings = processorSettings;
        }

        public override string Convert(string data, string rootTemplate, ITemplateProvider templateProvider, TraceInfo traceInfo = null)
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

            var cdaData = _dataParser.Parse(data);
            var context = CreateContext(templateProvider, cdaData);
            var rawResult = RenderTemplates(template, context);
            var result = PostProcessor.Process(rawResult);

            return result.ToString(Formatting.Indented);
        }

        private Context CreateContext(ITemplateProvider templateProvider, IDictionary<string, object> cdaData)
        {
            // Load data and templates
            var timeout = _settings?.TimeOut ?? 0;
            var context = new Context(
                environments: new List<Hash>() { Hash.FromDictionary(cdaData) },
                outerScope: new Hash(),
                registers: Hash.FromAnonymousObject(new { file_system = templateProvider.GetTemplateFileSystem() }),
                errorsOutputMode: ErrorsOutputMode.Rethrow,
                maxIterations: 0,
                timeout: timeout,
                formatProvider: CultureInfo.InvariantCulture);

            // Load filters
            context.AddFilters(typeof(Filters));

            return context;
        }
    }
}
