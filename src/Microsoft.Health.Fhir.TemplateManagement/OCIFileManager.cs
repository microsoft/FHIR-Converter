// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Health.Fhir.TemplateManagement.Client;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Microsoft.Health.Fhir.TemplateManagement.Overlay;

namespace Microsoft.Health.Fhir.TemplateManagement
{
    public class OciFileManager
    {
        private readonly IOciClient _client;
        private readonly IOverlayFileSystem _overlayFS;
        private readonly IOverlayOperator _overlayOperator;

        public OciFileManager(string registry, string workingFolder)
        {
            _overlayFS = new OverlayFileSystem(workingFolder);
            _client = new OrasClient(registry, Path.Combine(workingFolder, Constants.HiddenLayersFolder));
            _overlayOperator = new OverlayOperator();
        }

        /// <summary>
        /// Pull image layers into working folder.
        /// Layers will be written into hidden image folder.
        /// </summary>
        /// <param name="name">Image name.</param>
        /// <param name="reference">Image reference (digest or tag).</param>
        /// <param name="forceOverride">Whether to override the existed files.</param>
        /// <returns>OCI artifact image.</returns>
        public async Task<ArtifactImage> PullOciImageAsync(string name, string reference, bool forceOverride = false)
        {
            if (!_overlayFS.IsCleanWorkingFolder() && !forceOverride)
            {
                throw new TemplateManagementException($"The folder is not empty. If force to override existed files, please add -f in parameters");
            }

            var artifactImage = await _client.PullImageAsync(name, reference);

            // Optional to write in user's folder.
            await _overlayFS.WriteManifestAsync(artifactImage.Manifest);

            var fileLayer = await UnpackOciImageAsync(artifactImage);
            await _overlayFS.WriteOciFileLayerAsync(fileLayer);
            return artifactImage;
        }

        /// <summary>
        /// Push image layers from working folder in order.
        /// </summary>
        /// <param name="name">Image name.</param>
        /// <param name="tag">Image tag. </param>
        /// <param name="ignoreBaseLayers">Whether to ignore base layer when packing.</param>
        /// <returns>Image digest.</returns>
        public async Task<string> PushOciImageAsync(string name, string tag, bool ignoreBaseLayers = false)
        {
            var fileLayer = await _overlayFS.ReadOciFileLayerAsync();
            var artifactImage = await PackOciImageAsync(fileLayer, ignoreBaseLayers);

            return await _client.PushImageAsync(name, tag, artifactImage);
        }

        /// <summary>
        /// Extract layers and merge files from layers in order.
        /// The order of layers is given in manifest.
        /// </summary>
        private async Task<OciFileLayer> UnpackOciImageAsync(ArtifactImage image)
        {
            var sortedLayers = _overlayOperator.Sort(image.Blobs, image.Manifest);
            if (sortedLayers.Count == 0)
            {
                return new OciFileLayer();
            }

            // First layer is the base layer.
            // Clear base layer folder before writing.
            _overlayFS.ClearBaseLayerFolder();
            await _overlayFS.WriteBaseLayerAsync(sortedLayers[0]);

            // Decompress rawlayers to OCI files.
            var ociFileLayers = _overlayOperator.Extract(sortedLayers);

            // Merge OCI files.
            var unpackedLayer = _overlayOperator.Merge(ociFileLayers);
            return unpackedLayer;
        }

        /// <summary>
        /// Generate and archive the diff layer into tar.gz file.
        /// </summary>
        private async Task<ArtifactImage> PackOciImageAsync(OciFileLayer ociFileLayer, bool ignoreBaseLayers)
        {
            ArtifactBlob baseArtifactLayer = new OciFileLayer();
            if (!ignoreBaseLayers)
            {
                baseArtifactLayer = await _overlayFS.ReadBaseLayerAsync();
            }

            var extractedBaseLayer = _overlayOperator.Extract(baseArtifactLayer);

            // Generate diff layer by comparing files' digests.
            var diffLayer = _overlayOperator.GenerateDiffLayer(ociFileLayer, extractedBaseLayer);

            // Archive the diff layer.
            var diffArtifactLayer = _overlayOperator.Archive(diffLayer);
            return new ArtifactImage() { Blobs = new List<ArtifactBlob> { baseArtifactLayer, diffArtifactLayer } };
        }
    }
}
