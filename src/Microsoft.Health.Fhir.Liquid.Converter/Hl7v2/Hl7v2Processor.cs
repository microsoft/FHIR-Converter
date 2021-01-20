// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

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
            : base(processorSettings)
        {
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

        protected override Context CreateContext(ITemplateProvider templateProvider, object hl7v2Data)
        {
            // Load code system mapping
            var context = base.CreateContext(templateProvider, hl7v2Data);
            var codeSystemMapping = templateProvider.GetTemplate("CodeSystem/CodeSystem");
            if (codeSystemMapping?.Root?.NodeList?.First() != null)
            {
                context["CodeSystemMapping"] = codeSystemMapping.Root.NodeList.First();
            }

            return context;
        }
    }
}
