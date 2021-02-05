// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Health.Fhir.TemplateManagement.Client;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Microsoft.Health.Fhir.TemplateManagement.Overlay;

namespace Microsoft.Health.Fhir.TemplateManagement
{
    public class OCIFileManager
    {
        private readonly OrasClient _orasClient;
        private readonly OverlayFileSystem _overlayFS;
        private readonly OverlayOperator _overlayOperator;

        public OCIFileManager(string imageReference, string workingFolder)
        {
            ImageInfo.ValidateImageReference(imageReference);
            _orasClient = new OrasClient(imageReference);
            _overlayFS = new OverlayFileSystem(workingFolder);
            _overlayOperator = new OverlayOperator();
        }

        public async Task PullOCIImageAsync()
        {
            _overlayFS.ClearImageLayerFolder();
            await _orasClient.PullImageAsync(_overlayFS.WorkingImageLayerFolder);
        }

        public void UnpackOCIImage()
        {
            var rawLayers = _overlayFS.ReadImageLayers();
            if (rawLayers.Count == 0)
            {
                return;
            }

            _overlayFS.WriteBaseLayer(rawLayers[0]);
            var ociFileLayers = _overlayOperator.ExtractArtifactLayers(rawLayers);
            var mergedFileLayer = _overlayOperator.MergeOCIFileLayers(ociFileLayers);
            _overlayFS.WriteOCIFileLayer(mergedFileLayer);
        }

        public void PackOCIImage(bool ignoreBaseLayers = false)
        {
            var ociFileLayer = _overlayFS.ReadOCIFileLayer();
            OCIArtifactLayer baseArtifactLayer = new OCIFileLayer();
            if (!ignoreBaseLayers)
            {
                baseArtifactLayer = _overlayFS.ReadBaseLayer();
            }

            var extractedBaseLayer = _overlayOperator.ExtractArtifactLayer(baseArtifactLayer);
            var diffLayer = _overlayOperator.GenerateDiffLayer(ociFileLayer, extractedBaseLayer);
            var diffArtifactLayer = _overlayOperator.ArchiveOCIFileLayer(diffLayer);
            _overlayFS.WriteImageLayers(new List<OCIArtifactLayer> { baseArtifactLayer, diffArtifactLayer });
        }

        public async Task PushOCIImageAsync()
        {
            await _orasClient.PushImageAsync(_overlayFS.WorkingImageLayerFolder);
        }
    }
}
