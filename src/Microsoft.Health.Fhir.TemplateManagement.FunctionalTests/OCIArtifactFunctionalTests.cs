// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Health.Fhir.TemplateManagement.Client;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Utilities;
using Xunit;

namespace Microsoft.Health.Fhir.TemplateManagement.FunctionalTests
{
    public class OCIArtifactFunctionalTests : IAsyncLifetime
    {
        private const string _orasCacheEnvironmentVariableName = "ORAS_CACHE";
        private static readonly string _testTarGzPath = Path.Join("TestData", "TarGzFiles");
        private readonly string _containerRegistryServer;
        private readonly string _baseLayerTemplatePath = Path.Join(_testTarGzPath, "layer1.tar.gz");
        private readonly string _userLayerTemplatePath = Path.Join(_testTarGzPath, "layer2.tar.gz");
        private readonly string _emptySequenceNumberLayerPath = Path.Join(_testTarGzPath, "userV1.tar.gz");
        private readonly string _invalidCompressedImageLayerPath = Path.Join(_testTarGzPath, "invalid1.tar.gz");
        private readonly string _testOneLayerWithValidSequenceNumberImageReference;
        private readonly string _testOneLayerWithoutSequenceNumberImageReference;
        private readonly string _testOneLayerWithInValidSequenceNumberImageReference;
        private readonly string _testMultiLayersWithValidSequenceNumberImageReference;
        private readonly string _testMultiLayersWithInValidSequenceNumberImageReference;
        private readonly string _testInvalidCompressedImageReference;
        private bool _isOrasValid = true;
        private readonly string _orasErrorMessage = "Oras tool invalid.";

        public OCIArtifactFunctionalTests()
        {
            _containerRegistryServer = "localhost:5000";
            _testOneLayerWithValidSequenceNumberImageReference = _containerRegistryServer + "/templatetest:onelayer_valid_sequence";
            _testOneLayerWithoutSequenceNumberImageReference = _containerRegistryServer + "/templatetest:onelayer_without_sequence";
            _testOneLayerWithInValidSequenceNumberImageReference = _containerRegistryServer + "/templatetest:onelayer_invalid_sequence";
            _testMultiLayersWithValidSequenceNumberImageReference = _containerRegistryServer + "/templatetest:multilayers_valid_sequence";
            _testMultiLayersWithInValidSequenceNumberImageReference = _containerRegistryServer + "/templatetest:multilayers_invalid_sequence";
            _testInvalidCompressedImageReference = _containerRegistryServer + "/templatetest:invalid_image";
        }

        public async Task InitializeAsync()
        {
            await PushOneLayerWithValidSequenceNumberAsync();
            await PushOneLayerWithoutSequenceNumberAsync();
            await PushOneLayerWithInvalidSequenceNumberAsync();
            await PushMultiLayersWithValidSequenceNumberAsync();
            await PushMultiLayersWithInValidSequenceNumberAsync();
            await PushInvalidCompressedImageAsync();
        }

        public Task DisposeAsync()
        {
            ClearFolder(Environment.GetEnvironmentVariable(_orasCacheEnvironmentVariableName));
            return Task.CompletedTask;
        }

        private async Task PushOneLayerWithValidSequenceNumberAsync()
        {
            string command = $"push {_testOneLayerWithValidSequenceNumberImageReference} {_baseLayerTemplatePath}";
            await ExecuteOrasCommandAsync(command);
        }

        private async Task PushOneLayerWithoutSequenceNumberAsync()
        {
            string command = $"push {_testOneLayerWithoutSequenceNumberImageReference} {_emptySequenceNumberLayerPath}";
            await ExecuteOrasCommandAsync(command);
        }

        private async Task PushOneLayerWithInvalidSequenceNumberAsync()
        {
            string command = $"push {_testOneLayerWithInValidSequenceNumberImageReference} {_userLayerTemplatePath}";
            await ExecuteOrasCommandAsync(command);
        }

