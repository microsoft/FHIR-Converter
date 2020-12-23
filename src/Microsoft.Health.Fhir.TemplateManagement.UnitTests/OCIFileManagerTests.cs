// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
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
        private readonly string _testMultiLayersImageReference;
        private bool _isOrasValid = true;

        public OCIFileManagerTests()
        {
            _containerRegistryServer = Environment.GetEnvironmentVariable("TestContainerRegistryServer");
            _testOneLayerImageReference = _containerRegistryServer + "/templatetest:user1";
            _testMultiLayersImageReference = _containerRegistryServer + "/templatetest:user2";
            PushOneLayerImage();
            PushMultiLayersImage();
        }

        private void PushOneLayerImage()
        {
            string command = $"push {_testOneLayerImageReference} {_baseLayerTemplatePath}";
            OrasExecution(command);
        }

        private void PushMultiLayersImage()
        {
            string command = $"push {_testMultiLayersImageReference} {_baseLayerTemplatePath} {_userLayerTemplatePath}";
            OrasExecution(command);
        }

        [Fact]
        public async Task GivenAnImageReferenceAndOutputFolder_WhenPullOCIFiles_CorrectFilesWillBeWrittenToFolderAsync()
        {
            if (!_isOrasValid)
            {
                return;
            }

            string imageReference = _testOneLayerImageReference;
            string outputFolder = "TestData/testOneLayer";
            var testManager = new OCIFileManager(imageReference, outputFolder);
            await testManager.PullOCIImageAsync();
            testManager.UnpackOCIImage();
            Assert.Equal(842, Directory.EnumerateFiles(outputFolder, "*.*", SearchOption.AllDirectories).Count());
        }

        [Fact]
        public async Task GivenAnImageReferenceAndInputFolder_WhenPushOCIFiles_CorrectImageWillBePushedAsync()
        {
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
    }
}
