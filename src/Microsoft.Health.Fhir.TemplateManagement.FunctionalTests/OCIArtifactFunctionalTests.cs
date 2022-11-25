// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Health.Fhir.TemplateManagement.Client;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Microsoft.Health.Fhir.TemplateManagement.Utilities;
using Xunit;

namespace Microsoft.Health.Fhir.TemplateManagement.FunctionalTests
{
    public class OciArtifactFunctionalTests : IAsyncLifetime
    {
        private static readonly string _testTarGzPath = Path.Join("TestData", "TarGzFiles");
        private readonly string _containerRegistryServer;
        private readonly string _baseLayerTemplatePath = Path.Join(_testTarGzPath, "layerbase.tar.gz");
        private readonly string _userLayerTemplatePath = Path.Join(_testTarGzPath, "layer2.tar.gz");
        private readonly string _emptySequenceNumberLayerPath = Path.Join(_testTarGzPath, "userV1.tar.gz");
        private readonly string _invalidCompressedImageLayerPath = Path.Join(_testTarGzPath, "invalid1.tar.gz");
        private readonly string _testOneLayerWithValidSequenceNumberImageReference;
        private readonly string _testOneLayerWithoutSequenceNumberImageReference;
        private readonly string _testOneLayerWithInValidSequenceNumberImageReference;
        private readonly string _testMultiLayersWithValidSequenceNumberImageReference;
        private readonly string _testMultiLayersWithInValidSequenceNumberImageReference;
        private readonly string _testInvalidCompressedImageReference;
        private string _testOneLayerImageDigest;
        private string _testMultiLayerImageDigest;
        private bool _isOrasValid = true;
        private readonly string _orasErrorMessage = "Oras tool invalid.";
        private const string _orasCacheEnvironmentVariableName = "ORAS_CACHE";
        private const string _defaultOrasCacheEnvironmentVariable = ".oras/cache";

