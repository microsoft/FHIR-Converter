// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Azure.ContainerRegistry.Models;
using Microsoft.Health.Fhir.TemplateManagement.Client;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Microsoft.Health.Fhir.TemplateManagement.Utilities;

namespace Microsoft.Health.Fhir.TemplateManagement.ArtifactProviders
{
    public class OCIArtifactProvider : IOCIArtifactProvider
    {
        private readonly IOCIArtifactClient _client;

        public OCIArtifactProvider(ImageInfo imageInfo, IOCIArtifactClient client)
        {
            EnsureArg.IsNotNull(imageInfo, nameof(imageInfo));
            EnsureArg.IsNotNull(client, nameof(client));

            _client = client;
            ImageInfo = imageInfo;
        }

        protected ImageInfo ImageInfo { get; }

        public virtual async Task<List<ArtifactLayer>> GetOCIArtifactAsync(CancellationToken cancellationToken = default)
        {
            var artifactsResult = new List<ArtifactLayer>();
            cancellationToken.ThrowIfCancellationRequested();
            var manifest = await GetManifestAsync(cancellationToken);
            var layersInfo = manifest.Layers;
            foreach (var layer in layersInfo)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var artifactLayer = await GetLayerAsync(layer.Digest, cancellationToken);
                artifactsResult.Add(artifactLayer);
            }

            return artifactsResult;
        }

        public virtual async Task<ManifestWrapper> GetManifestAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ManifestWrapper manifestInfo = await _client.PullManifestAcync(ImageInfo.ImageName, ImageInfo.Label, cancellationToken);
            ValidationUtility.ValidateManifest(manifestInfo);
            return manifestInfo;
        }

        public virtual async Task<ArtifactLayer> GetLayerAsync(string layerDigest, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNull(layerDigest, nameof(layerDigest));

            cancellationToken.ThrowIfCancellationRequested();
            var rawBytes = await _client.PullBlobAsBytesAcync(ImageInfo.ImageName, layerDigest, cancellationToken);
            ArtifactLayer artifactsLayer = new ArtifactLayer()
            {
                Content = rawBytes,
                Digest = layerDigest,
                Size = rawBytes.Length,
            };
            return artifactsLayer;
        }
    }
}