        private async Task PushMultiLayersWithValidSequenceNumberAsync()
        {
            string command = $"push {_testMultiLayersWithValidSequenceNumberImageReference} {_baseLayerTemplatePath} {_userLayerTemplatePath}";
            await ExecuteOrasCommandAsync(command);
        }

        private async Task PushMultiLayersWithInValidSequenceNumberAsync()
        {
            string command = $"push {_testMultiLayersWithInValidSequenceNumberImageReference} {_baseLayerTemplatePath} {_emptySequenceNumberLayerPath}";
            await ExecuteOrasCommandAsync(command);
        }

        private async Task PushInvalidCompressedImageAsync()
        {
            string command = $"push {_testInvalidCompressedImageReference} {_invalidCompressedImageLayerPath}";
            await ExecuteOrasCommandAsync(command);
        }

        private async Task ExecuteOrasCommandAsync(string command)
        {
            try
            {
                await OrasClient.OrasExecutionAsync(command, Directory.GetCurrentDirectory());
            }
            catch
            {
                _isOrasValid = false;
            }
        }

        // Pull one layer image with valid sequence number, successfully pulled with base layer copied.
        [Fact]
        public async Task GivenOneLayerImage_WhenPulled_ArtifactsWillBePulledWithBaseLayerCopiedAsync()
        {
            Assert.True(_isOrasValid, _orasErrorMessage);
            var imageReference = _testOneLayerWithValidSequenceNumberImageReference;
            string outputFolder = "TestData/testOneLayerWithValidSequenceNumber";
            var testManager = new OCIFileManager(imageReference, outputFolder);
            var imageInfo = await testManager.PullOCIImageAsync();
            testManager.UnpackOCIImage(imageInfo.Manifest);
            Assert.Equal(843, Directory.EnumerateFiles(outputFolder, "*.*", SearchOption.AllDirectories).Count());
            Assert.Single(Directory.EnumerateFiles(Path.Combine(outputFolder, ".image", "base"), "*.tar.gz", SearchOption.AllDirectories));
            ClearFolder(outputFolder);
        }

        // Pull one layer image without sequence number, successfully pulled with base layer copied.
        [Fact]
        public async Task GivenOneLayerImageWithoutSequenceNumber_WhenPulled_ArtifactsWillBePulledWithBaseLayerCopiedAsync()
        {
            Assert.True(_isOrasValid, _orasErrorMessage);
            string imageReference = _testOneLayerWithoutSequenceNumberImageReference;
            string outputFolder = "TestData/testOneLayerWithoutSequenceNumber";
            var testManager = new OCIFileManager(imageReference, outputFolder);
            var imageInfo = await testManager.PullOCIImageAsync();
            testManager.UnpackOCIImage(imageInfo.Manifest);
            Assert.Equal(4, Directory.EnumerateFiles(outputFolder, "*.*", SearchOption.AllDirectories).Count());
            Assert.Single(Directory.EnumerateFiles(Path.Combine(outputFolder, ".image", "base"), "*.tar.gz", SearchOption.AllDirectories));
            ClearFolder(outputFolder);
        }

        // Pull one layer image with invalid sequence number, successfully pulled with base layer copied.
        [Fact]
        public async Task GivenOneLayerImageWithInvalidSequenceNumber_WhenPulled_ArtifactsWillBePulledWithBaseLayerCopiedAsync()
        {
            Assert.True(_isOrasValid, _orasErrorMessage);
            string imageReference = _testOneLayerWithInValidSequenceNumberImageReference;
            string outputFolder = "TestData/testOneLayerWithInValidSequenceNumber";
            var testManager = new OCIFileManager(imageReference, outputFolder);
            var imageInfo = await testManager.PullOCIImageAsync();
            testManager.UnpackOCIImage(imageInfo.Manifest);
            Assert.Equal(4, Directory.EnumerateFiles(outputFolder, "*.*", SearchOption.AllDirectories).Count());
            Assert.Single(Directory.EnumerateFiles(Path.Combine(outputFolder, ".image", "base"), "*.tar.gz", SearchOption.AllDirectories));
            ClearFolder(outputFolder);
        }

