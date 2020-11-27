// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
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
            _workingFolder = workingFolder;
            _workingImageLayerFolder = Path.Combine(workingFolder, Constants.HiddenLayersFolder);
            _workingBaseLayerFolder = Path.Combine(workingFolder, Constants.HiddenBaseLayerFolder);
        }

        public OCIFileLayer ReadMergedOCIFileLayer()
        {
            var filePaths = Directory.EnumerateFiles(_workingFolder, "*.*", SearchOption.AllDirectories);
            Dictionary<string, byte[]> fileContent = new Dictionary<string, byte[]>() { };
            foreach (var oneFile in filePaths)
            {
                if (!Path.GetRelativePath(_workingFolder, oneFile).Contains(Constants.HiddenImageFolder))
                {
                    fileContent.Add(Path.GetRelativePath(_workingFolder, oneFile), File.ReadAllBytes(oneFile));
                }
            }

            OCIFileLayer fileLayer = new OCIFileLayer() { FileContent = fileContent };
            return fileLayer;
        }

        public void WriteMergedOCIFileLayer(OCIFileLayer oneLayer)
        {
            if (!Directory.Exists(_workingFolder))
            {
                Directory.CreateDirectory(_workingFolder);
            }

            foreach (var oneFile in oneLayer.FileContent)
            {
                var directory = Path.GetDirectoryName(Path.Combine(_workingFolder, oneFile.Key));
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllBytes(Path.Combine(_workingFolder, oneFile.Key), oneFile.Value);
            }
        }

        public List<OCIArtifactLayer> ReadImageLayers()
        {
            return ReadOCIArtifactLayers(_workingImageLayerFolder);
        }

        public void WriteImageLayers(List<OCIArtifactLayer> imageLayers)
        {
            ClearFolder(_workingImageLayerFolder);
            foreach (var layer in imageLayers)
            {
                OCIArtifactLayer.WriteOCIArtifactLayer(layer, _workingImageLayerFolder);
            }
        }

        public List<OCIArtifactLayer> ReadBaseLayers()
        {
            return ReadOCIArtifactLayers(_workingBaseLayerFolder);
        }

        public void WriteBaseLayers(List<OCIArtifactLayer> layers)
        {
            ClearFolder(_workingBaseLayerFolder);
            foreach (var layer in layers)
            {
                OCIArtifactLayer.WriteOCIArtifactLayer(layer, _workingBaseLayerFolder);
            }
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
            if (!Directory.Exists(directory))
            {
                return;
            }

            DirectoryInfo folder = new DirectoryInfo(directory);
            folder.Delete(true);
        }

        private List<OCIArtifactLayer> ReadOCIArtifactLayers(string folder)
        {
            if (!Directory.Exists(folder))
            {
                return null;
            }

            var result = new List<OCIArtifactLayer>();
            var layersPath = Directory.EnumerateFiles(folder, "*.tar.gz", SearchOption.AllDirectories);
            foreach (var tarGzFile in layersPath)
            {
                var artifactLayer = OCIArtifactLayer.ReadOCIArtifactLayer(tarGzFile);
                if (artifactLayer != null)
                {
                    result.Add(artifactLayer);
                }
            }

            return result;
        }
    }
}
