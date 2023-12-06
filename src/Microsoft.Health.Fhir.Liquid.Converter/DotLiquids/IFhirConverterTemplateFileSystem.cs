// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using DotLiquid;
using DotLiquid.FileSystems;

namespace Microsoft.Health.Fhir.Liquid.Converter.DotLiquids
{
    public interface IFhirConverterTemplateFileSystem : ITemplateFileSystem
    {
        public Template GetTemplate(string templateName, string rootTemplatePath = "");
    }
}
