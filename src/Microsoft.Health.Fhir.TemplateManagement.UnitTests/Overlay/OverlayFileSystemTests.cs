// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Microsoft.Health.Fhir.TemplateManagement.Overlay;
using Xunit;

namespace Microsoft.Health.Fhir.TemplateManagement.UnitTests.Overlay
{
    public class OverlayFileSystemTests
    {
        [Fact]
        public void GivenAWorkingFolder_WhenReadOCIFiles_AnOCIFileLayerWillBeReturned()
        {
            string workingFolder = "TestData/DecompressedFiles";
            var overlayFs = new OverlayFileSystem(workingFolder);
            var oneLayer = overlayFs.ReadMergedOCIFileLayer();
            Assert.Equal(3, oneLayer.FileContent.Count());
        }

        [Fact]
        public void GivenAnOCIFileLayerAndAWorkingFolder_WhenWriteOCIFiles_OCIFilesWillBeWrittenToFolder()
        {
            string fileFolder = "TestData/DecompressedFiles";
            var testlayFs = new OverlayFileSystem(fileFolder);
            var testLayer = testlayFs.ReadMergedOCIFileLayer();

            string workingFolder = "TestData/workingFolder";
            Directory.CreateDirectory(workingFolder);
            ClearFolder(workingFolder);
            var overlayFs = new OverlayFileSystem(workingFolder);
            overlayFs.WriteMergedOCIFileLayer(testLayer);
            var filePaths = Directory.EnumerateFiles(workingFolder, "*.*", SearchOption.AllDirectories);
            Assert.Equal(3, filePaths.Count());
        }

        [Fact]
        public void GivenOCIArtifactLayers_WhenWriteImageFolder_AllOCIArtifactLayersWillBeWrittenToFolder()
        {
            string layerPath = "TestData/TarGzFiles/userV1.tar.gz";
            var layer = new OCIArtifactLayer() { SequenceNumber = 2, Content = File.ReadAllBytes(layerPath), FileName = "userV1.tar.gz" };
            string workingFolder = "TestData/testImageLayer";
            ClearFolder(workingFolder);
            var overlayFs = new OverlayFileSystem(workingFolder);
            overlayFs.WriteImageLayers(new List<OCIArtifactLayer>() { layer });
            var filePaths = Directory.EnumerateFiles(Path.Combine(workingFolder, ".image/layers"), "*.*", SearchOption.AllDirectories);
            Assert.Single(filePaths);
            ClearFolder(workingFolder);
        }

        private void ClearFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                return;
            }

            DirectoryInfo folder = new DirectoryInfo(path);
            folder.Delete(true);
        }
    }
}
