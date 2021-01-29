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
using Microsoft.Health.Fhir.TemplateManagement.Utilities;

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

        public async Task<string> PullOCIImageAsync()
        {
            _overlayFS.ClearImageLayerFolder();
            return await _orasClient.PullImageAsync(_overlayFS.WorkingImageLayerFolder);
        }

        public void UnpackOCIImage()
        {
            var rawLayers = _overlayFS.ReadImageLayers();
            if (rawLayers.Count == 0)
            {
                return;
            }

            var fileLayers = _overlayOperator.ExtractOCIFileLayers(rawLayers);
            var sortedLayers = _overlayOperator.SortOCIFileLayers(fileLayers);
            _overlayFS.WriteBaseLayers(sortedLayers.GetRange(0, 1).Cast<OCIArtifactLayer>().ToList());
            var mergedFileLayer = _overlayOperator.MergeOCIFileLayers(sortedLayers);
            _overlayFS.WriteMergedOCIFileLayer(mergedFileLayer);
        }

        public void UnpackOCIImage(string digest)
        {
            var rawLayers = _overlayFS.ReadImageLayers();
            if (rawLayers.Count == 0)
            {
                return;
            }

            var fileLayers = _overlayOperator.ExtractOCIFileLayers(rawLayers);

            // Load manifest.
            var manifest = _overlayFS.ReadManifest(digest);
            List<OCIFileLayer> sortedLayers;
            if (manifest != null)
            {
                ValidationUtility.ValidateManifest(manifest);
                _overlayFS.WriteManifest(manifest);
            }

            sortedLayers = _overlayOperator.SortOCIFileLayers(fileLayers, manifest);

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
            var manifest = _overlayFS.ReadManifest(_overlayFS.WorkingImageFolder, Constants.Manifest);

            var baseOCIFileLayers = _overlayOperator.SortOCIFileLayers(fileLayers, manifest, true);
            var snapshotLayer = _overlayOperator.MergeOCIFileLayers(baseOCIFileLayers);
            var diffLayer = _overlayOperator.GenerateDiffLayer(mergedLayer, snapshotLayer);
            var diffArtifactLayer = _overlayOperator.ArchiveOCIFileLayer(diffLayer);
            baseOCIFileLayers.Add((OCIFileLayer)diffArtifactLayer);
            var sortedlayers = baseOCIFileLayers.Where(layer => layer.Content != null).Select(layer => (OCIArtifactLayer)layer).ToList();
            return sortedlayers;
        }

        public async Task PushOCIImageAsync(List<OCIArtifactLayer> sortedlayers)
        {
            _overlayFS.WriteImageLayers(sortedlayers);
            await _orasClient.PushImageAsync(_overlayFS.WorkingImageLayerFolder, sortedlayers.Select(layer => layer.FileName).ToList());
        }
    }
}