        // Pull multi-layers with valid sequence numbers, successfully pulled with base layer copied.
        [Fact]
        public async Task GivenMultiLayerImageWithValidSequenceNumber_WhenPulled_ArtifactsWillBePulledWithBaseLayerCopiedAsync()
        {
            Assert.True(_isOrasValid, _orasErrorMessage);
            string imageReference = _testMultiLayersWithValidSequenceNumberImageReference;
            string outputFolder = "TestData/testMultiLayersWithValidSequenceNumber";
            var testManager = new OCIFileManager(imageReference, outputFolder);
            var imageInfo = await testManager.PullOCIImageAsync();
            testManager.UnpackOCIImage(imageInfo.Manifest);
            Assert.Equal(10, Directory.EnumerateFiles(outputFolder, "*.*", SearchOption.AllDirectories).Count());
            Assert.Single(Directory.EnumerateFiles(Path.Combine(outputFolder, ".image", "base"), "*.tar.gz", SearchOption.AllDirectories));
            ClearFolder(outputFolder);
        }

        // Pull multi-layers with invalid sequence numbers, successfully pulled with base layer copied.
        [Fact]
        public async Task GivenMultiLayersImageWithInvalidSequenceNumber_WhenPulled_ArtifactsWillBePulledWithBaseLayerCopiedAsync()
        {
            Assert.True(_isOrasValid, _orasErrorMessage);
            string imageReference = _testMultiLayersWithInValidSequenceNumberImageReference;
            string outputFolder = "TestData/testMultiLayersWithInValidSequenceNumber";
            var testManager = new OCIFileManager(imageReference, outputFolder);
            var imageInfo = await testManager.PullOCIImageAsync();
            testManager.UnpackOCIImage(imageInfo.Manifest);
            Assert.Equal(32, Directory.EnumerateFiles(outputFolder, "*.*", SearchOption.AllDirectories).Count());
            Assert.Single(Directory.EnumerateFiles(Path.Combine(outputFolder, ".image", "base"), "*.tar.gz", SearchOption.AllDirectories));
            ClearFolder(outputFolder);
        }

        // Pull invalid image, exception will be thrown.
        [Fact]
        public async Task GivenInvalidCompressedImage_WhenPulled_ExceptionWillBeThrownAsync()
        {
            Assert.True(_isOrasValid, _orasErrorMessage);
            string imageReference = _testInvalidCompressedImageReference;
            string outputFolder = "TestData/testInvalidCompressedImage";
            var testManager = new OCIFileManager(imageReference, outputFolder);
            var imageInfo = await testManager.PullOCIImageAsync();
            Assert.Throws<ArtifactDecompressException>(() => testManager.UnpackOCIImage(imageInfo.Manifest));
            ClearFolder(outputFolder);
        }

        // Push artifacts which unpacked from base layer. If user modify artifacts, successfully pushed multi-layers image.
        [Fact]
        public async Task GivenAnInputFolderUnpackedFromBaseLayer_WhenPushOCIFiles_IfUserModify_TwoLayersWillBePushedAsync()
        {
            Assert.True(_isOrasValid, _orasErrorMessage);

            // Pull an image
            string initImageReference = _testOneLayerWithValidSequenceNumberImageReference;
            string initInputFolder = "TestData/UserFolder1";
            var testManager = new OCIFileManager(initImageReference, initInputFolder);
            var imageInfo = await testManager.PullOCIImageAsync();
            testManager.UnpackOCIImage(imageInfo.Manifest);

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
            await OrasClient.OrasExecutionAsync(command, Directory.GetCurrentDirectory());
            Assert.Equal(2, Directory.EnumerateFiles("checkMultiLayersFolder", "*.tar.gz", SearchOption.AllDirectories).Count());
            Assert.Equal(4, StreamUtility.DecompressTarGzStream(File.OpenRead(Path.Combine("checkMultiLayersFolder", "layer2.tar.gz"))).Count());
            ClearFolder(initInputFolder);
            ClearFolder("checkMultiLayersFolder");
        }

