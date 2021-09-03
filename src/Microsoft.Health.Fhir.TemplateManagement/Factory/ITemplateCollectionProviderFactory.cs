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
        /// <param name="imageReference">Image reference. registry/name:tag or registry/name@digest.</param>
        /// <param name="token">Token for authentication.</param>
        /// <returns>TemplateCollectionProvider</returns>
        ITemplateCollectionProvider CreateTemplateCollectionProvider(string imageReference, string token);
    }
}
