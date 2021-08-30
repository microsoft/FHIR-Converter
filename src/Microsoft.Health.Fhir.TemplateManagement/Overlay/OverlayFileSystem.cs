// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task<OCIFileLayer> ReadOCIFileLayerAsync()
        {
            var filePaths = Directory.EnumerateFiles(_workingFolder, "*.*", SearchOption.AllDirectories)
                .Where(f => !string.Equals(GetTopDirectoryPath(Path.GetRelativePath(_workingFolder, f)), Constants.HiddenImageFolder));

            Dictionary<string, byte[]> fileContent = new Dictionary<string, byte[]>() { };
            foreach (var oneFile in filePaths)
            {
                fileContent.Add(Path.GetRelativePath(_workingFolder, oneFile), await File.ReadAllBytesAsync(oneFile));
            }

            OCIFileLayer fileLayer = new OCIFileLayer() { FileContent = fileContent };
            return fileLayer;
        }

        public async Task WriteOCIFileLayerAsync(OCIFileLayer oneLayer)
        {
            EnsureArg.IsNotNull(oneLayer, nameof(oneLayer));

            Directory.CreateDirectory(_workingFolder);
            foreach (var oneFile in oneLayer.FileContent)
            {
                var filePath = Path.Combine(_workingFolder, oneFile.Key);
                var directory = Path.GetDirectoryName(filePath);
                Directory.CreateDirectory(directory);
                await File.WriteAllBytesAsync(Path.Combine(_workingFolder, oneFile.Key), oneFile.Value);
            }
        }

        public async Task<List<ArtifactBlob>> ReadImageLayersAsync()
        {
            return await ReadOCIArtifactLayersAsync(_workingImageLayerFolder);
        }

        public async Task WriteImageLayersAsync(List<ArtifactBlob> imageLayers)
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

                await layer.WriteToFileAsync(Path.Combine(_workingImageLayerFolder, string.Format("layer{0}.tar.gz", layerNumber)));
                layerNumber += 1;
            }
        }

        public async Task WriteManifestAsync(ManifestWrapper manifest)
        {
            EnsureArg.IsNotNull(manifest, nameof(manifest));

            Directory.CreateDirectory(_workingImageFolder);
            await File.WriteAllTextAsync(Path.Combine(_workingImageFolder, Constants.ManifestFileName), JsonConvert.SerializeObject(manifest));
        }

        public async Task<ArtifactBlob> ReadBaseLayerAsync()
        {
            var layers = await ReadOCIArtifactLayersAsync(_workingBaseLayerFolder);
            if (layers.Count() > 1)
            {
                throw new OverlayException(TemplateManagementErrorCode.BaseLayerLoadFailed, $"Base layer load failed. More than one layer in the base layer folder.");
            }

            return layers.Count == 0 ? new OCIFileLayer() : layers[0];
        }

        public async Task WriteBaseLayerAsync(ArtifactBlob baseLayer)
        {
            EnsureArg.IsNotNull(baseLayer, nameof(baseLayer));

            // Clear base layer folder before writing.
            ClearBaseLayerFolder();
            Directory.CreateDirectory(_workingBaseLayerFolder);
            await baseLayer.WriteToFileAsync(Path.Combine(_workingBaseLayerFolder, "layer1.tar.gz"));
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

        private async Task<List<ArtifactBlob>> ReadOCIArtifactLayersAsync(string folder)
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
                await artifactLayer.ReadFromFileAsync(tarGzFile);
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
