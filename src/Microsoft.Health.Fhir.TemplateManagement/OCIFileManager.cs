// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EnsureThat;
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

        public void PackOCIImage(bool ignoreBaseLayers = false)
        {
            var mergedLayer = _overlayFS.ReadMergedOCIFileLayer();
            var baseArtifactLayers = new List<OCIArtifactLayer>();
            if (!ignoreBaseLayers)
            {
                baseArtifactLayers = _overlayFS.ReadBaseLayers();
            }

            var snapshotLayer = GenerateBaseFileLayer(baseArtifactLayers);
            var diffLayer = _overlayOperator.GenerateDiffLayer(mergedLayer, snapshotLayer);
            var diffArtifactLayer = _overlayOperator.ArchiveOCIFileLayer(diffLayer);
            if (baseArtifactLayers.Count == 0)
            {
                _overlayFS.WriteBaseLayers(new List<OCIArtifactLayer> { diffArtifactLayer });
            }

            baseArtifactLayers.Add(diffArtifactLayer);
            _overlayFS.WriteImageLayers(baseArtifactLayers);
        }

        public async Task PushOCIImageAsync()
        {
            await _orasClient.PushImageAsync(_overlayFS.WorkingImageLayerFolder);
        }

        private OCIFileLayer GenerateBaseFileLayer(List<OCIArtifactLayer> baseArtifactLayers)
        {
            if (baseArtifactLayers == null || baseArtifactLayers.Count == 0)
            {
                return null;
            }

            var fileLayers = _overlayOperator.ExtractOCIFileLayers(baseArtifactLayers);
            var sortedFileLayers = _overlayOperator.SortOCIFileLayersBySequenceNumber(fileLayers);
            var mergedFileLayer = _overlayOperator.MergeOCIFileLayers(sortedFileLayers);
            return mergedFileLayer;
        }
    }
}
