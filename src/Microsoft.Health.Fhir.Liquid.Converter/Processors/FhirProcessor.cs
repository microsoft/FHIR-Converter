// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.Extensions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Models.Hl7v2;
using Microsoft.Health.Fhir.Liquid.Converter.Parsers;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Liquid.Converter.Processors
{
    public class FhirProcessor : BaseProcessor
    {
        private readonly IDataParser _parser = new FhirDataParser();

        public FhirProcessor(ProcessorSettings processorSettings = null)
            : base(DisablePostProcessor(processorSettings))
        {
            if (processorSettings == null)
            {
                processorSettings = new ProcessorSettings();
            }

            processorSettings.PostProcess = false;
        }

        protected override string DataKey { get; set; } = "fhirData";

        protected override Context CreateContext(ITemplateProvider templateProvider, IDictionary<string, object> data)
        {
            // Load code system mapping
            /*var context = base.CreateContext(templateProvider, data);
            var codeMapping = templateProvider.GetTemplate("CodeSystem/CodeSystem");
            if (codeMapping?.Root?.NodeList?.First() != null)
            {
                context["CodeMapping"] = codeMapping.Root.NodeList.First();
            }*/

            var context = base.CreateContext(templateProvider, data);
            var codeMapping = JToken.Parse(templateProvider.GetTemplateFileSystem().ReadTemplateFile(context, "CodeSystem/CodeSystem"));
            context["CodeMapping"] = codeMapping.ToObject();

            return context;
        }

        public static ProcessorSettings DisablePostProcessor(ProcessorSettings processorSettings)
        {
            if (processorSettings == null)
            {
                processorSettings = new ProcessorSettings();
            }

            processorSettings.PostProcess = false;
            return processorSettings;
        }

        public override string Convert(string data, string rootTemplate, ITemplateProvider templateProvider, TraceInfo traceInfo = null)
        {
            var fhirData = _parser.Parse(data);
            return Convert(fhirData, rootTemplate, templateProvider, traceInfo);
        }
    }
}