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
    public class OverlayOperatorTests
    {
        private readonly IOverlayOperator _overlayOperator;

        public OverlayOperatorTests()
        {
            _overlayOperator = new OverlayOperator();
        }

        [Fact]
        public void GivenAListOfOCIArtifactLayers_WhenExtractLayers_ListOfOCIFileLayersShouldBeReturned()
        {
            List<string> artifactFilePath = new List<string>() { "TestData/TarGzFiles/baseLayer.tar.gz", "TestData/TarGzFiles/userV1.tar.gz"};
            List<OCIArtifactLayer> inputLayers = new List<OCIArtifactLayer>();
            foreach (var oneFile in artifactFilePath)
            {
                var oneLayer = new OCIArtifactLayer() { Content = File.ReadAllBytes(oneFile) };
                inputLayers.Add(oneLayer);
            }

            var result = _overlayOperator.ExtractOCIFileLayers(inputLayers);
            Assert.Equal(inputLayers.Count(), result.Count());
        }

        [Fact]
        public void GivenAListOfOCIArtifactLayers_WhenSortLayers_ListOfsortedLayersShouldBeReturned()
        {
            List<string> artifactFilePath = new List<string>() { "TestData/TarGzFiles/testdecompress.tar.gz", "TestData/TarGzFiles/userV2.tar.gz", "TestData/TarGzFiles/userV1.tar.gz", "TestData/TarGzFiles/baseLayer.tar.gz" };
            List<OCIArtifactLayer> inputLayers = new List<OCIArtifactLayer>();
            foreach (var oneFile in artifactFilePath)
            {
                var oneLayer = new OCIArtifactLayer() { Content = File.ReadAllBytes(oneFile), FileName = Path.GetFileName(oneFile) };
                inputLayers.Add(oneLayer);
            }

            var fileLayers = _overlayOperator.ExtractOCIFileLayers(inputLayers);
            var sortedLayers = _overlayOperator.SortOCIFileLayersBySequenceNumber(fileLayers);
            //Assert.Equal("baseLayer.tar.gz", sortedLayers[0].FileName);
            //Assert.Equal("userV2.tar.gz", sortedLayers[1].FileName);
            //Assert.Equal("userV1.tar.gz", sortedLayers[2].FileName);
            //Assert.Equal("testdecompress.tar.gz", sortedLayers[3].FileName);
        }

        [Fact]
        public void GivenAListOfOCIFileLayers_WhenMergeLayers_AMergedOCIFileLayersShouldBeReturned()
        {
            // Generate List of OCIFileLayers
            List<string> artifactFilePath = new List<string>() { "TestData/TarGzFiles/baseLayer.tar.gz", "TestData/TarGzFiles/userV1.tar.gz" };
            List<OCIArtifactLayer> inputLayers = new List<OCIArtifactLayer>();
            foreach (var oneFile in artifactFilePath)
            {
                var oneLayer = new OCIArtifactLayer() { Content = File.ReadAllBytes(oneFile) };
                inputLayers.Add(oneLayer);
            }

            var fileLayers = _overlayOperator.ExtractOCIFileLayers(inputLayers);
            var sortedLayers = _overlayOperator.SortOCIFileLayersBySequenceNumber(fileLayers);

            // Generate Merged OCIFileLAyers
            var mergedLayer = _overlayOperator.MergeOCIFileLayers(sortedLayers);
            Assert.Equal(5, mergedLayer.FileContent.Count());
        }

        [Fact]
        public void GivenAOCIFileLayer_WhenGenerateDiffOCIFileLayer_IfBaseLayerFolderIsEmpty_ABaseOCIFileLayerShouldBeReturned()
        {
            var overlayFs = new OverlayFileSystem("TestData/UserFolder");
            var fileLayer = overlayFs.ReadMergedOCIFileLayer();
            var diffLayers = _overlayOperator.GenerateDiffLayer(fileLayer, null);
            Assert.Equal(6, diffLayers.FileContent.Count());
            Assert.Equal(1, diffLayers.SequenceNumber);
        }

        [Fact]
        public void GivenAOCIFileLayer_WhenGenerateDiffOCIFileLayerWithSnapshot_ADiffOCIFileLayerShouldBeReturned()
        {
            var overlayFs = new OverlayFileSystem("TestData/UserFolder");
            overlayFs.ClearImageLayerFolder();
            Directory.CreateDirectory("TestData/UserFolder/.image/base");
            File.Copy("TestData/Snapshot/baselayer.tar.gz", "TestData/UserFolder/.image/base/defaultlayer.tar.gz", true);

            var fileLayer = overlayFs.ReadMergedOCIFileLayer();
            var baseLayers = overlayFs.ReadBaseLayers();

            var fileLayers = _overlayOperator.ExtractOCIFileLayers(baseLayers);
            var sortedFileLayers = _overlayOperator.SortOCIFileLayersBySequenceNumber(fileLayers);
            var mergedFileLayer = _overlayOperator.MergeOCIFileLayers(sortedFileLayers);
            var diffLayers = _overlayOperator.GenerateDiffLayer(fileLayer, mergedFileLayer);

            File.Delete("TestData/UserFolder/.image/base/defaultlayer.tar.gz");
            Assert.Equal(814, diffLayers.FileContent.Count());
            Assert.Equal(2, diffLayers.SequenceNumber);
        }

        [Fact]
        public void GivenAOCIFileLayer_WhenPackLayer_AnOCIArtifactLayerShouldBeReturned()
        {
            var overlayFs = new OverlayFileSystem("TestData/UserFolder");
            string artifactPath = "TestData/TarGzFiles/baseLayer.tar.gz";
            OCIArtifactLayer inputLayer = new OCIArtifactLayer() { Content = File.ReadAllBytes(artifactPath) };
            var overlayOperator = new OverlayOperator();
            var fileLayer = overlayOperator.ExtractOCIFileLayer(inputLayer);
            var packedLayer = _overlayOperator.ArchiveOCIFileLayer(fileLayer);
            //Assert.Equal(inputLayer.Content.Length, packedLayer.Content.Length);
        }
    }
}
