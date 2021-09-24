// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.ContainerRegistry.Models;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Microsoft.Health.Fhir.TemplateManagement.Overlay;
using Microsoft.Health.Fhir.TemplateManagement.Utilities;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Health.Fhir.TemplateManagement.UnitTests.Overlay
{
    public class OverlayOperatorTests
    {
        private readonly IOverlayOperator _overlayOperator;

        public OverlayOperatorTests()
        {
            _overlayOperator = new OverlayOperator();
        }

        public static IEnumerable<object[]> GetValidOciArtifact()
        {
            yield return new object[] { new ArtifactBlob() { FileName = "testdecompress.tar.gz", Content = File.ReadAllBytes("TestData/TarGzFiles/testdecompress.tar.gz"), Size = 100, Digest = "test1" }, 3 };
            yield return new object[] { new ArtifactBlob() { FileName = "layer1.tar.gz", Content = File.ReadAllBytes("TestData/TarGzFiles/layer1.tar.gz"), Size = 100, Digest = "test2" }, 840 };
            yield return new object[] { new ArtifactBlob() { FileName = "layer2.tar.gz", Content = File.ReadAllBytes("TestData/TarGzFiles/layer2.tar.gz"), Size = 100, Digest = "test3" }, 835 };
        }

        [Theory]
        [MemberData(nameof(GetValidOciArtifact))]
        public void GivenAnOciArtifactLayer_WhenExtractLayers_CorrectOCIFileLayerShouldBeReturned(ArtifactBlob inputLayer, int fileCounts)
        {
            var result = _overlayOperator.Extract(inputLayer);
            Assert.Equal(fileCounts, result.FileContent.Count);
            Assert.Equal(inputLayer.Content, result.Content);
            Assert.Equal(inputLayer.FileName, result.FileName);
            Assert.Equal(inputLayer.Size, result.Size);
            Assert.Equal(inputLayer.Digest, result.Digest);
        }

        [Fact]
        public void GivenInvalidContentOciArtifactLayer_WhenExtractLayers_ExceptionWillBeThrown()
        {
            string filePath = "TestData/TarGzFiles/invalid1.tar.gz";
            var oneLayer = new ArtifactBlob() { Content = File.ReadAllBytes(filePath) };

            Assert.Throws<ArtifactArchiveException>(() => _overlayOperator.Extract(oneLayer));
        }

        [Fact]
        public void GivenAListOfOciArtifactLayers_WhenExtractLayers_ListOfOciFileLayersShouldBeReturned()
        {
            List<string> artifactFilePath = new List<string>() { "TestData/TarGzFiles/baseLayer.tar.gz", "TestData/TarGzFiles/userV1.tar.gz" };
            List<ArtifactBlob> inputLayers = new List<ArtifactBlob>();
            foreach (var oneFile in artifactFilePath)
            {
                var oneLayer = new ArtifactBlob() { Content = File.ReadAllBytes(oneFile) };
                inputLayers.Add(oneLayer);
            }

            var result = _overlayOperator.Extract(inputLayers);
            Assert.Equal(inputLayers.Count(), result.Count());
        }

        [Fact]
        public async Task GivenAListOfOciArtifactLayers_WhenSortLayers_ASortedOciArtifactLayersShouldBeReturnedAsync()
        {
            string layer1 = "TestData/TarGzFiles/layer1.tar.gz";
            string layer2 = "TestData/TarGzFiles/layer2.tar.gz";
            string layer3 = "TestData/TarGzFiles/userV1.tar.gz";
            string manifest = "TestData/ExpectedManifest/testOrderManifest";
            string workingFolder = "TestData/testSortLayers";
            DirectoryHelper.ClearFolder(workingFolder);
            Directory.CreateDirectory(Path.Combine(workingFolder, ".image/layers"));

            // Rename files to rearrange the sequence.
            File.Copy(layer1, Path.Combine(workingFolder, ".image/layers/3.tar.gz"));
            File.Copy(layer2, Path.Combine(workingFolder, ".image/layers/2.tar.gz"));
            File.Copy(layer3, Path.Combine(workingFolder, ".image/layers/1.tar.gz"));
            var overlayFs = new OverlayFileSystem(workingFolder);
            var layers = await overlayFs.ReadImageLayersAsync();

            var sortedLayers = _overlayOperator.Sort(layers, JsonConvert.DeserializeObject<ManifestWrapper>(File.ReadAllText(manifest)));
            Assert.Equal("3.tar.gz", sortedLayers[0].FileName);
            Assert.Equal("2.tar.gz", sortedLayers[1].FileName);
            Assert.Equal("1.tar.gz", sortedLayers[2].FileName);
            DirectoryHelper.ClearFolder(workingFolder);
        }

        [Fact]
        public void GivenAListOfOciFileLayers_WhenMergeLayers_AMergedOciFileLayersShouldBeReturned()
        {
            // Generate List of OCIFileLayers
            List<string> artifactFilePath = new List<string>() { "TestData/TarGzFiles/layer1.tar.gz", "TestData/TarGzFiles/layer2.tar.gz" };
            List<ArtifactBlob> inputLayers = new List<ArtifactBlob>();
            foreach (var oneFile in artifactFilePath)
            {
                var oneLayer = new ArtifactBlob() { Content = File.ReadAllBytes(oneFile) };
                inputLayers.Add(oneLayer);
            }

            var fileLayers = _overlayOperator.Extract(inputLayers);

            // Generate Merged OCIFileLAyers
            var mergedLayer = _overlayOperator.Merge(fileLayers);
            Assert.Equal(6, mergedLayer.FileContent.Count());
        }

        [Fact]
        public async Task GivenAOciFileLayer_WhenGenerateDiffOciFileLayer_IfBaseLayerFolderIsEmptyOrNull_ABaseOciFileLayerShouldBeReturnedAsync()
        {
            var overlayFs = new OverlayFileSystem("TestData/UserFolder");
            var fileLayer = await overlayFs.ReadOciFileLayerAsync();
            var diffLayers = _overlayOperator.GenerateDiffLayer(fileLayer, null);
            Assert.Equal(6, diffLayers.FileContent.Count());
        }

        [Fact]
        public async Task GivenAOciFileLayer_WhenGenerateDiffOciFileLayerWithSnapshot_ADiffOciFileLayerShouldBeReturnedAsync()
        {
            var overlayFs = new OverlayFileSystem("TestData/UserFolder");
            overlayFs.ClearBaseLayerFolder();
            Directory.CreateDirectory("TestData/UserFolder/.image/base");
            File.Copy("TestData/TarGzFiles/layer1.tar.gz", "TestData/UserFolder/.image/base/layer1.tar.gz", true);
            var fileLayer = await overlayFs.ReadOciFileLayerAsync();
            var baseLayers = await overlayFs.ReadBaseLayerAsync();
            var baseOcifileLayer = _overlayOperator.Extract(baseLayers);
            var diffLayers = _overlayOperator.GenerateDiffLayer(fileLayer, baseOcifileLayer);

            overlayFs.ClearBaseLayerFolder();
            Assert.Equal(839, diffLayers.FileContent.Count());
        }

        [Fact]
        public void GivenAOciFileLayer_WhenPackLayer_AnOciArtifactLayerShouldBeReturned()
        {
            string artifactPath = "TestData/TarGzFiles/layer1.tar.gz";
            ArtifactBlob inputLayer = new ArtifactBlob() { Content = File.ReadAllBytes(artifactPath) };
            var overlayOperator = new OverlayOperator();
            var fileLayer = overlayOperator.Extract(inputLayer);
            var diffLayers = _overlayOperator.GenerateDiffLayer(fileLayer, null);
            var packedLayer = _overlayOperator.Archive(diffLayers);
            Assert.True(inputLayer.Content.Count() == packedLayer.Content.Count());
        }

        [Fact]
        public void GivenOciFileLayers_WhenPackLayers_OciArtifactLayersShouldBeReturned()
        {
            List<string> artifactPaths = new List<string> { "TestData/TarGzFiles/baseLayer.tar.gz" };
            var inputLayers = new List<ArtifactBlob>();
            foreach (var path in artifactPaths)
            {
                inputLayers.Add(new ArtifactBlob() { Content = File.ReadAllBytes(path) });
            }

            var overlayOperator = new OverlayOperator();
            var fileLayers = overlayOperator.Extract(inputLayers);
            var diffLayers = new List<OciFileLayer>();
            foreach (var layer in fileLayers)
            {
                diffLayers.Add(_overlayOperator.GenerateDiffLayer(layer, null));
            }

            var packedLayers = _overlayOperator.Archive(diffLayers);
            Assert.Equal(inputLayers.Count, packedLayers.Count);
        }
    }
}
