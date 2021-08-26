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
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.TemplateManagement.Overlay
{
    public class OverlayFileSystem : IOverlayFileSystem
    {
        private readonly string _workingFolder;
        private readonly string _workingImageFolder;
        private readonly string _workingImageLayerFolder;
        private readonly string _workingBaseLayerFolder;

        public OverlayFileSystem(string workingFolder)
        {
            EnsureArg.IsNotNullOrEmpty(workingFolder, nameof(workingFolder));

            _workingFolder = workingFolder;
            _workingImageFolder = Path.Combine(workingFolder, Constants.HiddenImageFolder);
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

        public List<ArtifactBlob> ReadImageLayers()
        {
            return ReadOCIArtifactLayers(_workingImageLayerFolder);
        }

        public void WriteImageLayers(List<ArtifactBlob> imageLayers)
        {
            EnsureArg.IsNotNull(imageLayers, nameof(imageLayers));

            // Clear image layer folder before writing.
            ClearImageLayerFolder();
            Directory.CreateDirectory(_workingImageLayerFolder);

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

        public void WriteManifest(ManifestWrapper manifest)
        {
            EnsureArg.IsNotNull(manifest, nameof(manifest));

            Directory.CreateDirectory(_workingImageFolder);
            File.WriteAllText(Path.Combine(_workingImageFolder, Constants.ManifestFileName), JsonConvert.SerializeObject(manifest));
        }

        public ArtifactBlob ReadBaseLayer()
        {
            var layers = ReadOCIArtifactLayers(_workingBaseLayerFolder);
            if (layers.Count() > 1)
            {
                throw new OverlayException(TemplateManagementErrorCode.BaseLayerLoadFailed, $"Base layer load failed. More than one layer in the base layer folder.");
            }

            return layers.Count == 0 ? new OCIFileLayer() : layers[0];
        }

        public void WriteBaseLayer(ArtifactBlob baseLayer)
        {
            EnsureArg.IsNotNull(baseLayer, nameof(baseLayer));

            // Clear base layer folder before writing.
            ClearBaseLayerFolder();
            Directory.CreateDirectory(_workingBaseLayerFolder);
            baseLayer.WriteToFile(Path.Combine(_workingBaseLayerFolder, "layer1.tar.gz"));
        }

        public bool IsCleanWorkingFolder()
        {
            return !(Directory.Exists(_workingFolder) && Directory.EnumerateFileSystemEntries(_workingFolder).Any());
        }

        public void ClearWorkingFolder()
        {
            IoUtility.ClearFolder(_workingFolder);
            Directory.CreateDirectory(_workingFolder);
        }

        public void ClearImageFolder()
        {
            IoUtility.ClearFolder(_workingImageFolder);
            Directory.CreateDirectory(_workingFolder);
        }

        public void ClearImageLayerFolder()
        {
            IoUtility.ClearFolder(_workingImageLayerFolder);
            Directory.CreateDirectory(_workingImageLayerFolder);
        }

        public void ClearBaseLayerFolder()
        {
            IoUtility.ClearFolder(_workingBaseLayerFolder);
            Directory.CreateDirectory(_workingBaseLayerFolder);
        }

        private List<ArtifactBlob> ReadOCIArtifactLayers(string folder)
        {
            EnsureArg.IsNotNullOrEmpty(folder, nameof(folder));

            if (!Directory.Exists(folder))
            {
                return new List<ArtifactBlob>();
            }

            var result = new List<ArtifactBlob>();
            var layersPath = Directory.EnumerateFiles(folder, "*.tar.gz", SearchOption.AllDirectories);
            foreach (var tarGzFile in layersPath)
            {
                var artifactLayer = new ArtifactBlob();
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
