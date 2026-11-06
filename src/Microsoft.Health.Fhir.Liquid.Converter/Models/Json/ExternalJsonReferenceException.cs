// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Health.Fhir.Liquid.Converter.Models.Json
{
    /// <summary>
    /// Thrown when a JSON Schema template attempts to reference an external (file or URL) document via
    /// <c>$ref</c>. Resolving such references is disabled to prevent server-side request forgery (SSRF).
    /// </summary>
    public class ExternalJsonReferenceException : Exception
    {
        public ExternalJsonReferenceException(string reference)
            : base(string.Format(Resources.ExternalJsonSchemaReferenceNotAllowed, reference))
        {
            Reference = reference;
        }

        public string Reference { get; }
    }
}
