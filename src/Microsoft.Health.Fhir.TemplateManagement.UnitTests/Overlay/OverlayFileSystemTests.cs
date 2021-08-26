﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Microsoft.Health.Fhir.TemplateManagement.Overlay;
using Microsoft.Health.Fhir.TemplateManagement.Utilities;
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
            var oneLayer = overlayFs.ReadOCIFileLayer();
            Assert.Equal(3, oneLayer.FileContent.Count());
        }

        [Fact]
        public void GivenAnOCIFileLayerAndAWorkingFolder_WhenWriteOCIFiles_OCIFilesWillBeWrittenToFolder()
        {
            string fileFolder = "TestData/DecompressedFiles";
            var testlayFs = new OverlayFileSystem(fileFolder);
            var testLayer = testlayFs.ReadOCIFileLayer();

            string workingFolder = "TestData/workingFolder";
            Directory.CreateDirectory(workingFolder);
            ClearFolder(workingFolder);
            var overlayFs = new OverlayFileSystem(workingFolder);
            overlayFs.WriteOCIFileLayer(testLayer);
            var filePaths = Directory.EnumerateFiles(workingFolder, "*.*", SearchOption.AllDirectories);
            Assert.Equal(3, filePaths.Count());
        }

        [Fact]
        public void GivenOCIArtifactLayers_WhenWriteImageFolder_AllOCIArtifactLayersWillBeWrittenToFolder()
        {
            string layerPath = "TestData/TarGzFiles/userV1.tar.gz";
            var layer1 = new ArtifactBlob() { SequenceNumber = 1, Content = File.ReadAllBytes(layerPath), FileName = "userV1.tar.gz" };
            var layer2 = new ArtifactBlob() { SequenceNumber = 2, Content = File.ReadAllBytes(layerPath), FileName = "userV2.tar.gz" };
            string workingFolder = "TestData/testImageLayer";
            ClearFolder(workingFolder);
            var overlayFs = new OverlayFileSystem(workingFolder);
            overlayFs.WriteImageLayers(new List<ArtifactBlob>() { layer1, layer2 });
            var filePaths = Directory.EnumerateFiles(Path.Combine(workingFolder, ".image/layers"), "*.*", SearchOption.AllDirectories);
            Assert.Equal(2, filePaths.Count());
            ClearFolder(workingFolder);
        }

        [Fact]
        public void GivenInputImageFolder_WhenReadImageFolder_AllOCIArtifactLayersWillBeReturned()
        {
            string layer1 = "TestData/TarGzFiles/layer1.tar.gz";
            string layer2 = "TestData/TarGzFiles/layer2.tar.gz";
            string layer3 = "TestData/TarGzFiles/userV1.tar.gz";
            string workingFolder = "TestData/testReadImageLayer";
            ClearFolder(workingFolder);
            Directory.CreateDirectory(Path.Combine(workingFolder, ".image/layers"));

            File.Copy(layer1, Path.Combine(workingFolder, ".image/layers/3.tar.gz"));
            File.Copy(layer2, Path.Combine(workingFolder, ".image/layers/2.tar.gz"));
            File.Copy(layer3, Path.Combine(workingFolder, ".image/layers/1.tar.gz"));
            var overlayFs = new OverlayFileSystem(workingFolder);
            var layers = overlayFs.ReadImageLayers();
            Assert.Equal(3, layers.Count);
            ClearFolder(workingFolder);
        }

        [Fact]
        public void GivenInputWorkingFolder_WhenReadBaseLayer_IfOneFileExist_BaseLayerWillBeReturn()
        {
            string layerPath = "TestData/TarGzFiles/userV1.tar.gz";
            string workingFolder = "TestData/testValidBaseLayer";
            ClearFolder(workingFolder);
            Directory.CreateDirectory(Path.Combine(workingFolder, ".image/base"));
            File.Copy(layerPath, Path.Combine(workingFolder, ".image/base/layer1.tar.gz"));
            var overlayFs = new OverlayFileSystem(workingFolder);
            Assert.Equal(StreamUtility.CalculateDigestFromSha256(File.OpenRead(layerPath)), overlayFs.ReadBaseLayer().Digest);
            ClearFolder(workingFolder);
        }

        [Fact]
        public void GivenInputWorkingFolder_WhenReadBaseLayer_IfTwoFilesExist_ExceptionWillBeThrown()
        {
            string layerPath = "TestData/TarGzFiles/userV1.tar.gz";
            string workingFolder = "TestData/testInValidBaseLayer";
            ClearFolder(workingFolder);
            Directory.CreateDirectory(Path.Combine(workingFolder, ".image/base"));
            File.Copy(layerPath, Path.Combine(workingFolder, ".image/base/layer1.tar.gz"));
            File.Copy(layerPath, Path.Combine(workingFolder, ".image/base/layer2.tar.gz"));
            var overlayFs = new OverlayFileSystem(workingFolder);
            Assert.Throws<OverlayException>(() => overlayFs.ReadBaseLayer());
            ClearFolder(workingFolder);
        }

        [Fact]
        public void GivenInputWorkingFolder_WhenReadBaseLayer_IfNoBaseExist_EmptyBaseLayerWillBeReturn()
        {
            string workingFolder = "TestData/testEmptyBaseLayer";
            Directory.CreateDirectory(Path.Combine(workingFolder));
            var overlayFs = new OverlayFileSystem(workingFolder);
            Assert.Null(overlayFs.ReadBaseLayer().Digest);
            Directory.CreateDirectory(Path.Combine(workingFolder, ".image/base"));
            Assert.Null(overlayFs.ReadBaseLayer().Digest);
            ClearFolder(workingFolder);
        }

        [Fact]
        public void GivenInputWorkingFolder_WhenWriteBaseLayer_OneBaseLayerWillBeWritten()
        {
            string layerPath = "TestData/TarGzFiles/userV1.tar.gz";
            string workingFolder = "TestData/testWriteBaseLayer";
            Directory.CreateDirectory(Path.Combine(workingFolder));
            var overlayFs = new OverlayFileSystem(workingFolder);
            overlayFs.WriteBaseLayer(new ArtifactBlob() { Content = File.ReadAllBytes(layerPath) });
            Assert.Single(Directory.EnumerateFiles(workingFolder + "/.image/base", "*.*", SearchOption.AllDirectories));
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
