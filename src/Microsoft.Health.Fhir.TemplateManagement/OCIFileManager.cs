// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
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

            var fileLayers = _overlayOperator.ExtractOCIFileLayers(rawLayers);
            var sortedLayers = _overlayOperator.SortOCIFileLayersBySequenceNumber(fileLayers);
            _overlayFS.WriteBaseLayers(sortedLayers.GetRange(0, 1).Cast<OCIArtifactLayer>().ToList());
            var mergedFileLayer = _overlayOperator.MergeOCIFileLayers(sortedLayers);
            _overlayFS.WriteMergedOCIFileLayer(mergedFileLayer);
        }

        public List<OCIArtifactLayer> PackOCIImage(bool ignoreBaseLayers = false)
        {
            var mergedLayer = _overlayFS.ReadMergedOCIFileLayer();
            var baseArtifactLayers = new List<OCIArtifactLayer>();
            if (!ignoreBaseLayers)
            {
                baseArtifactLayers = _overlayFS.ReadBaseLayers();
            }

            var fileLayers = _overlayOperator.ExtractOCIFileLayers(baseArtifactLayers);
            var sortedFileLayers = _overlayOperator.SortOCIFileLayersBySequenceNumber(fileLayers);
            var snapshotLayer = _overlayOperator.MergeOCIFileLayers(sortedFileLayers);

            var diffLayer = _overlayOperator.GenerateDiffLayer(mergedLayer, snapshotLayer);
            var diffArtifactLayer = _overlayOperator.ArchiveOCIFileLayer(diffLayer);
            var baseLayers = sortedFileLayers.Select(layer => (OCIArtifactLayer)layer).ToList();
            baseLayers.Add(diffArtifactLayer);
            return baseLayers.Where(layer => layer.Content != null).ToList();
        }

        public async Task PushOCIImageAsync(List<OCIArtifactLayer> sortedlayers)
        {
            _overlayFS.WriteImageLayers(sortedlayers);
            await _orasClient.PushImageAsync(_overlayFS.WorkingImageLayerFolder, sortedlayers.Select(layer => layer.FileName).ToList());
        }
    }
}
