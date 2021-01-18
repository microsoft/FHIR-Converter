// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Hl7v2.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.OutputProcessor;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Liquid.Converter.Hl7v2
{
    public class Hl7v2Processor : BaseProcessor
    {
        private readonly Hl7v2DataParser _dataParser = new Hl7v2DataParser();
        private readonly ProcessorSettings _settings;

        public Hl7v2Processor(ProcessorSettings processorSettings = null)
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

            var hl7v2Data = _dataParser.Parse(data);
            var context = CreateContext(templateProvider, hl7v2Data);
            var rawResult = RenderTemplates(template, context);
            var result = PostProcessor.Process(rawResult);
            if (traceInfo is Hl7v2TraceInfo hl7V2TraceInfo)
            {
                hl7V2TraceInfo.UnusedSegments = Hl7v2TraceInfo.CreateTraceInfo(hl7v2Data).UnusedSegments;
            }

            return result.ToString(Formatting.Indented);
        }

        private Context CreateContext(ITemplateProvider templateProvider, Hl7v2Data hl7v2Data)
        {
            // Load data and templates
            var timeout = _settings?.TimeOut ?? 0;
            var context = new Context(
                environments: new List<Hash>() { Hash.FromDictionary(new Dictionary<string, object>() { { "hl7v2Data", hl7v2Data } }) },
                outerScope: new Hash(),
                registers: Hash.FromDictionary(new Dictionary<string, object>() { { "file_system", templateProvider.GetTemplateFileSystem() } }),
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
    }
}
