// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Fluid;

namespace Microsoft.Health.Fhir.Liquid.Converter.Fluid
{
    internal static class TemplateProviderExtensions
    {
        private static readonly string TemplateProviderKey = "file_system";

        public static TemplateContext SetTemplateProvider(this TemplateContext context, ITemplateProvider<IFluidTemplate> templateProvider)
        {
            context.SetValue(TemplateProviderKey, templateProvider);
            return context;
        }

        public static ITemplateProvider<IFluidTemplate> GetTemplateProvider(this TemplateContext context)
        {
            return context.GetValue(TemplateProviderKey).ToObjectValue() as ITemplateProvider<IFluidTemplate>;
        }
    }
}
