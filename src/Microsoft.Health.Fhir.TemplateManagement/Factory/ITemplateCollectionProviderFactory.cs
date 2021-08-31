// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Fhir.TemplateManagement.ArtifactProviders;

namespace Microsoft.Health.Fhir.TemplateManagement
{
    public interface ITemplateCollectionProviderFactory : IOciArtifactProviderFactory
    {
        /// <summary>
        /// Get template container from a image ID
        /// </summary>
        /// <param name="token"></param>
        /// <param name="imageReference"></param>
        /// <returns>TemplateCollectionProvider</returns>
        ITemplateCollectionProvider CreateTemplateCollectionProvider(string imageReference, string token);
    }
}
