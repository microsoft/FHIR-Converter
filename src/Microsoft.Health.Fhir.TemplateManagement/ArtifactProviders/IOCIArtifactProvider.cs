// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ContainerRegistry.Models;
using Microsoft.Health.Fhir.TemplateManagement.Models;

namespace Microsoft.Health.Fhir.TemplateManagement.ArtifactProviders
{
    public interface IOCIArtifactProvider
    {
        /// <summary>
        /// Get all layers of OCI artifact.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of artifact layers.</returns>
        Task<List<OCIArtifactLayer>> GetOCIArtifactAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get manifest of OCI image.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Manifest object</returns>
        Task<ManifestWrapper> GetManifestAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get one layer of OCI artifact.
        /// </summary>
        /// <param name="digest">Layer digest</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>One layer of artifact.</returns>
        Task<OCIArtifactLayer> GetLayerAsync(string digest, CancellationToken cancellationToken = default);
    }
}
