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
    public class OCIFileManager
    {
        private readonly IOCIClient _client;
        private readonly OverlayFileSystem _overlayFS;
        private readonly OverlayOperator _overlayOperator;

        public OCIFileManager(string registry, string workingFolder)
        {
            _overlayFS = new OverlayFileSystem(workingFolder);
            _client = new OrasClient(registry, Path.Combine(workingFolder, Constants.HiddenLayersFolder));
            _overlayOperator = new OverlayOperator();
        }

        /// <summary>
        /// Pull image layers into working folder.
        /// Layers will be written into hidden image folder.
        /// </summary>
        public async Task<ArtifactImage> PullOCIImageAsync(string name, string reference, bool forceOverride = false)
        {
            if (!_overlayFS.IsCleanWorkingFolder())
            {
                if (!forceOverride)
                {
                    throw new TemplateManagementException($"The folder is not empty. If force to override, please add -f in parameters");
                }

                _overlayFS.ClearImageLayerFolder();
            }

            var artifactImage = await _client.PullImageAsync(name, reference);

            // Optional to write in user's folder.
            await _overlayFS.WriteManifestAsync(artifactImage.Manifest);

            var fileLayer = await UnpackOCIImageAsync (artifactImage);
            await _overlayFS.WriteOCIFileLayerAsync(fileLayer);
            return artifactImage;
        }

        /// <summary>
        /// Push image layers from working folder in order.
        /// </summary>
        public async Task<string> PushOCIImageAsync(string name, string tag, bool ignoreBaseLayers = false)
        {
            var fileLayer = await _overlayFS.ReadOCIFileLayerAsync();
            var artifactImage = await PackOCIImageAsync(fileLayer, ignoreBaseLayers);

            return await _client.PushImageAsync(name, tag, artifactImage);
        }

        /// <summary>
        /// Extract layers and merge files from layers in order.
        /// The order of layers is given in manifest.
        /// </summary>
        private async Task<OCIFileLayer> UnpackOCIImageAsync(ArtifactImage image)
        {
            var sortedLayers = _overlayOperator.Sort(image.Blobs, image.Manifest);
            if (sortedLayers.Count == 0)
            {
                return new OCIFileLayer();
            }

            // First layer is the base layer.
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
        private async Task<ArtifactImage> PackOCIImageAsync(OCIFileLayer ociFileLayer, bool ignoreBaseLayers)
        {
            ArtifactBlob baseArtifactLayer = new OCIFileLayer();
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
