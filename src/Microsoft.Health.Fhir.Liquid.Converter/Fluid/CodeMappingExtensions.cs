// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using Fluid;
using Microsoft.Health.Fhir.Liquid.Converter.Models;

namespace Microsoft.Health.Fhir.Liquid.Converter.Fluid
{
    internal static class CodeMappingExtensions
    {
        private static readonly string CodeMappingKey = "CodeMapping";

        public static CodeMapping GetCodeMapping(this TemplateContext context)
        {
            EnsureArg.IsNotNull(context, nameof(context));

            return context.GetValue(CodeMappingKey).ToObjectValue() as CodeMapping;
        }

        public static TemplateContext RegisterCodeMapping(this TemplateContext context, ITemplateProvider<IFluidTemplate> templateProvider)
        {
            EnsureArg.IsNotNull(context, nameof(context));
            EnsureArg.IsNotNull(templateProvider, nameof(templateProvider));

            // Old Code
            // var codeMapping = templateProvider.GetTemplate(GetCodeMappingTemplatePath(context));
            // if (codeMapping?.Root?.NodeList?.First() != null)
            // {
            //    context["CodeMapping"] = codeMapping.Root.NodeList.First();
            // }
            // return context;

            // TODO: Implement the following logic

            context.SetValue(CodeMappingKey, null);

            return context;
        }
    }
}
