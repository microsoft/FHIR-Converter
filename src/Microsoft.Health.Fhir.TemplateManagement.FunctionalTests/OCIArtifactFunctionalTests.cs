// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Utilities;
using Xunit;

namespace Microsoft.Health.Fhir.TemplateManagement.FunctionalTests
{
    /// <summary>
    /// Test Cases:
    /// Pull one layer image with valid sequence number, successfully pulled with base layer copied.
    /// Pull one layer image without sequence number, successfully pulled without base layer copied.
    /// Pull one layer image with invalid sequence number, exception will be thrown.
    /// Pull multi-layers with valid sequence numbers, successfully pulled with base layer copied.
    /// Pull multi-layers with invalid sequence numbers, exception will be thrown.
    /// Pull invalid image, exception will be thrown.
    /// Push artifacts which unpacked from base layer. If user modify artifacts, successfully pushed multi-layers image.
    /// Push artifacts which unpacked from base layer. If user don't modify artifacts, successfully pushed one-layer image.
    /// Push artifacts without base layer, successfully pushed one-layer image.
    /// Push artifacts and ignore base layer, successfully pushed one-layer image.
    /// Push empty artifact folder, exception will be thrown.
    /// </summary>
    public class OCIArtifactFunctionalTests
    {
        private readonly string _containerRegistryServer;
        private readonly string _baseLayerTemplatePath = "TestData/TarGzFiles/layer1.tar.gz";
        private readonly string _userLayerTemplatePath = "TestData/TarGzFiles/layer2.tar.gz";
        private readonly string _emptySequenceNumberLayerPath = "TestData/TarGzFiles/userV1.tar.gz";
        private readonly string _invalidCompressedImageLayerPath = "TestData/TarGzFiles/invalid1.tar.gz";
        private readonly string _testOneLayerWithValidSequenceNumberImageReference;
        private readonly string _testOneLayerWithoutSequenceNumberImageReference;
        private readonly string _testOneLayerWithInValidSequenceNumberImageReference;
        private readonly string _testMultiLayersWithValidSequenceNumberImageReference;
        private readonly string _testMultiLayersWithInValidSequenceNumberImageReference;
        private readonly string _testInvalidCompressedImageReference;
        private bool _isOrasValid = true;
        private string _orasErrorMessage;
        private readonly string _orasFileName;

        public OCIArtifactFunctionalTests()
        {
            _orasFileName = "oras.exe";
            _containerRegistryServer = "localhost:5000";
            _testOneLayerWithValidSequenceNumberImageReference = _containerRegistryServer + "/templatetest:onelayer_valid_sequence";
            _testOneLayerWithoutSequenceNumberImageReference = _containerRegistryServer + "/templatetest:onelayer_without_sequence";
            _testOneLayerWithInValidSequenceNumberImageReference = _containerRegistryServer + "/templatetest:onelayer_invalid_sequence";
            _testMultiLayersWithValidSequenceNumberImageReference = _containerRegistryServer + "/templatetest:multilayers_valid_sequence";
            _testMultiLayersWithInValidSequenceNumberImageReference = _containerRegistryServer + "/templatetest:multilayers_invalid_sequence";
            _testInvalidCompressedImageReference = _containerRegistryServer + "/templatetest:invalid_image";
            PushOneLayerWithValidSequenceNumber();
            PushOneLayerWithoutSequenceNumber();
            PushOneLayerWithInvalidSequenceNumber();
            PushMultiLayerWithValidSequenceNumber();
            PushMultiLayerWithInValidSequenceNumber();
            PushInvalidCompressedImage();
        }

        private void PushOneLayerWithValidSequenceNumber()
        {
            string command = $"push {_testOneLayerWithValidSequenceNumberImageReference} {_baseLayerTemplatePath}";
            ExecuteOrasCommand(command);
        }

        private void PushOneLayerWithoutSequenceNumber()
        {
            string command = $"push {_testOneLayerWithoutSequenceNumberImageReference} {_emptySequenceNumberLayerPath}";
            ExecuteOrasCommand(command);
        }

        private void PushOneLayerWithInvalidSequenceNumber()
        {
            string command = $"push {_testOneLayerWithInValidSequenceNumberImageReference} {_userLayerTemplatePath}";
            ExecuteOrasCommand(command);
        }

