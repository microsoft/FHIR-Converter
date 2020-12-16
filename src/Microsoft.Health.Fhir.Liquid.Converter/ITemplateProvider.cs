// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using DotLiquid;

namespace Microsoft.Health.Fhir.Liquid.Converter
{
    public interface ITemplateProvider
    {
        public Template GetTemplate(Context context, string templateName);

        public Template GetTemplate(string templateName);
    }
}
