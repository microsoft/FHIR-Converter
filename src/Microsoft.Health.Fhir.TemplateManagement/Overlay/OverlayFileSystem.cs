// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using EnsureThat;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Models;

namespace Microsoft.Health.Fhir.TemplateManagement.Overlay
{
    public class OverlayFileSystem : IOverlayFileSystem
    {
        private readonly string _workingFolder;
        private readonly string _workingImageLayerFolder;
        private readonly string _workingBaseLayerFolder;

        public OverlayFileSystem(string workingFolder)
        {
            EnsureArg.IsNotNullOrEmpty(workingFolder, nameof(workingFolder));

            _workingFolder = workingFolder;
            _workingImageLayerFolder = Path.Combine(workingFolder, Constants.HiddenLayersFolder);
            _workingBaseLayerFolder = Path.Combine(workingFolder, Constants.HiddenBaseLayerFolder);
        }

        public OCIFileLayer ReadOCIFileLayer()
        {
            var filePaths = Directory.EnumerateFiles(_workingFolder, "*.*", SearchOption.AllDirectories)
                .Where(f => !string.Equals(GetTopDirectoryPath(Path.GetRelativePath(_workingFolder, f)), Constants.HiddenImageFolder));

            Dictionary<string, byte[]> fileContent = new Dictionary<string, byte[]>() { };
            foreach (var oneFile in filePaths)
            {
                fileContent.Add(Path.GetRelativePath(_workingFolder, oneFile), File.ReadAllBytes(oneFile));
            }

            OCIFileLayer fileLayer = new OCIFileLayer() { FileContent = fileContent };
            return fileLayer;
        }

        public void WriteOCIFileLayer(OCIFileLayer oneLayer)
        {
            EnsureArg.IsNotNull(oneLayer, nameof(oneLayer));

            Directory.CreateDirectory(_workingFolder);
            foreach (var oneFile in oneLayer.FileContent)
            {
                var filePath = Path.Combine(_workingFolder, oneFile.Key);
                var directory = Path.GetDirectoryName(filePath);
                Directory.CreateDirectory(directory);
                File.WriteAllBytes(Path.Combine(_workingFolder, oneFile.Key), oneFile.Value);
            }
        }

        public List<OCIArtifactLayer> ReadImageLayers()
        {
            return ReadOCIArtifactLayers(_workingImageLayerFolder);
        }

        public void WriteImageLayers(List<OCIArtifactLayer> imageLayers)
        {
            EnsureArg.IsNotNull(imageLayers, nameof(imageLayers));

            ClearFolder(_workingImageLayerFolder);

            // Used to label layers sequence. In this version, only two layers will be generated.
            // Should not exceed 9 in the future.
            var layerNumber = 1;
            foreach (var layer in imageLayers)
            {
                if (layer.Content == null)
                {
                    continue;
                }

                layer.WriteToFile(Path.Combine(_workingImageLayerFolder, string.Format("layer{0}.tar.gz", layerNumber)));
                layerNumber += 1;
            }
        }

        public void WriteManifest(string manifest)
        {
            EnsureArg.IsNotNull(manifest, nameof(manifest));

            File.WriteAllText(Path.Combine(_workingFolder, Constants.HiddenImageFolder, Constants.ManifestFileName), manifest);
        }

        public OCIArtifactLayer ReadBaseLayer()
        {
            var layers = ReadOCIArtifactLayers(_workingBaseLayerFolder);
            if (layers.Count() > 1)
            {
                throw new OverlayException(TemplateManagementErrorCode.BaseLayerLoadFailed, $"Base layer load failed. More than one layer in the base layer folder.");
            }

            return layers.Count == 0 ? new OCIFileLayer() : layers[0];
        }

        public void WriteBaseLayer(OCIArtifactLayer baseLayer)
        {
            EnsureArg.IsNotNull(baseLayer, nameof(baseLayer));

            ClearFolder(_workingBaseLayerFolder);
            baseLayer.WriteToFile(Path.Combine(_workingBaseLayerFolder, "layer1.tar.gz"));
        }

        public void ClearImageLayerFolder()
        {
            ClearFolder(_workingImageLayerFolder);
        }

        public void ClearBaseLayerFolder()
        {
            ClearFolder(_workingBaseLayerFolder);
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
    }
}
