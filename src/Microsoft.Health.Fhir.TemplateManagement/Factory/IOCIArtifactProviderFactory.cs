// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Fhir.TemplateManagement.ArtifactProviders;

namespace Microsoft.Health.Fhir.TemplateManagement
{
    public interface IOCIArtifactProviderFactory
    {
        /// <summary>
        /// Get template container from a image ID
        /// </summary>
        /// <param name="token"></param>
        /// <param name="imageReference"></param>
        /// <returns>Interface of OCIArtifactProvider</returns>
        IOCIArtifactProvider CreateProvider(string imageReference, string token);
    }
}
