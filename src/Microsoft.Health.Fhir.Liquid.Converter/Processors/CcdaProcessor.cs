// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using DotLiquid;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Parsers;
using Microsoft.Health.Fhir.Liquid.Converter.Utilities;

namespace Microsoft.Health.Fhir.Liquid.Converter.Processors
{
    public class CcdaProcessor : BaseProcessor
    {
        private readonly IDataParser _parser = new CcdaDataParser();

        public CcdaProcessor(ProcessorSettings processorSettings, ILogger<CcdaProcessor> logger)
            : base(processorSettings, logger)
        {
        }

        protected override DefaultRootTemplateParentPath DefaultRootTemplateParentPath { get; set; } = DefaultRootTemplateParentPath.Ccda;

        protected override string InternalConvert(string data, string rootTemplate, ITemplateProvider templateProvider, TraceInfo traceInfo = null)
        {
            object ccdaData = _parser.Parse(data);
            return InternalConvertFromObject(ccdaData, rootTemplate, templateProvider, traceInfo);
        }

        protected override Context CreateContext(ITemplateProvider templateProvider, IDictionary<string, object> data, string rootTemplate)
        {
            // Load value set mapping
            var context = base.CreateContext(templateProvider, data, rootTemplate);
            var codeMapping = templateProvider.GetTemplate(GetCodeMappingTemplatePath(context));
            if (codeMapping?.Root?.NodeList?.First() != null)
            {
                context["CodeMapping"] = codeMapping.Root.NodeList.First();
            }

            return context;
        }

        private string GetCodeMappingTemplatePath(Context context)
        {
            var rootTemplateParentPath = context[TemplateUtility.RootTemplateParentPathScope]?.ToString();
            var codeSystemTemplateName = "ValueSet/ValueSet";
            return TemplateUtility.GetFormattedTemplatePath(codeSystemTemplateName, rootTemplateParentPath);
        }
    }
}