        // Push artifacts and ignore base layer, successfully pushed one-layer image.
        [Fact]
        public async Task GivenAnInputFolderUnpackedFromBaseLayer_WhenPushOCIFiles_IfIgnoreBaseLayer_NewBaseLayerWillBePushedAsync()
        {
            Assert.True(_isOrasValid, _orasErrorMessage);

            // Pull an image
            string initImageReference = _testOneLayerWithValidSequenceNumberImageReference;
            string initInputFolder = "TestData/UserFolder2";
            var testManager = new OCIFileManager(initImageReference, initInputFolder);
            var imageInfo = await testManager.PullOCIImageAsync();
            testManager.UnpackOCIImage(imageInfo.Manifest);

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
            await OrasClient.OrasExecutionAsync(command, Directory.GetCurrentDirectory());
            Assert.Single(Directory.EnumerateFiles("checkNewBaseLayerFolder", "*.tar.gz", SearchOption.AllDirectories));
            Assert.Equal(840, StreamUtility.DecompressTarGzStream(File.OpenRead(Path.Combine("checkNewBaseLayerFolder", "layer1.tar.gz"))).Count());
            ClearFolder(initInputFolder);
            ClearFolder("checkNewBaseLayerFolder");
        }

        // Push artifacts which unpacked from base layer. If user don't modify artifacts, successfully pushed one-layer image.
        [Fact]
        public async Task GivenAnInputFolderUnpackedFromBaseLayer_WhenPushOCIFiles_IfUserDoNotModify_OnlyBaseLayerWillBePushedAsync()
        {
            Assert.True(_isOrasValid, _orasErrorMessage);

            // Pull an image
            string initImageReference = _testOneLayerWithValidSequenceNumberImageReference;
            string initInputFolder = "TestData/UserFolder3";
            var testManager = new OCIFileManager(initImageReference, initInputFolder);
            var imageInfo = await testManager.PullOCIImageAsync();
            testManager.UnpackOCIImage(imageInfo.Manifest);

            // Push image.
            string testPushBaseLayerImageReference = _containerRegistryServer + "/templatetest:push_baselayer";
            var pushManager = new OCIFileManager(testPushBaseLayerImageReference, initInputFolder);
            pushManager.PackOCIImage();
            await pushManager.PushOCIImageAsync();

            // Check Image
            string command = $"pull {testPushBaseLayerImageReference} -o checkBaseLayerFolder";
            await OrasClient.OrasExecutionAsync(command, Directory.GetCurrentDirectory());
            Assert.Single(Directory.EnumerateFiles("checkBaseLayerFolder", "*.tar.gz", SearchOption.AllDirectories));
            ClearFolder(initInputFolder);
            ClearFolder("checkBaseLayerFolder");
        }

        // Push artifacts without base layer, successfully pushed one-layer image.
        [Fact]
        public async Task GivenAnInputFolderWithoutBaseLayer_WhenPushOCIFiles_IfUserModify_OneBaseLayerWillBePushedAsync()
        {
            Assert.True(_isOrasValid, _orasErrorMessage);

            string initInputFolder = "TestData/UserFolder4";
            Directory.CreateDirectory(initInputFolder);

            // Modified by user
            File.WriteAllText(Path.Combine(initInputFolder, "add"), "add");

            // Push image.
            string testPushNewBaseLayerImageReference = _containerRegistryServer + "/templatetest:pushwithoutbase_newbaselayer";
            var pushManager = new OCIFileManager(testPushNewBaseLayerImageReference, initInputFolder);
            pushManager.PackOCIImage();
            await pushManager.PushOCIImageAsync();

            // Check Image
            string command = $"pull {testPushNewBaseLayerImageReference} -o checkLayerFolder";
            await OrasClient.OrasExecutionAsync(command, Directory.GetCurrentDirectory());
            Assert.Single(Directory.EnumerateFiles("checkLayerFolder", "*.tar.gz", SearchOption.AllDirectories));
            ClearFolder(initInputFolder);
            ClearFolder("checkLayerFolder");
        }

        // Push empty artifact folder, exception will be thrown.
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
            await Assert.ThrowsAsync<OverlayException>(() => pushManager.PushOCIImageAsync());

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