        public OciArtifactFunctionalTests()
        {
            _containerRegistryServer = "localhost:5000";
            _testOneLayerWithValidSequenceNumberImageReference = _containerRegistryServer + "/templatetest:onelayer_valid_sequence";
            _testOneLayerWithoutSequenceNumberImageReference = _containerRegistryServer + "/templatetest:onelayer_without_sequence";
            _testOneLayerWithInValidSequenceNumberImageReference = _containerRegistryServer + "/templatetest:onelayer_invalid_sequence";
            _testMultiLayersWithValidSequenceNumberImageReference = _containerRegistryServer + "/templatetest:multilayers_valid_sequence";
            _testMultiLayersWithInValidSequenceNumberImageReference = _containerRegistryServer + "/templatetest:multilayers_invalid_sequence";
            _testInvalidCompressedImageReference = _containerRegistryServer + "/templatetest:invalid_image";

            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(_orasCacheEnvironmentVariableName)))
            {
                Environment.SetEnvironmentVariable(_orasCacheEnvironmentVariableName, _defaultOrasCacheEnvironmentVariable);
            }
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
            return Task.CompletedTask;
        }

        private async Task PushOneLayerWithValidSequenceNumberAsync()
        {
            string command = $"push {_testOneLayerWithValidSequenceNumberImageReference} {_baseLayerTemplatePath}";
            _testOneLayerImageDigest = await ExecuteOrasCommandAsync(command);
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
            _testMultiLayerImageDigest = await ExecuteOrasCommandAsync(command);
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

        private async Task<string> ExecuteOrasCommandAsync(string command)
        {
            try
            {
                var output = await OrasClient.OrasExecutionAsync(command, Directory.GetCurrentDirectory());
                var digest = GetImageDigest(output);
                return digest.Value;
            }
            catch
            {
                _isOrasValid = false;
                return null;
            }
        }

        private Digest GetImageDigest(string input)
        {
            var digests = Digest.GetDigest(input);
            if (digests.Count == 0)
            {
                throw new OciClientException(TemplateManagementErrorCode.OrasProcessFailed, "Failed to parse image digest.");
            }

            return digests[0];
        }

        // Pull one layer image with valid sequence number, successfully pulled with base layer copied.
        [Fact]
        public async Task GivenOneLayerImage_WhenPulled_ArtifactsWillBePulledWithBaseLayerCopiedAsync()
        {
            Assert.True(_isOrasValid, _orasErrorMessage);
            var imageReference = _testOneLayerWithValidSequenceNumberImageReference;
            string outputFolder = "TestData/testOneLayerWithValidSequenceNumber";
            DirectoryHelper.ClearFolder(outputFolder);

            var imageInfo = ImageInfo.CreateFromImageReference(imageReference);
            var testManager = new OciFileManager(_containerRegistryServer, outputFolder);
            await testManager.PullOciImageAsync(imageInfo.ImageName, imageInfo.Tag, true);
            Assert.Equal(843, Directory.EnumerateFiles(outputFolder, "*.*", SearchOption.AllDirectories).Count());
            Assert.Single(Directory.EnumerateFiles(Path.Combine(outputFolder, ".image", "base"), "*.tar.gz", SearchOption.AllDirectories));
            DirectoryHelper.ClearFolder(outputFolder);
        }

        // Pull one layer image without sequence number, successfully pulled with base layer copied.
        [Fact]
        public async Task GivenOneLayerImageWithoutSequenceNumber_WhenPulled_ArtifactsWillBePulledWithBaseLayerCopiedAsync()
        {
            Assert.True(_isOrasValid, _orasErrorMessage);
            string imageReference = _testOneLayerWithoutSequenceNumberImageReference;
            string outputFolder = "TestData/testOneLayerWithoutSequenceNumber";
            DirectoryHelper.ClearFolder(outputFolder);

            var imageInfo = ImageInfo.CreateFromImageReference(imageReference);
            var testManager = new OciFileManager(_containerRegistryServer, outputFolder);
            await testManager.PullOciImageAsync(imageInfo.ImageName, imageInfo.Tag, true);
            Assert.Equal(4, Directory.EnumerateFiles(outputFolder, "*.*", SearchOption.AllDirectories).Count());
            Assert.Single(Directory.EnumerateFiles(Path.Combine(outputFolder, ".image", "base"), "*.tar.gz", SearchOption.AllDirectories));
            DirectoryHelper.ClearFolder(outputFolder);
        }

        [Fact]
        public async Task GivenOneLayerImage_WhenPulledUsingDigest_ArtifactsWillBePulledWithBaseLayerCopiedAsync()
        {
            Assert.True(_isOrasValid, _orasErrorMessage);
            string outputFolder = "TestData/testOneLayerWithDigest";
            DirectoryHelper.ClearFolder(outputFolder);

            var testManager = new OciFileManager(_containerRegistryServer, outputFolder);
            await testManager.PullOciImageAsync("templatetest", _testOneLayerImageDigest, true);
            Assert.Equal(843, Directory.EnumerateFiles(outputFolder, "*.*", SearchOption.AllDirectories).Count());
            Assert.Single(Directory.EnumerateFiles(Path.Combine(outputFolder, ".image", "base"), "*.tar.gz", SearchOption.AllDirectories));
            DirectoryHelper.ClearFolder(outputFolder);
        }

        // Pull one layer image with invalid sequence number, successfully pulled with base layer copied.
        [Fact]
        public async Task GivenOneLayerImageWithInvalidSequenceNumber_WhenPulled_ArtifactsWillBePulledWithBaseLayerCopiedAsync()
        {
            Assert.True(_isOrasValid, _orasErrorMessage);
            string imageReference = _testOneLayerWithInValidSequenceNumberImageReference;
            string outputFolder = "TestData/testOneLayerWithInValidSequenceNumber";
            DirectoryHelper.ClearFolder(outputFolder);

            var imageInfo = ImageInfo.CreateFromImageReference(imageReference);
            var testManager = new OciFileManager(_containerRegistryServer, outputFolder);
            await testManager.PullOciImageAsync(imageInfo.ImageName, imageInfo.Tag, true);
            Assert.Equal(4, Directory.EnumerateFiles(outputFolder, "*.*", SearchOption.AllDirectories).Count());
            Assert.Single(Directory.EnumerateFiles(Path.Combine(outputFolder, ".image", "base"), "*.tar.gz", SearchOption.AllDirectories));
            DirectoryHelper.ClearFolder(outputFolder);
        }

        // Pull multi-layers with valid sequence numbers, successfully pulled with base layer copied.
        [Fact]
        public async Task GivenMultiLayerImageWithValidSequenceNumber_WhenPulled_ArtifactsWillBePulledWithBaseLayerCopiedAsync()
        {
            Assert.True(_isOrasValid, _orasErrorMessage);
            string imageReference = _testMultiLayersWithValidSequenceNumberImageReference;
            string outputFolder = "TestData/testMultiLayersWithValidSequenceNumber";
            DirectoryHelper.ClearFolder(outputFolder);

            var imageInfo = ImageInfo.CreateFromImageReference(imageReference);
            var testManager = new OciFileManager(_containerRegistryServer, outputFolder);
            await testManager.PullOciImageAsync(imageInfo.ImageName, imageInfo.Tag, true);
            Assert.Equal(10, Directory.EnumerateFiles(outputFolder, "*.*", SearchOption.AllDirectories).Count());
            Assert.Single(Directory.EnumerateFiles(Path.Combine(outputFolder, ".image", "base"), "*.tar.gz", SearchOption.AllDirectories));
            DirectoryHelper.ClearFolder(outputFolder);
        }

        // Pull multi-layers with invalid sequence numbers, successfully pulled with base layer copied.
        [Fact]
        public async Task GivenMultiLayersImageWithInvalidSequenceNumber_WhenPulled_ArtifactsWillBePulledWithBaseLayerCopiedAsync()
        {
            Assert.True(_isOrasValid, _orasErrorMessage);
            string imageReference = _testMultiLayersWithInValidSequenceNumberImageReference;
            string outputFolder = "TestData/testMultiLayersWithInValidSequenceNumber";
            DirectoryHelper.ClearFolder(outputFolder);

            var imageInfo = ImageInfo.CreateFromImageReference(imageReference);
            var testManager = new OciFileManager(_containerRegistryServer, outputFolder);
            await testManager.PullOciImageAsync(imageInfo.ImageName, imageInfo.Tag, true);
            Assert.Equal(32, Directory.EnumerateFiles(outputFolder, "*.*", SearchOption.AllDirectories).Count());
            Assert.Single(Directory.EnumerateFiles(Path.Combine(outputFolder, ".image", "base"), "*.tar.gz", SearchOption.AllDirectories));
            DirectoryHelper.ClearFolder(outputFolder);
        }

        [Fact]
        public async Task GivenMultiLayerImage_WhenPulledUsingDigest_ArtifactsWillBePulledWithBaseLayerCopiedAsync()
        {
            Assert.True(_isOrasValid, _orasErrorMessage);
            string outputFolder = "TestData/testMultiLayerWithDigest";
            DirectoryHelper.ClearFolder(outputFolder);

            var testManager = new OciFileManager(_containerRegistryServer, outputFolder);
            await testManager.PullOciImageAsync("templatetest", _testMultiLayerImageDigest, true);
            Assert.Equal(10, Directory.EnumerateFiles(outputFolder, "*.*", SearchOption.AllDirectories).Count());
            Assert.Single(Directory.EnumerateFiles(Path.Combine(outputFolder, ".image", "base"), "*.tar.gz", SearchOption.AllDirectories));
            DirectoryHelper.ClearFolder(outputFolder);
        }

        // Pull invalid image, exception will be thrown.
        [Fact]
        public async Task GivenInvalidCompressedImage_WhenPulled_ExceptionWillBeThrownAsync()
        {
            Assert.True(_isOrasValid, _orasErrorMessage);
            string imageReference = _testInvalidCompressedImageReference;
            string outputFolder = "TestData/testInvalidCompressedImage";
            DirectoryHelper.ClearFolder(outputFolder);

            var imageInfo = ImageInfo.CreateFromImageReference(imageReference);
            var testManager = new OciFileManager(_containerRegistryServer, outputFolder);
            await Assert.ThrowsAsync<ArtifactArchiveException>(() => testManager.PullOciImageAsync(imageInfo.ImageName, imageInfo.Tag, true));
            DirectoryHelper.ClearFolder(outputFolder);
        }

        // Push artifacts which unpacked from base layer. If user modify artifacts, successfully pushed multi-layers image.
        [Fact]
        public async Task GivenAnInputFolderUnpackedFromBaseLayer_WhenPushOciFiles_IfUserModify_TwoLayersWillBePushedAsync()
        {
            Assert.True(_isOrasValid, _orasErrorMessage);

            // Pull an image
            string initImageReference = _testOneLayerWithValidSequenceNumberImageReference;
            string initInputFolder = "TestData/UserFolder1";

            DirectoryHelper.ClearFolder(initInputFolder);
            DirectoryHelper.ClearFolder("checkMultiLayersFolder");

            var imageInfo = ImageInfo.CreateFromImageReference(initImageReference);
            var testManager = new OciFileManager(_containerRegistryServer, initInputFolder);
            await testManager.PullOciImageAsync(imageInfo.ImageName, imageInfo.Tag, true);

            // Modified by user
            File.WriteAllText(Path.Combine(initInputFolder, "add"), "add");
            File.WriteAllText(Path.Combine(initInputFolder, "metadata.json"), "modify");
            File.Delete(Path.Combine(initInputFolder, "ADT_A01.liquid"));

            // Push new image.
            string testPushMultiLayersImageReference = _containerRegistryServer + "/templatetest:push_multilayers";
            imageInfo = ImageInfo.CreateFromImageReference(testPushMultiLayersImageReference);
            var pushManager = new OciFileManager(_containerRegistryServer, initInputFolder);
            await pushManager.PushOciImageAsync(imageInfo.ImageName, imageInfo.Tag);

            // Check Image
            string command = $"pull {testPushMultiLayersImageReference} -o checkMultiLayersFolder";
            await OrasClient.OrasExecutionAsync(command, Directory.GetCurrentDirectory());
            Assert.Equal(2, Directory.EnumerateFiles("checkMultiLayersFolder", "*.tar.gz", SearchOption.AllDirectories).Count());
            using FileStream fs = File.OpenRead(Path.Combine("checkMultiLayersFolder", "layer2.tar.gz"));
            Assert.Equal(4, StreamUtility.DecompressFromTarGz(fs).Count());
            DirectoryHelper.ClearFolder(initInputFolder);
            DirectoryHelper.ClearFolder("checkMultiLayersFolder");
        }

        // Push artifacts and ignore base layer, successfully pushed one-layer image.
        [Fact]
        public async Task GivenAnInputFolderUnpackedFromBaseLayer_WhenPushOciFiles_IfIgnoreBaseLayer_NewBaseLayerWillBePushedAsync()
        {
            Assert.True(_isOrasValid, _orasErrorMessage);

            // Pull an image
            string initImageReference = _testOneLayerWithValidSequenceNumberImageReference;
            string initInputFolder = "TestData/UserFolder2";

            DirectoryHelper.ClearFolder(initInputFolder);
            DirectoryHelper.ClearFolder("checkNewBaseLayerFolder");

            var testManager = new OciFileManager(_containerRegistryServer, initInputFolder);
            var imageInfo = ImageInfo.CreateFromImageReference(initImageReference);
            await testManager.PullOciImageAsync(imageInfo.ImageName, imageInfo.Tag, true);

            // Modified by user
            File.WriteAllText(Path.Combine(initInputFolder, "add"), "add");
            File.WriteAllText(Path.Combine(initInputFolder, "metadata.json"), "modify");
            File.Delete(Path.Combine(initInputFolder, "ADT_A01.liquid"));

            // Push new image ignore base layer.
            string testPushNewBaseLayerImageReference = _containerRegistryServer + "/templatetest:push_newbaselayer";
            var pushManager = new OciFileManager(_containerRegistryServer, initInputFolder);
            imageInfo = ImageInfo.CreateFromImageReference(testPushNewBaseLayerImageReference);
            await pushManager.PushOciImageAsync(imageInfo.ImageName, imageInfo.Tag, true);

            // Check Image
            string command = $"pull {testPushNewBaseLayerImageReference} -o checkNewBaseLayerFolder";
            await OrasClient.OrasExecutionAsync(command, Directory.GetCurrentDirectory());
            Assert.Single(Directory.EnumerateFiles("checkNewBaseLayerFolder", "*.tar.gz", SearchOption.AllDirectories));
            using FileStream fs = File.OpenRead(Path.Combine("checkNewBaseLayerFolder", "layer1.tar.gz"));
            Assert.Equal(840, StreamUtility.DecompressFromTarGz(fs).Count());
            DirectoryHelper.ClearFolder(initInputFolder);
            DirectoryHelper.ClearFolder("checkNewBaseLayerFolder");
        }

        // Push artifacts which unpacked from base layer. If user don't modify artifacts, successfully pushed one-layer image.
        [Fact]
        public async Task GivenAnInputFolderUnpackedFromBaseLayer_WhenPushOciFiles_IfUserDoNotModify_OnlyBaseLayerWillBePushedAsync()
        {
            Assert.True(_isOrasValid, _orasErrorMessage);

            // Pull an image
            string initImageReference = _testOneLayerWithValidSequenceNumberImageReference;
            string initInputFolder = "TestData/UserFolder3";

            DirectoryHelper.ClearFolder(initInputFolder);
            DirectoryHelper.ClearFolder("checkBaseLayerFolder");

            var testManager = new OciFileManager(_containerRegistryServer, initInputFolder);
            var imageInfo = ImageInfo.CreateFromImageReference(initImageReference);
            await testManager.PullOciImageAsync(imageInfo.ImageName, imageInfo.Tag, true);

            // Push image.
            string testPushBaseLayerImageReference = _containerRegistryServer + "/templatetest:push_baselayer";
            var pushManager = new OciFileManager(_containerRegistryServer, initInputFolder);
            imageInfo = ImageInfo.CreateFromImageReference(testPushBaseLayerImageReference);
            await pushManager.PushOciImageAsync(imageInfo.ImageName, imageInfo.Tag);

            // Check Image
            string command = $"pull {testPushBaseLayerImageReference} -o checkBaseLayerFolder";
            await OrasClient.OrasExecutionAsync(command, Directory.GetCurrentDirectory());
            Assert.Single(Directory.EnumerateFiles("checkBaseLayerFolder", "*.tar.gz", SearchOption.AllDirectories));
            DirectoryHelper.ClearFolder(initInputFolder);
            DirectoryHelper.ClearFolder("checkBaseLayerFolder");
        }

        // Push artifacts without base layer, successfully pushed one-layer image.
        [Fact]
        public async Task GivenAnInputFolderWithoutBaseLayer_WhenPushOciFiles_IfUserModify_OneBaseLayerWillBePushedAsync()
        {
            Assert.True(_isOrasValid, _orasErrorMessage);

            string initInputFolder = "TestData/UserFolder4";
            Directory.CreateDirectory(initInputFolder);
            DirectoryHelper.ClearFolder(initInputFolder);
            DirectoryHelper.ClearFolder("checkLayerFolder");

            // Modified by user
            File.WriteAllText(Path.Combine(initInputFolder, "add"), "add");

            // Push image.
            string testPushNewBaseLayerImageReference = _containerRegistryServer + "/templatetest:pushwithoutbase_newbaselayer";
            var pushManager = new OciFileManager(_containerRegistryServer, initInputFolder);
            var imageInfo = ImageInfo.CreateFromImageReference(testPushNewBaseLayerImageReference);
            await pushManager.PushOciImageAsync(imageInfo.ImageName, imageInfo.Tag);

            // Check Image
            string command = $"pull {testPushNewBaseLayerImageReference} -o checkLayerFolder";
            await OrasClient.OrasExecutionAsync(command, Directory.GetCurrentDirectory());
            Assert.Single(Directory.EnumerateFiles("checkLayerFolder", "*.tar.gz", SearchOption.AllDirectories));
            DirectoryHelper.ClearFolder(initInputFolder);
            DirectoryHelper.ClearFolder("checkLayerFolder");
        }

        // Push empty artifact folder, exception will be thrown.
        [Fact]
        public async Task GivenAnEmptyInputFolder_WhenPushOciFiles_OneBaseLayerWillBePushedAsync()
        {
            Assert.True(_isOrasValid, _orasErrorMessage);
            string emptyFolder = "emptyFoler";

            Directory.CreateDirectory(emptyFolder);
            DirectoryHelper.ClearFolder(emptyFolder);

            // Push image.
            string testPushNewBaseLayerImageReference = _containerRegistryServer + "/templatetest:empty";
            var pushManager = new OciFileManager(_containerRegistryServer, emptyFolder);
            var imageInfo = ImageInfo.CreateFromImageReference(testPushNewBaseLayerImageReference);
            await Assert.ThrowsAsync<OverlayException>(() => pushManager.PushOciImageAsync(imageInfo.ImageName, imageInfo.Tag));

            DirectoryHelper.ClearFolder(emptyFolder);
        }
    }
}