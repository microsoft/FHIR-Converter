// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using EnsureThat;
using Microsoft.Health.Fhir.TemplateManagement.Models;

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

        public OCIFileLayer ReadMergedOCIFileLayer()
        {
            var filePaths = Directory.EnumerateFiles(WorkingFolder, "*.*", SearchOption.AllDirectories);
            Dictionary<string, byte[]> fileContent = new Dictionary<string, byte[]>() { };
            foreach (var oneFile in filePaths)
            {
                if (!Path.GetRelativePath(WorkingFolder, oneFile).Contains(Constants.HiddenImageFolder))
                {
                    fileContent.Add(Path.GetRelativePath(WorkingFolder, oneFile), File.ReadAllBytes(oneFile));
                }
            }

            OCIFileLayer fileLayer = new OCIFileLayer() { FileContent = fileContent };
            return fileLayer;
        }

        public void WriteMergedOCIFileLayer(OCIFileLayer oneLayer)
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

        public List<OCIArtifactLayer> ReadImageLayers()
        {
            return ReadOCIArtifactLayers(WorkingImageLayerFolder);
        }

        public void WriteImageLayers(List<OCIArtifactLayer> imageLayers)
        {
            EnsureArg.IsNotNull(imageLayers, nameof(imageLayers));

            ClearFolder(WorkingImageLayerFolder);
            foreach (var layer in imageLayers)
            {
                layer.WriteToFolder(WorkingImageLayerFolder);
            }
        }

        public List<OCIArtifactLayer> ReadBaseLayers()
        {
            return ReadOCIArtifactLayers(WorkingBaseLayerFolder);
        }

        public void WriteBaseLayers(List<OCIArtifactLayer> layers)
        {
            EnsureArg.IsNotNull(layers, nameof(layers));

            ClearFolder(WorkingBaseLayerFolder);
            for (var index = 1; index <= layers.Count; index++)
            {
                if (layers[index - 1].SequenceNumber == index)
                {
                    layers[index - 1].WriteToFolder(WorkingBaseLayerFolder);
                }
            }
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
                artifactLayer.ReadFromFolder(tarGzFile);
                if (artifactLayer.Content != null)
                {
                    result.Add(artifactLayer);
                }
            }

            return result;
        }
    }
}
