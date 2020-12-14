// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
    /// Push artifacts without baselayer, successfully pushed one-layer image.
    /// Push artifacts and ignore base layer, successfully pushed one-layer image.
    /// Push empty artifacts, exception will be thrown.
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

        public OCIArtifactFunctionalTests()
        {
            //_containerRegistryServer = Environment.GetEnvironmentVariable("TestContainerRegistryServer");
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
            OrasExecution(command);
        }

        private void PushOneLayerWithoutSequenceNumber()
        {
            string command = $"push {_testOneLayerWithoutSequenceNumberImageReference} {_emptySequenceNumberLayerPath}";
            OrasExecution(command);
        }

        private void PushOneLayerWithInvalidSequenceNumber()
        {
            string command = $"push {_testOneLayerWithInValidSequenceNumberImageReference} {_userLayerTemplatePath}";
            OrasExecution(command);
        }

        private void PushMultiLayerWithValidSequenceNumber()
        {
            string command = $"push {_testMultiLayersWithValidSequenceNumberImageReference} {_baseLayerTemplatePath} {_userLayerTemplatePath}";
            OrasExecution(command);
        }

        private void PushMultiLayerWithInValidSequenceNumber()
        {
            string command = $"push {_testMultiLayersWithInValidSequenceNumberImageReference} {_emptySequenceNumberLayerPath} {_userLayerTemplatePath}";
            OrasExecution(command);
        }

        private void PushInvalidCompressedImage()
        {
            string command = $"push {_testInvalidCompressedImageReference} {_invalidCompressedImageLayerPath}";
            OrasExecution(command);
        }

        [Fact]
        public async Task GivenOneLayerImageWithValidSequenceNumber_WhenPulled_ArtifactsWillBePulledWithBaseLayerCopiedAsync()
        {
            if (!_isOrasValid)
            {
                return;
            }

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
            if (!_isOrasValid)
            {
                return;
            }

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
            if (!_isOrasValid)
            {
                return;
            }

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
            if (!_isOrasValid)
            {
                return;
            }

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
            if (!_isOrasValid)
            {
                return;
            }

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
            if (!_isOrasValid)
            {
                return;
            }

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
            if (!_isOrasValid)
            {
                return;
            }

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
            Assert.True(await pushManager.PushOCIImageAsync());

            // Check Image
            string command = $"pull {testPushMultiLayersImageReference} -o checkMultiLayersFolder";
            OrasExecution(command);
            Assert.Equal(2, Directory.EnumerateFiles("checkMultiLayersFolder", "*.tar.gz", SearchOption.AllDirectories).Count());
            Assert.Equal(4, StreamUtility.DecompressTarGzStream(File.OpenRead(Path.Combine("checkMultiLayersFolder", "layer2.tar.gz"))).Count());
            ClearFolder(initInputFolder);
            ClearFolder("checkMultiLayersFolder");
        }

        [Fact]
        public async Task GivenAnInputFolderUnpackedFromBaseLayer_WhenPushOCIFiles_IfIgnoreBaseLayer_NewBaseLayerWillBePushedAsync()
        {
            if (!_isOrasValid)
            {
                return;
            }

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
            Assert.True(await pushManager.PushOCIImageAsync());

            // Check Image
            string command = $"pull {testPushNewBaseLayerImageReference} -o checkNewBaseLayerFolder";
            OrasExecution(command);
            Assert.Single(Directory.EnumerateFiles("checkNewBaseLayerFolder", "*.tar.gz", SearchOption.AllDirectories));
            Assert.Equal(840, StreamUtility.DecompressTarGzStream(File.OpenRead(Path.Combine("checkNewBaseLayerFolder", "layer1.tar.gz"))).Count());
            ClearFolder(initInputFolder);
            ClearFolder("checkNewBaseLayerFolder");
        }

        [Fact]
        public async Task GivenAnInputFolderUnpackedFromBaseLayer_WhenPushOCIFiles_IfUserDoNotModify_OnlyBaseLayerWillBePushedAsync()
        {
            if (!_isOrasValid)
            {
                return;
            }

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
            Assert.True(await pushManager.PushOCIImageAsync());

            // Check Image
            string command = $"pull {testPushBaseLayerImageReference} -o checkBaseLayerFolder";
            OrasExecution(command);
            Assert.Single(Directory.EnumerateFiles("checkBaseLayerFolder", "*.tar.gz", SearchOption.AllDirectories));
            ClearFolder(initInputFolder);
            ClearFolder("checkBaseLayerFolder");
        }

        [Fact]
        public async Task GivenAnInputFolderWithoutBaseLayer_WhenPushOCIFiles_IfUserModify_OneBaseLayerWillBePushedAsync()
        {
            if (!_isOrasValid)
            {
                return;
            }

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
            Assert.True(await pushManager.PushOCIImageAsync());

            // Check Image
            string command = $"pull {testPushNewBaseLayerImageReference} -o checkLayerFolder";
            OrasExecution(command);
            Assert.Single(Directory.EnumerateFiles("checkLayerFolder", "*.tar.gz", SearchOption.AllDirectories));
            ClearFolder(initInputFolder);
            ClearFolder("checkLayerFolder");
        }

        [Fact]
        public async Task GivenAnEmptyInputFolder_WhenPushOCIFiles_OneBaseLayerWillBePushedAsync()
        {
            if (!_isOrasValid)
            {
                return;
            }
            string emptyFolder = "emptyFoler";
            Directory.CreateDirectory(emptyFolder);
            // Push image.
            string testPushNewBaseLayerImageReference = _containerRegistryServer + "/templatetest:empty";
            var pushManager = new OCIFileManager(testPushNewBaseLayerImageReference, emptyFolder);
            pushManager.PackOCIImage();
            Assert.True(await pushManager.PushOCIImageAsync());

            ClearFolder(emptyFolder);
        }

        private void OrasExecution(string command)
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo(Path.Combine(AppContext.BaseDirectory, "oras.exe")),
            };

            process.StartInfo.Arguments = command;
            process.StartInfo.RedirectStandardError = true;
            process.EnableRaisingEvents = true;
            process.Start();

            StreamReader errStreamReader = process.StandardError;
            process.WaitForExit(30000);
            if (process.HasExited)
            {
                var error = errStreamReader.ReadToEnd();
                if (!string.IsNullOrEmpty(error))
                {
                    _isOrasValid = false;
                }
            }
            else
            {
                _isOrasValid = false;
            }
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
