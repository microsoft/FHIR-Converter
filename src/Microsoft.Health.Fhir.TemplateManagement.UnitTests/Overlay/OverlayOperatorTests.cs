// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Microsoft.Health.Fhir.TemplateManagement.Overlay;
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

        public static IEnumerable<object[]> GetValidOCIArtifact()
        {
            yield return new object[] { new OCIArtifactLayer() { FileName = "testdecompress.tar.gz", Content = File.ReadAllBytes("TestData/TarGzFiles/testdecompress.tar.gz"), Size = 100, Digest = "test1" }, 3, -1 };
            yield return new object[] { new OCIArtifactLayer() { FileName = "layer1.tar.gz", Content = File.ReadAllBytes("TestData/TarGzFiles/layer1.tar.gz"), Size = 100, Digest = "test2" }, 840, 1 };
            yield return new object[] { new OCIArtifactLayer() { FileName = "layer2.tar.gz", Content = File.ReadAllBytes("TestData/TarGzFiles/layer2.tar.gz"), Size = 100, Digest = "test3" }, 835, 2 };
        }

        [Theory]
        [MemberData(nameof(GetValidOCIArtifact))]
        public void GivenAnOCIArtifactLayer_WhenExtractLayers_CorrectOCIFileLayerShouldBeReturned(OCIArtifactLayer inputLayer, int fileCounts, int sequenceNumber)
        {
            var result = _overlayOperator.Extract(inputLayer);
            Assert.Equal(fileCounts, result.FileContent.Count);
            Assert.Equal(sequenceNumber, result.SequenceNumber);
            Assert.Equal(inputLayer.Content, result.Content);
            Assert.Equal(inputLayer.FileName, result.FileName);
            Assert.Equal(inputLayer.Size, result.Size);
            Assert.Equal(inputLayer.Digest, result.Digest);
        }

        [Fact]
        public void GivenInvalidContentOCIArtifactLayer_WhenExtractLayers_ExceptionWillBeThrown()
        {
            string filePath = "TestData/TarGzFiles/invalid1.tar.gz";
            var oneLayer = new OCIArtifactLayer() { Content = File.ReadAllBytes(filePath) };

            Assert.Throws<ArtifactDecompressException>(() => _overlayOperator.Extract(oneLayer));
        }

        [Fact]
        public void GivenOCIArtifactLayer_IfSequenceNumberNotFound_WhenExtractLayers_OCIFileLayerWithDefaultSequenceWillBeReturn()
        {
            string filePath = "TestData/TarGzFiles/testdecompress.tar.gz";
            var oneLayer = new OCIArtifactLayer() { Content = File.ReadAllBytes(filePath) };

            var result = _overlayOperator.Extract(oneLayer);
            Assert.Equal(-1, result.SequenceNumber);
        }

        [Fact]
        public void GivenAListOfOCIArtifactLayers_WhenExtractLayers_ListOfOCIFileLayersShouldBeReturned()
        {
            List<string> artifactFilePath = new List<string>() { "TestData/TarGzFiles/baseLayer.tar.gz", "TestData/TarGzFiles/userV1.tar.gz" };
            List<OCIArtifactLayer> inputLayers = new List<OCIArtifactLayer>();
            foreach (var oneFile in artifactFilePath)
            {
                var oneLayer = new OCIArtifactLayer() { Content = File.ReadAllBytes(oneFile) };
                inputLayers.Add(oneLayer);
            }

            var result = _overlayOperator.Extract(inputLayers);
            Assert.Equal(inputLayers.Count(), result.Count());
        }

        [Fact]
        public void GivenAListOfOCIFileLayers_WhenMergeLayers_AMergedOCIFileLayersShouldBeReturned()
        {
            // Generate List of OCIFileLayers
            List<string> artifactFilePath = new List<string>() { "TestData/TarGzFiles/layer1.tar.gz", "TestData/TarGzFiles/layer2.tar.gz" };
            List<OCIArtifactLayer> inputLayers = new List<OCIArtifactLayer>();
            foreach (var oneFile in artifactFilePath)
            {
                var oneLayer = new OCIArtifactLayer() { Content = File.ReadAllBytes(oneFile) };
                inputLayers.Add(oneLayer);
            }

            var fileLayers = _overlayOperator.Extract(inputLayers);

            // Generate Merged OCIFileLAyers
            var mergedLayer = _overlayOperator.Merge(fileLayers);
            Assert.Equal(6, mergedLayer.FileContent.Count());
        }

        [Fact]
        public void GivenAOCIFileLayer_WhenGenerateDiffOCIFileLayer_IfBaseLayerFolderIsEmptyOrNull_ABaseOCIFileLayerShouldBeReturned()
        {
            var overlayFs = new OverlayFileSystem("TestData/UserFolder");
            var fileLayer = overlayFs.ReadOCIFileLayer();
            var diffLayers = _overlayOperator.GenerateDiffLayer(fileLayer, null);
            Assert.Equal(6, diffLayers.FileContent.Count());
            Assert.Equal(1, diffLayers.SequenceNumber);
        }

        [Fact]
        public void GivenAOCIFileLayer_WhenGenerateDiffOCIFileLayerWithSnapshot_ADiffOCIFileLayerShouldBeReturned()
        {
            var overlayFs = new OverlayFileSystem("TestData/UserFolder");
            overlayFs.ClearBaseLayerFolder();
            Directory.CreateDirectory("TestData/UserFolder/.image/base");
            File.Copy("TestData/TarGzFiles/layer1.tar.gz", "TestData/UserFolder/.image/base/layer1.tar.gz", true);
            var fileLayer = overlayFs.ReadOCIFileLayer();
            var baseLayers = overlayFs.ReadBaseLayer();
            var baseOCIfileLayer = _overlayOperator.Extract(baseLayers);
            var diffLayers = _overlayOperator.GenerateDiffLayer(fileLayer, baseOCIfileLayer);

            overlayFs.ClearBaseLayerFolder();
            Assert.Equal(839, diffLayers.FileContent.Count());
            Assert.Equal(2, diffLayers.SequenceNumber);
        }

        [Fact]
        public void GivenAOCIFileLayer_WhenPackLayer_AnOCIArtifactLayerShouldBeReturned()
        {
            string artifactPath = "TestData/TarGzFiles/layer1.tar.gz";
            OCIArtifactLayer inputLayer = new OCIArtifactLayer() { Content = File.ReadAllBytes(artifactPath) };
            var overlayOperator = new OverlayOperator();
            var fileLayer = overlayOperator.Extract(inputLayer);
            var diffLayers = _overlayOperator.GenerateDiffLayer(fileLayer, null);
            var packedLayer = _overlayOperator.Archive(diffLayers);
            Assert.True(inputLayer.Content.Count() == packedLayer.Content.Count());
        }

        [Fact]
        public void GivenOCIFileLayers_WhenPackLayers_OCIArtifactLayersShouldBeReturned()
        {
            List<string> artifactPaths = new List<string> { "TestData/TarGzFiles/baseLayer.tar.gz" };
            var inputLayers = new List<OCIArtifactLayer>();
            foreach (var path in artifactPaths)
            {
                inputLayers.Add(new OCIArtifactLayer() { Content = File.ReadAllBytes(path) });
            }

            var overlayOperator = new OverlayOperator();
            var fileLayers = overlayOperator.Extract(inputLayers);
            var diffLayers = new List<OCIFileLayer>();
            foreach (var layer in fileLayers)
            {
                diffLayers.Add(_overlayOperator.GenerateDiffLayer(layer, null));
            }

            var packedLayers = _overlayOperator.Archive(diffLayers);
            Assert.Equal(inputLayers.Count, packedLayers.Count);
        }
    }
}