        private void PushMultiLayerWithValidSequenceNumber()
        {
            string command = $"push {_testMultiLayersWithValidSequenceNumberImageReference} {_baseLayerTemplatePath} {_userLayerTemplatePath}";
            ExecuteOrasCommand(command);
        }

        private void PushMultiLayerWithInValidSequenceNumber()
        {
            string command = $"push {_testMultiLayersWithInValidSequenceNumberImageReference} {_emptySequenceNumberLayerPath} {_userLayerTemplatePath}";
            ExecuteOrasCommand(command);
        }

        private void PushInvalidCompressedImage()
        {
            string command = $"push {_testInvalidCompressedImageReference} {_invalidCompressedImageLayerPath}";
            ExecuteOrasCommand(command);
        }

        private void ExecuteOrasCommand(string command)
        {
            _orasErrorMessage = OrasUtility.OrasExecution(command, _orasFileName);
            if (!string.IsNullOrEmpty(_orasErrorMessage))
            {
                _isOrasValid = false;
            }
        }

        [Fact]
        public async Task GivenOneLayerImageWithValidSequenceNumber_WhenPulled_ArtifactsWillBePulledWithBaseLayerCopiedAsync()
        {
            Assert.True(_isOrasValid, _orasErrorMessage);
            string imageReference = _testOneLayerWithValidSequenceNumberImageReference;
            string outputFolder = "TestData/testOneLayerWithValidSequenceNumber";
            var testManager = new OCIFileManager(imageReference, outputFolder);
            await testManager.PullOCIImageAsync();
            testManager.UnpackOCIImage();
            Assert.Equal(842, Directory.EnumerateFiles(outputFolder, "*.*", SearchOption.AllDirectories).Count());
            Assert.Single(Directory.EnumerateFiles(Path.Combine(outputFolder, ".image", "base"), "*.tar.gz", SearchOption.AllDirectories));
            ClearFolder(outputFolder);
        }

        [Fact]
        public async Task GivenOneLayerImageWithoutSequenceNumber_WhenPulled_ArtifactsWillBePulledWithoutBaseLayerCopiedAsync()
        {
            Assert.True(_isOrasValid, _orasErrorMessage);
            string imageReference = _testOneLayerWithoutSequenceNumberImageReference;
            string outputFolder = "TestData/testOneLayerWithoutSequenceNumber";
            var testManager = new OCIFileManager(imageReference, outputFolder);
            await testManager.PullOCIImageAsync();
            testManager.UnpackOCIImage();
            Assert.Equal(2, Directory.EnumerateFiles(outputFolder, "*.*", SearchOption.AllDirectories).Count());
            Assert.False(Directory.Exists(Path.Combine(outputFolder, ".image", "base")));
            ClearFolder(outputFolder);
        }

        [Fact]
        public async Task GivenOneLayerImageWithInvalidSequenceNumber_WhenPulled_ExceptionWillBeThrownAsync()
        {
            Assert.True(_isOrasValid, _orasErrorMessage);
            string imageReference = _testOneLayerWithInValidSequenceNumberImageReference;
            string outputFolder = "TestData/testOneLayerWithInValidSequenceNumber";
            var testManager = new OCIFileManager(imageReference, outputFolder);
            await testManager.PullOCIImageAsync();
            Assert.Throws<OverlayException>(() => testManager.UnpackOCIImage());
            ClearFolder(outputFolder);
        }

        [Fact]
        public async Task GivenMultiLayerImageWithValidSequenceNumber_WhenPulled_ArtifactsWillBePulledWithBaseLayerCopiedAsync()
        {
            Assert.True(_isOrasValid, _orasErrorMessage);
            string imageReference = _testMultiLayersWithValidSequenceNumberImageReference;
            string outputFolder = "TestData/testMultiLayersWithValidSequenceNumber";
            var testManager = new OCIFileManager(imageReference, outputFolder);
            await testManager.PullOCIImageAsync();
            testManager.UnpackOCIImage();
            Assert.Equal(9, Directory.EnumerateFiles(outputFolder, "*.*", SearchOption.AllDirectories).Count());
            Assert.Single(Directory.EnumerateFiles(Path.Combine(outputFolder, ".image", "base"), "*.tar.gz", SearchOption.AllDirectories));
            ClearFolder(outputFolder);
        }

