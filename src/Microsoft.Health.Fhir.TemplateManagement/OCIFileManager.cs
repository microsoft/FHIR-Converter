// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.ContainerRegistry.Models;
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
        private readonly string _imageReference;

        public OCIFileManager(string imageReference, string workingFolder)
        {
            ImageInfo.ValidateImageReference(imageReference);
            _imageReference = imageReference;

            _overlayFS = new OverlayFileSystem(workingFolder);
            _client = new OrasClient(Path.Combine(workingFolder, Constants.HiddenLayersFolder));
            _overlayOperator = new OverlayOperator();
        }

        /// <summary>
        /// Pull image layers into working folder.
        /// Layers will be written into hidden image folder.
        /// </summary>
        /// <returns>Image information.</returns>
        /// <param name="forceOverride">Whether to override if folder is not empty.</param>
        public async Task<ImageInfo> PullOCIImageAsync(bool forceOverride = false)
        {
            if (!_overlayFS.IsCleanWorkingFolder())
            {
                if (!forceOverride)
                {
                    throw new TemplateManagementException($"The folder is not empty. If force to override, please add -f in parameters");
                }

                _overlayFS.ClearWorkingFolder();
            }

            var imageInfo = await _client.PullImageAsync(_imageReference);
            _overlayFS.WriteManifest(imageInfo.Manifest);
            UnpackOCIImage(imageInfo.Manifest);
            return imageInfo;
        }

        /// <summary>
        /// Push image layers from working folder in order.
        /// </summary>
        /// <param name="ignoreBaseLayers">Whether ignore base layer when generating new layers.</param>
        public async Task<ImageInfo> PushOCIImageAsync(bool ignoreBaseLayers = false)
        {
            PackOCIImage(ignoreBaseLayers);
            return await _client.PushImageAsync(_imageReference);
        }

        /// <summary>
        /// Extract layers and merge files from layers in order.
        /// The order of layers is given in manifest.
        /// </summary>
        /// <param name="manifest">Manifest of the image.</param>
        private void UnpackOCIImage(ManifestWrapper manifest)
        {
            var rawLayers = _overlayFS.ReadImageLayers();
            var sortedLayers = _overlayOperator.Sort(rawLayers, manifest);
            if (sortedLayers.Count == 0)
            {
                return;
            }

            // First layer is the base layer.
            _overlayFS.WriteBaseLayer(sortedLayers[0]);

            // Decompress rawlayers to OCI files.
            var ociFileLayers = _overlayOperator.Extract(sortedLayers);

            // Merge OCI files.
            var content = _overlayOperator.Merge(ociFileLayers);
            _overlayFS.WriteOCIFileLayer(content);
        }

        /// <summary>
        /// Generate and archive the diff layer into tar.gz file.
        /// </summary>
        /// <param name="ignoreBaseLayers">Whether ignore base layer when generating diff layer.</param>
        private void PackOCIImage(bool ignoreBaseLayers)
        {
            var ociFileLayer = _overlayFS.ReadOCIFileLayer();
            OCIArtifactLayer baseArtifactLayer = new OCIFileLayer();
            if (!ignoreBaseLayers)
            {
                baseArtifactLayer = _overlayFS.ReadBaseLayer();
            }

            var extractedBaseLayer = _overlayOperator.Extract(baseArtifactLayer);

            // Generate diff layer by comparing files' digests.
            var diffLayer = _overlayOperator.GenerateDiffLayer(ociFileLayer, extractedBaseLayer);

            // Archive the diff layer.
            var diffArtifactLayer = _overlayOperator.Archive(diffLayer);
            _overlayFS.WriteImageLayers(new List<OCIArtifactLayer> { baseArtifactLayer, diffArtifactLayer });
        }
    }
}
