// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using EnsureThat;
using Microsoft.Azure.ContainerRegistry.Models;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Microsoft.Health.Fhir.TemplateManagement.Utilities;

namespace Microsoft.Health.Fhir.TemplateManagement.Overlay
{
    public class OverlayFileSystem : IOverlayFileSystem
    {
        public OverlayFileSystem(string workingFolder)
        {
            EnsureArg.IsNotNullOrEmpty(workingFolder, nameof(workingFolder));

            WorkingFolder = workingFolder;
            WorkingImageLayerFolder = Path.Combine(workingFolder, Constants.HiddenLayersFolder);
            WorkingBaseLayerFolder = Path.Combine(workingFolder, Constants.HiddenBaseLayerFolder);
        }

        public string WorkingFolder { get; set; }

        public string WorkingImageLayerFolder { get; set; }

        public string WorkingBaseLayerFolder { get; set; }

        public OCIFileLayer ReadOCIFileLayer()
        {
            var filePaths = Directory.EnumerateFiles(WorkingFolder, "*.*", SearchOption.AllDirectories)
                .Where(f => !string.Equals(GetTopDirectoryPath(Path.GetRelativePath(WorkingFolder, f)), Constants.HiddenImageFolder));

            Dictionary<string, byte[]> fileContent = new Dictionary<string, byte[]>() { };
            foreach (var oneFile in filePaths)
            {
                fileContent.Add(Path.GetRelativePath(WorkingFolder, oneFile), File.ReadAllBytes(oneFile));
            }

            OCIFileLayer fileLayer = new OCIFileLayer() { FileContent = fileContent };
            return fileLayer;
        }

        public void WriteOCIFileLayer(OCIFileLayer oneLayer)
        {
            EnsureArg.IsNotNull(oneLayer, nameof(oneLayer));

            Directory.CreateDirectory(WorkingFolder);
            foreach (var oneFile in oneLayer.FileContent)
            {
                var filePath = Path.Combine(WorkingFolder, oneFile.Key);
                var directory = Path.GetDirectoryName(filePath);
                Directory.CreateDirectory(directory);
                File.WriteAllBytes(Path.Combine(WorkingFolder, oneFile.Key), oneFile.Value);
            }
        }

        public List<OCIArtifactLayer> ReadImageLayers(ManifestWrapper manifest)
        {
            EnsureArg.IsNotNull(manifest, nameof(manifest));

            var imageLayers = ReadOCIArtifactLayers(WorkingImageLayerFolder);
            return SortLayers(imageLayers, manifest);
        }

        public void WriteImageLayers(List<OCIArtifactLayer> imageLayers)
        {
            EnsureArg.IsNotNull(imageLayers, nameof(imageLayers));

            ClearFolder(WorkingImageLayerFolder);

            // Used to label layers sequence. In this version, only two layers will be generated.
            // Should not exceed 9 in the future.
            var layerNumber = 1;
            foreach (var layer in imageLayers)
            {
                if (layer.Content == null)
                {
                    continue;
                }

                layer.WriteToFile(Path.Combine(WorkingImageLayerFolder, string.Format("layer{0}.tar.gz", layerNumber)));
                layerNumber += 1;
            }
        }

        public void WriteManifest(string manifest)
        {
            EnsureArg.IsNotNull(manifest, nameof(manifest));

            File.WriteAllText(Path.Combine(WorkingFolder, Constants.HiddenImageFolder, Constants.ManifestFileName), manifest);
        }

        public OCIArtifactLayer ReadBaseLayer()
        {
            var layers = ReadOCIArtifactLayers(WorkingBaseLayerFolder);
            if (layers.Count() > 1)
            {
                throw new OverlayException(TemplateManagementErrorCode.BaseLayerLoadFailed, $"Base layer load failed. More than one layer in the base layer folder.");
            }

            return layers.Count == 0 ? new OCIFileLayer() : layers[0];
        }

        public void WriteBaseLayer(OCIArtifactLayer baseLayer)
        {
            EnsureArg.IsNotNull(baseLayer, nameof(baseLayer));

            ClearFolder(WorkingBaseLayerFolder);
            baseLayer.WriteToFile(Path.Combine(WorkingBaseLayerFolder, "layer1.tar.gz"));
        }

        public void ClearImageLayerFolder()
        {
            ClearFolder(WorkingImageLayerFolder);
        }

        public void ClearBaseLayerFolder()
        {
            ClearFolder(WorkingBaseLayerFolder);
        }

        private void ClearFolder(string directory)
        {
            EnsureArg.IsNotNullOrEmpty(directory, nameof(directory));

            if (!Directory.Exists(directory))
            {
                return;
            }

            DirectoryInfo folder = new DirectoryInfo(directory);
            folder.Delete(true);
        }

        private List<OCIArtifactLayer> ReadOCIArtifactLayers(string folder)
        {
            EnsureArg.IsNotNullOrEmpty(folder, nameof(folder));

            if (!Directory.Exists(folder))
            {
                return new List<OCIArtifactLayer>();
            }

            var result = new List<OCIArtifactLayer>();
            var layersPath = Directory.EnumerateFiles(folder, "*.tar.gz", SearchOption.AllDirectories);
            foreach (var tarGzFile in layersPath)
            {
                var artifactLayer = new OCIArtifactLayer();
                artifactLayer.ReadFromFile(tarGzFile);
                if (artifactLayer.Content != null)
                {
                    result.Add(artifactLayer);
                }
            }

            return result;
        }

        private string GetTopDirectoryPath(string path)
        {
            if (string.IsNullOrEmpty(Path.GetDirectoryName(path)))
            {
                return path;
            }
            else
            {
                return GetTopDirectoryPath(Path.GetDirectoryName(path));
            }
        }

        private List<OCIArtifactLayer> SortLayers(List<OCIArtifactLayer> imageLayers, ManifestWrapper manifest)
        {
            var sortedLayers = new List<OCIArtifactLayer>();
            ValidationUtility.ValidateManifest(manifest);
            foreach (var layerInfo in manifest.Layers)
            {
                var currentLayers = imageLayers.Where(layer => layer.Digest == layerInfo.Digest).ToList();
                if (!currentLayers.Any())
                {
                    throw new OverlayException(TemplateManagementErrorCode.ImageLayersNotFound, $"Layer {layerInfo.Digest} not found.");
                }

                sortedLayers.Add(currentLayers[0]);
            }

            return sortedLayers;
        }
    }
}
