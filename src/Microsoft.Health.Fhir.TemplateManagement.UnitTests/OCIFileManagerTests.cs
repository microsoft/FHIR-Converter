// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Health.Fhir.TemplateManagement.UnitTests
{
    public class OCIFileManagerTests
    {
        private readonly string _containerRegistryServer;
        private readonly string _baseLayerTemplatePath = "TestData/TarGzFiles/layer1.tar.gz";
        private readonly string _userLayerTemplatePath = "TestData/TarGzFiles/layer2.tar.gz";
        private readonly string _testOneLayerImageReference;
        private bool _isOrasValid = true;
        private string _orasErrorMessage;
        private readonly string _orasFileName;

        public OCIFileManagerTests()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                    _orasFileName = "oras-win.exe";
                    break;
                case PlatformID.Unix:
                    _orasFileName = "oras-unix";
                    OrasUtility.AddFilePermissionInLinuxSystem(_orasFileName);
                    break;
                default:
                    throw new SystemException("System operation is not supported");
            }

            _containerRegistryServer = Environment.GetEnvironmentVariable("TestContainerRegistryServer");
            _testOneLayerImageReference = _containerRegistryServer + "/templatetest:user1";
            PushOneLayerImage();
            PushMultiLayersImage();
        }

        private void PushOneLayerImage()
        {
            string command = $"push {_testOneLayerImageReference} {_baseLayerTemplatePath}";
            _orasErrorMessage = OrasUtility.OrasExecution(command, _orasFileName);
            if (!string.IsNullOrEmpty(_orasErrorMessage))
            {
                _isOrasValid = false;
            }
        }

        private void PushMultiLayersImage()
        {
            string command = $"push {_testOneLayerImageReference} {_baseLayerTemplatePath} {_userLayerTemplatePath}";
            _orasErrorMessage = OrasUtility.OrasExecution(command, _orasFileName);
            if (!string.IsNullOrEmpty(_orasErrorMessage))
            {
                _isOrasValid = false;
            }
        }

        [Fact]
        public async Task GivenAnImageReferenceAndOutputFolder_WhenPullOCIFiles_CorrectFilesWillBeWrittenToFolderAsync()
        {
            Assert.True(_isOrasValid, _orasErrorMessage);
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
            Assert.True(_isOrasValid, _orasErrorMessage);

            string imageReference = _containerRegistryServer + "/templatetest:test";
            string inputFolder = "TestData/UserFolder";
            var testManager = new OCIFileManager(imageReference, inputFolder);
            testManager.PackOCIImage(true);
            await testManager.PushOCIImageAsync();
        }
    }
}
