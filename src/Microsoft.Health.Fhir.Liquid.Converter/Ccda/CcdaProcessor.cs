﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Linq;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.OutputProcessor;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Liquid.Converter.Ccda
{
    public class CcdaProcessor : BaseProcessor
    {
        private readonly CcdaDataParser _dataParser = new CcdaDataParser();

        public CcdaProcessor(ProcessorSettings processorSettings = null)
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

            var ccdaData = _dataParser.Parse(data);
            var context = CreateContext(templateProvider, ccdaData);
            var rawResult = RenderTemplates(template, context);
            var result = PostProcessor.Process(rawResult);

            return result.ToString(Formatting.Indented);
        }

        protected override Context CreateContext(ITemplateProvider templateProvider, object ccdaData)
        {
            // Load value set mapping
            var context = base.CreateContext(templateProvider, ccdaData);
            var codeMapping = templateProvider.GetTemplate("ValueSet/ValueSet");
            if (codeMapping?.Root?.NodeList?.First() != null)
            {
                context["CodeMapping"] = codeMapping.Root.NodeList.First();
            }

            return context;
        }
    }
}
