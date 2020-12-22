// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Health.Fhir.TemplateManagement.Client;
using Xunit;

namespace Microsoft.Health.Fhir.TemplateManagement.UnitTests
{
    public class OCIFileManagerTests
    {
        private readonly string _containerRegistryServer;
        private readonly string _baseLayerTemplatePath = "TestData/TarGzFiles/layer1.tar.gz";
        private readonly string _userLayerTemplatePath = "TestData/TarGzFiles/layer2.tar.gz";
        private readonly string _testOneLayerImageReference;
        private readonly string _testMultiLayersImageReference;
        private bool _isOrasValid = true;
        private readonly Task _pushOneLayerImage;
        private readonly Task _pushMultiLayerImage;

        public OCIFileManagerTests()
        {
            _pushOneLayerImage = PushOneLayerImageAsync();
            _pushMultiLayerImage = PushMultiLayersImageAsync();
            _containerRegistryServer = Environment.GetEnvironmentVariable("TestContainerRegistryServer");
            _testOneLayerImageReference = _containerRegistryServer + "/templatetest:user1";
            _testMultiLayersImageReference = _containerRegistryServer + "/templatetest:user2";
        }

        private async Task PushOneLayerImageAsync()
        {
            string command = $"push {_testOneLayerImageReference} {_baseLayerTemplatePath}";
            try
            {
                await OrasClient.OrasExecutionAsync(command, Directory.GetCurrentDirectory());
            }
            catch
            {
                _isOrasValid = false;
            }
        }

        private async Task PushMultiLayersImageAsync()
        {
            string command = $"push {_testMultiLayersImageReference} {_baseLayerTemplatePath} {_userLayerTemplatePath}";
            try
            {
                await OrasClient.OrasExecutionAsync(command, Directory.GetCurrentDirectory());
            }
            catch
            {
                _isOrasValid = false;
            }
        }

        [Fact]
        public async Task GivenAnImageReferenceAndOutputFolder_WhenPullOCIFiles_CorrectFilesWillBeWrittenToFolderAsync()
        {
            await _pushOneLayerImage;
            await _pushMultiLayerImage;

            if (!_isOrasValid)
            {
                return;
            }

            string imageReference = _testOneLayerImageReference;
            string outputFolder = "TestData/testOneLayer";
            var testManager = new OCIFileManager(imageReference, outputFolder);
            await testManager.PullOCIImageAsync();
            testManager.UnpackOCIImage();
            Assert.Equal(9, Directory.EnumerateFiles(outputFolder, "*.*", SearchOption.AllDirectories).Count());
        }

        [Fact]
        public async Task GivenAnImageReferenceAndInputFolder_WhenPushOCIFiles_CorrectImageWillBePushedAsync()
        {
            await _pushOneLayerImage;
            await _pushMultiLayerImage;

            if (!_isOrasValid)
            {
                return;
            }

            string imageReference = _containerRegistryServer + "/templatetest:test";
            string inputFolder = "TestData/UserFolder";
            var testManager = new OCIFileManager(imageReference, inputFolder);
            testManager.PackOCIImage(true);
            await testManager.PushOCIImageAsync();
        }
    }
}
