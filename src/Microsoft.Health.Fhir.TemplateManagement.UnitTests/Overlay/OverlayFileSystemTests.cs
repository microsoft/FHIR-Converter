// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        public async Task GivenAWorkingFolder_WhenReadOCIFiles_AnOCIFileLayerWillBeReturnedAsync()
        {
            string workingFolder = "TestData/DecompressedFiles";
            var overlayFs = new OverlayFileSystem(workingFolder);
            var oneLayer = await overlayFs.ReadOCIFileLayerAsync();
            Assert.Equal(3, oneLayer.FileContent.Count());
        }

        [Fact]
        public async Task GivenAnOCIFileLayerAndAWorkingFolder_WhenWriteOCIFiles_OCIFilesWillBeWrittenToFolderAsync()
        {
            string fileFolder = "TestData/DecompressedFiles";
            var testlayFs = new OverlayFileSystem(fileFolder);
            var testLayer = await testlayFs.ReadOCIFileLayerAsync();

            string workingFolder = "TestData/workingFolder";
            Directory.CreateDirectory(workingFolder);
            ClearFolder(workingFolder);
            var overlayFs = new OverlayFileSystem(workingFolder);
            await overlayFs.WriteOCIFileLayerAsync(testLayer);
            var filePaths = Directory.EnumerateFiles(workingFolder, "*.*", SearchOption.AllDirectories);
            Assert.Equal(3, filePaths.Count());
        }

        [Fact]
        public async Task GivenOCIArtifactLayers_WhenWriteImageFolder_AllOCIArtifactLayersWillBeWrittenToFolderAsync()
        {
            string layerPath = "TestData/TarGzFiles/userV1.tar.gz";
            var layer1 = new ArtifactBlob() { SequenceNumber = 1, Content = File.ReadAllBytes(layerPath), FileName = "userV1.tar.gz" };
            var layer2 = new ArtifactBlob() { SequenceNumber = 2, Content = File.ReadAllBytes(layerPath), FileName = "userV2.tar.gz" };
            string workingFolder = "TestData/testImageLayer";
            ClearFolder(workingFolder);
            var overlayFs = new OverlayFileSystem(workingFolder);
            await overlayFs.WriteImageLayersAsync(new List<ArtifactBlob>() { layer1, layer2 });
            var filePaths = Directory.EnumerateFiles(Path.Combine(workingFolder, ".image/layers"), "*.*", SearchOption.AllDirectories);
            Assert.Equal(2, filePaths.Count());
            ClearFolder(workingFolder);
        }

        [Fact]
        public async Task GivenInputImageFolder_WhenReadImageFolder_AllOCIArtifactLayersWillBeReturnedAsync()
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
            var layers = await overlayFs.ReadImageLayersAsync();
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
            Assert.Equal(StreamUtility.CalculateDigestFromSha256(File.OpenRead(layerPath)), overlayFs.ReadBaseLayerAsync().Result.Digest);
            ClearFolder(workingFolder);
        }

        [Fact]
        public async Task GivenInputWorkingFolder_WhenReadBaseLayer_IfTwoFilesExist_ExceptionWillBeThrown()
        {
            string layerPath = "TestData/TarGzFiles/userV1.tar.gz";
            string workingFolder = "TestData/testInValidBaseLayer";
            ClearFolder(workingFolder);
            Directory.CreateDirectory(Path.Combine(workingFolder, ".image/base"));
            File.Copy(layerPath, Path.Combine(workingFolder, ".image/base/layer1.tar.gz"));
            File.Copy(layerPath, Path.Combine(workingFolder, ".image/base/layer2.tar.gz"));
            var overlayFs = new OverlayFileSystem(workingFolder);
            await Assert.ThrowsAsync<OverlayException>(() => overlayFs.ReadBaseLayerAsync());
            ClearFolder(workingFolder);
        }

        [Fact]
        public void GivenInputWorkingFolder_WhenReadBaseLayer_IfNoBaseExist_EmptyBaseLayerWillBeReturn()
        {
            string workingFolder = "TestData/testEmptyBaseLayer";
            Directory.CreateDirectory(Path.Combine(workingFolder));
            var overlayFs = new OverlayFileSystem(workingFolder);
            Assert.Null(overlayFs.ReadBaseLayerAsync().Result.Digest);
            Directory.CreateDirectory(Path.Combine(workingFolder, ".image/base"));
            Assert.Null(overlayFs.ReadBaseLayerAsync().Result.Digest);
            ClearFolder(workingFolder);
        }

        [Fact]
        public async Task GivenInputWorkingFolder_WhenWriteBaseLayer_OneBaseLayerWillBeWrittenAsync()
        {
            string layerPath = "TestData/TarGzFiles/userV1.tar.gz";
            string workingFolder = "TestData/testWriteBaseLayer";
            Directory.CreateDirectory(Path.Combine(workingFolder));
            var overlayFs = new OverlayFileSystem(workingFolder);
            await overlayFs.WriteBaseLayerAsync(new ArtifactBlob() { Content = File.ReadAllBytes(layerPath) });
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