        [Fact]
        public async Task GivenMultiLayersImageWithInvalidSequenceNumber_WhenPulled_ExceptionWillBeThrownAsync()
        {
            Assert.True(_isOrasValid, _orasErrorMessage);
            string imageReference = _testMultiLayersWithInValidSequenceNumberImageReference;
            string outputFolder = "TestData/testMultiLayersWithInValidSequenceNumber";
            var testManager = new OCIFileManager(imageReference, outputFolder);
            await testManager.PullOCIImageAsync();
            Assert.Throws<OverlayException>(() => testManager.UnpackOCIImage());
            ClearFolder(outputFolder);
        }

        [Fact]
        public async Task GivenInvalidCompressedImage_WhenPulled_ExceptionWillBeThrownAsync()
        {
            Assert.True(_isOrasValid, _orasErrorMessage);
            string imageReference = _testInvalidCompressedImageReference;
            string outputFolder = "TestData/testInvalidCompressedImage";
            var testManager = new OCIFileManager(imageReference, outputFolder);
            await testManager.PullOCIImageAsync();
            Assert.Throws<ArtifactDecompressException>(() => testManager.UnpackOCIImage());
            ClearFolder(outputFolder);
        }

        [Fact]
        public async Task GivenAnInputFolderUnpackedFromBaseLayer_WhenPushOCIFiles_IfUserModify_TwoLayersWillBePushedAsync()
        {
            Assert.True(_isOrasValid, _orasErrorMessage);

            // Pull an image
            string initImageReference = _testOneLayerWithValidSequenceNumberImageReference;
            string initInputFolder = "TestData/UserFolder1";
            var testManager = new OCIFileManager(initImageReference, initInputFolder);
            await testManager.PullOCIImageAsync();
            testManager.UnpackOCIImage();

            // Modified by user
            File.WriteAllText(Path.Combine(initInputFolder, "add"), "add");
            File.WriteAllText(Path.Combine(initInputFolder, "metadata.json"), "modify");
            File.Delete(Path.Combine(initInputFolder, "ADT_A01.liquid"));

            // Push new image.
            string testPushMultiLayersImageReference = _containerRegistryServer + "/templatetest:push_multilayers";
            var pushManager = new OCIFileManager(testPushMultiLayersImageReference, initInputFolder);
            pushManager.PackOCIImage();
            await pushManager.PushOCIImageAsync();

            // Check Image
            string command = $"pull {testPushMultiLayersImageReference} -o checkMultiLayersFolder";
            OrasUtility.OrasExecution(command, _orasFileName);
            Assert.Equal(2, Directory.EnumerateFiles("checkMultiLayersFolder", "*.tar.gz", SearchOption.AllDirectories).Count());
            Assert.Equal(4, StreamUtility.DecompressTarGzStream(File.OpenRead(Path.Combine("checkMultiLayersFolder", "layer2.tar.gz"))).Count());
            ClearFolder(initInputFolder);
            ClearFolder("checkMultiLayersFolder");
        }

        [Fact]
        public async Task GivenAnInputFolderUnpackedFromBaseLayer_WhenPushOCIFiles_IfIgnoreBaseLayer_NewBaseLayerWillBePushedAsync()
        {
            Assert.True(_isOrasValid, _orasErrorMessage);

            // Pull an image
            string initImageReference = _testOneLayerWithValidSequenceNumberImageReference;
            string initInputFolder = "TestData/UserFolder2";
            var testManager = new OCIFileManager(initImageReference, initInputFolder);
            await testManager.PullOCIImageAsync();
            testManager.UnpackOCIImage();

            // Modified by user
            File.WriteAllText(Path.Combine(initInputFolder, "add"), "add");
            File.WriteAllText(Path.Combine(initInputFolder, "metadata.json"), "modify");
            File.Delete(Path.Combine(initInputFolder, "ADT_A01.liquid"));

            // Push new image ignore base layer.
            string testPushNewBaseLayerImageReference = _containerRegistryServer + "/templatetest:push_newbaselayer";
            var pushManager = new OCIFileManager(testPushNewBaseLayerImageReference, initInputFolder);
            pushManager.PackOCIImage(true);
            await pushManager.PushOCIImageAsync();

            // Check Image
            string command = $"pull {testPushNewBaseLayerImageReference} -o checkNewBaseLayerFolder";
            OrasUtility.OrasExecution(command, _orasFileName);
            Assert.Single(Directory.EnumerateFiles("checkNewBaseLayerFolder", "*.tar.gz", SearchOption.AllDirectories));
            Assert.Equal(840, StreamUtility.DecompressTarGzStream(File.OpenRead(Path.Combine("checkNewBaseLayerFolder", "layer1.tar.gz"))).Count());
            ClearFolder(initInputFolder);
            ClearFolder("checkNewBaseLayerFolder");
        }

