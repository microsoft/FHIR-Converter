// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
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
            EnsureArg.IsNotNull(imageReference);
            EnsureArg.IsNotNull(workingFolder);

            _orasClient = new OrasClient(imageReference, workingFolder);
            _overlayFS = new OverlayFileSystem(workingFolder);
            _overlayOperator = new OverlayOperator();
        }

        public string PullOCIImage()
        {
            _overlayFS.ClearImageLayerFolder();
            var orasOutput = _orasClient.PullImage();
            return orasOutput;
        }

        public void UnpackOCIImage()
        {
            var rawLayers = _overlayFS.ReadImageLayers();
            if (rawLayers == null)
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
            List<OCIArtifactLayer> baseLayers = null;
            if (!ignoreBaseLayers)
            {
                baseLayers = _overlayFS.ReadBaseLayers();
            }

            var snapshotLayer = GenerateSnapshotLayer(baseLayers);
            var diffLayer = _overlayOperator.GenerateDiffLayer(mergedLayer, snapshotLayer);
            var diffArtifactLayer = _overlayOperator.ArchiveOCIFileLayer(diffLayer);
            var allLayers = new List<OCIArtifactLayer>() { };

            if (diffArtifactLayer != null)
            {
                allLayers.Add(diffArtifactLayer);
            }

            allLayers.AddRange(baseLayers ?? new List<OCIArtifactLayer>());

            _overlayFS.WriteImageLayers(allLayers);
        }

        public string PushOCIImage()
        {
            var orasOutput = _orasClient.PushImage();
            return orasOutput;
        }

        private OCIFileLayer GenerateSnapshotLayer(List<OCIArtifactLayer> baseLayers)
        {
            if (baseLayers == null)
            {
                return null;
            }

            var fileLayers = _overlayOperator.ExtractOCIFileLayers(baseLayers);
            var sortedFileLayers = _overlayOperator.SortOCIFileLayersBySequenceNumber(fileLayers);
            var mergedFileLayer = _overlayOperator.MergeOCIFileLayers(sortedFileLayers);
            return mergedFileLayer;
        }
    }
}
