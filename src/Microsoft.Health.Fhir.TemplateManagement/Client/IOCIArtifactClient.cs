// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ContainerRegistry.Models;

namespace Microsoft.Health.Fhir.TemplateManagement.Client
{
    public interface IOCIArtifactClient
    {
        /// <summary>
        /// Pull one blob as stream from acr.
        /// </summary>
        /// <param name="imageName">Image name</param>
        /// <param name="digest">Blob digest</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Stream of one blob.</returns>
        Task<Stream> PullBlobAsStreamAcync(string imageName, string digest, CancellationToken cancellationToken = default);

        /// <summary>
        /// Pull one blob as bytes from acr.
        /// </summary>
        /// <param name="imageName">Image name</param>
        /// <param name="digest">Blob digest</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Raw bytes of blob.</returns>
        Task<byte[]> PullBlobAsBytesAcync(string imageName, string digest, CancellationToken cancellationToken = default);

        /// <summary>
        /// Pull manifest from acr.
        /// </summary>
        /// <param name="imageName">Image name</param>
        /// <param name="label">Image tag or digest</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Manifest object</returns>
        Task<ManifestWrapper> PullManifestAcync(string imageName, string label, CancellationToken cancellationToken = default);
    }
}