        [Fact]
        public async Task GivenAnInputFolderUnpackedFromBaseLayer_WhenPushOCIFiles_IfUserDoNotModify_OnlyBaseLayerWillBePushedAsync()
        {
            Assert.True(_isOrasValid, _orasErrorMessage);

            // Pull an image
            string initImageReference = _testOneLayerWithValidSequenceNumberImageReference;
            string initInputFolder = "TestData/UserFolder3";
            var testManager = new OCIFileManager(initImageReference, initInputFolder);
            await testManager.PullOCIImageAsync();
            testManager.UnpackOCIImage();

            // Push image.
            string testPushBaseLayerImageReference = _containerRegistryServer + "/templatetest:push_baselayer";
            var pushManager = new OCIFileManager(testPushBaseLayerImageReference, initInputFolder);
            pushManager.PackOCIImage();
            await pushManager.PushOCIImageAsync();

            // Check Image
            string command = $"pull {testPushBaseLayerImageReference} -o checkBaseLayerFolder";
            OrasUtility.OrasExecution(command, _orasFileName);
            Assert.Single(Directory.EnumerateFiles("checkBaseLayerFolder", "*.tar.gz", SearchOption.AllDirectories));
            ClearFolder(initInputFolder);
            ClearFolder("checkBaseLayerFolder");
        }

        [Fact]
        public async Task GivenAnInputFolderWithoutBaseLayer_WhenPushOCIFiles_IfUserModify_OneBaseLayerWillBePushedAsync()
        {
            Assert.True(_isOrasValid, _orasErrorMessage);

            // Pull an image
            string initImageReference = _testOneLayerWithoutSequenceNumberImageReference;
            string initInputFolder = "TestData/UserFolder4";
            var testManager = new OCIFileManager(initImageReference, initInputFolder);
            await testManager.PullOCIImageAsync();
            testManager.UnpackOCIImage();

            // Modified by user
            File.WriteAllText(Path.Combine(initInputFolder, "add"), "add");

            // Push image.
            string testPushNewBaseLayerImageReference = _containerRegistryServer + "/templatetest:pushwithoutbase_newbaselayer";
            var pushManager = new OCIFileManager(testPushNewBaseLayerImageReference, initInputFolder);
            pushManager.PackOCIImage();
            await pushManager.PushOCIImageAsync();

            // Check Image
            string command = $"pull {testPushNewBaseLayerImageReference} -o checkLayerFolder";
            OrasUtility.OrasExecution(command, _orasFileName);
            Assert.Single(Directory.EnumerateFiles("checkLayerFolder", "*.tar.gz", SearchOption.AllDirectories));
            ClearFolder(initInputFolder);
            ClearFolder("checkLayerFolder");
        }

        [Fact]
        public async Task GivenAnEmptyInputFolder_WhenPushOCIFiles_OneBaseLayerWillBePushedAsync()
        {
            Assert.True(_isOrasValid, _orasErrorMessage);
            string emptyFolder = "emptyFoler";
            Directory.CreateDirectory(emptyFolder);

            // Push image.
            string testPushNewBaseLayerImageReference = _containerRegistryServer + "/templatetest:empty";
            var pushManager = new OCIFileManager(testPushNewBaseLayerImageReference, emptyFolder);
            pushManager.PackOCIImage();
            await Assert.ThrowsAsync<DirectoryNotFoundException>(() => pushManager.PushOCIImageAsync());

            ClearFolder(emptyFolder);
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
    }
}