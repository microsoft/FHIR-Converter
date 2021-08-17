// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.ContainerRegistry.Models;
using Microsoft.Health.Fhir.TemplateManagement.Client;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Microsoft.Health.Fhir.TemplateManagement.Overlay;

namespace Microsoft.Health.Fhir.TemplateManagement
{
    public class OCIFileManager
    {
        private readonly IOCIClient _client;
        private readonly OverlayFileSystem _overlayFS;
        private readonly OverlayOperator _overlayOperator;

        public OCIFileManager(string imageReference, string workingFolder)
        {
            ImageInfo.ValidateImageReference(imageReference);
            _client = new OrasClient(imageReference);
            _client.InitClientEnvironment();
            _overlayFS = new OverlayFileSystem(workingFolder);
            _overlayOperator = new OverlayOperator();
        }

        public async Task<ManifestWrapper> PullOCIImageAsync()
        {
            _overlayFS.ClearImageLayerFolder();
            return await _client.PullImageAsync(_overlayFS.WorkingImageLayerFolder);
        }

        public void UnpackOCIImage(ManifestWrapper manifest)
        {
            var rawLayers = _overlayFS.ReadImageLayers(manifest);
            if (rawLayers.Count == 0)
            {
                return;
            }

            // First layer is the base layer.
            _overlayFS.WriteBaseLayer(rawLayers[0]);

            // Decompress rawlayers to OCI files.
            var ociFileLayers = _overlayOperator.Extract(rawLayers);

            // Merge OCI files.
            var content = _overlayOperator.Merge(ociFileLayers);
            _overlayFS.WriteOCIFileLayer(content);
        }

        public void PackOCIImage(bool ignoreBaseLayers = false)
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

        public async Task PushOCIImageAsync()
        {
            await _client.PushImageAsync(_overlayFS.WorkingImageLayerFolder);
        }
    }
}
