// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
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

        public OCIFileManager(string url, string workingFolder)
        {
            _overlayFS = new OverlayFileSystem(workingFolder);
            _client = new OrasClient(url);
            _overlayOperator = new OverlayOperator();
        }

        /// <summary>
        /// Pull image layers into working folder.
        /// Layers will be written into hidden image folder.
        /// </summary>
        /// <returns>Image information.</returns>
        /// <param name="forceOverride">Whether to override if folder is not empty.</param>
        public async Task<ArtifactImage> PullOCIImageAsync(string imageName, string reference, bool forceOverride = false)
        {
            if (!_overlayFS.IsCleanWorkingFolder())
            {
                if (!forceOverride)
                {
                    throw new TemplateManagementException($"The folder is not empty. If force to override, please add -f in parameters");
                }

                _overlayFS.ClearImageLayerFolder();
            }

            var artifactImage = await _client.PullImageAsync(imageName, reference);
            _overlayFS.WriteManifest(artifactImage.Manifest);
            _overlayFS.WriteImageLayers(artifactImage.Blobs);
            var fileLayer = UnpackOCIImage(artifactImage);
            _overlayFS.WriteOCIFileLayer(fileLayer);
            return artifactImage;
        }

        /// <summary>
        /// Push image layers from working folder in order.
        /// </summary>
        /// <param name="ignoreBaseLayers">Whether ignore base layer when generating new layers.</param>
        public async Task<ArtifactImage> PushOCIImageAsync(string imageName, string reference, bool ignoreBaseLayers = false)
        {
            var fileLayer = _overlayFS.ReadOCIFileLayer();
            var artifactImage = PackOCIImage(fileLayer, ignoreBaseLayers);
            _overlayFS.WriteImageLayers(artifactImage.Blobs);
            return await _client.PushImageAsync(imageName, reference, artifactImage);
        }

        /// <summary>
        /// Extract layers and merge files from layers in order.
        /// The order of layers is given in manifest.
        /// </summary>
        /// <param name="manifest">Manifest of the image.</param>
        private OCIFileLayer UnpackOCIImage(ArtifactImage image)
        {
            var sortedLayers = _overlayOperator.Sort(image.Blobs, image.Manifest);
            if (sortedLayers.Count == 0)
            {
                return new OCIFileLayer();
            }

            // First layer is the base layer.
            _overlayFS.WriteBaseLayer(sortedLayers[0]);

            // Decompress rawlayers to OCI files.
            var ociFileLayers = _overlayOperator.Extract(sortedLayers);

            // Merge OCI files.
            var unpackedLayer = _overlayOperator.Merge(ociFileLayers);
            return unpackedLayer;
        }

        /// <summary>
        /// Generate and archive the diff layer into tar.gz file.
        /// </summary>
        /// <param name="ignoreBaseLayers">Whether ignore base layer when generating diff layer.</param>
        private ArtifactImage PackOCIImage(OCIFileLayer ociFileLayer, bool ignoreBaseLayers)
        {
            Models.ArtifactBlob baseArtifactLayer = new OCIFileLayer();
            if (!ignoreBaseLayers)
            {
                baseArtifactLayer = _overlayFS.ReadBaseLayer();
            }

            var extractedBaseLayer = _overlayOperator.Extract(baseArtifactLayer);

            // Generate diff layer by comparing files' digests.
            var diffLayer = _overlayOperator.GenerateDiffLayer(ociFileLayer, extractedBaseLayer);

            // Archive the diff layer.
            var diffArtifactLayer = _overlayOperator.Archive(diffLayer);
            return new ArtifactImage() { Blobs = new List<Models.ArtifactBlob> { baseArtifactLayer, diffArtifactLayer } };
        }
    }
}
