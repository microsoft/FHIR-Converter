// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Azure.ContainerRegistry.Models;
using Microsoft.Health.Fhir.TemplateManagement.Client;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Microsoft.Health.Fhir.TemplateManagement.Utilities;

namespace Microsoft.Health.Fhir.TemplateManagement.ArtifactProviders
{
    public class OciArtifactProvider : IOciArtifactProvider
    {
        private readonly IOciClient _client;

        public OciArtifactProvider(ImageInfo imageInfo, IOciClient client)
        {
            EnsureArg.IsNotNull(imageInfo, nameof(imageInfo));
            EnsureArg.IsNotNull(client, nameof(client));

            _client = client;
            ImageInfo = imageInfo;
        }

        protected ImageInfo ImageInfo { get; }

        public virtual async Task<ArtifactImage> GetOciArtifactAsync(CancellationToken cancellationToken = default)
        {
            var artifactsResult = new ArtifactImage();
            cancellationToken.ThrowIfCancellationRequested();
            var manifest = await GetManifestAsync(cancellationToken);
            var layersInfo = manifest.Layers;
            artifactsResult.Manifest = manifest;

            foreach (var layer in layersInfo)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var artifactLayer = await GetLayerAsync(layer.Digest, cancellationToken);
                artifactsResult.Blobs.Add(artifactLayer);
            }

            return artifactsResult;
        }

        public virtual async Task<ManifestWrapper> GetManifestAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ManifestWrapper manifestInfo = await _client.GetManifestAsync(ImageInfo.ImageName, ImageInfo.Label, cancellationToken);
            ValidationUtility.ValidateManifest(manifestInfo);
            return manifestInfo;
        }

        public virtual async Task<ArtifactBlob> GetLayerAsync(string layerDigest, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNull(layerDigest, nameof(layerDigest));

            cancellationToken.ThrowIfCancellationRequested();
            var rawBytes = await _client.GetBlobAsync(ImageInfo.ImageName, layerDigest, cancellationToken);
            ArtifactBlob artifactsLayer = new ArtifactBlob()
            {
                Content = rawBytes.Content,
                Digest = layerDigest,
                Size = rawBytes.Content.Length,
            };
            return artifactsLayer;
        }
    }
}
