// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;

namespace Microsoft.Health.Fhir.Liquid.Converter
{
    /// <summary>
    /// Filters for raising errors from templates
    /// </summary>
    public partial class Filters
    {
        public static string RaiseError(string message)
        {
            throw new RenderException(FhirConverterErrorCode.TemplateRenderingError, message);
        }
    }
}
