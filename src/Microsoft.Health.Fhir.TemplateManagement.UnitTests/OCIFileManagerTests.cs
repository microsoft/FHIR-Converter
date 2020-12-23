// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using System;
using System.Collections.Generic;
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

        public static IEnumerable<object[]> GetValidOutputFolder()
        {
            yield return new object[] { @"OCI/test folder" };
            yield return new object[] { @"OCI/testfolder" };
            yield return new object[] { @"OCI/test（1）" };
            yield return new object[] { @"OCI/&$%^#$%$" };
        }

        public static IEnumerable<object[]> GetInValidOutputFolder()
        {
            yield return new object[] { @" " };
        }

        [Theory]
        [MemberData(nameof(GetInValidOutputFolder))]
        public async Task GivenInValidOutputFolder_WhenPullOCIFiles_ExceptionWillBeThrownAsync(string outputFolder)
        {
            if (!_isOrasValid)
            {
                return;
            }

            string imageReference = _testOneLayerImageReference;
            var testManager = new OCIFileManager(imageReference, outputFolder);
            await Assert.ThrowsAsync<OrasException>(async () => await testManager.PullOCIImageAsync());
            ClearFolder(outputFolder);
        }

        [Theory]
        [MemberData(nameof(GetValidOutputFolder))]
        public async Task GivenValidOutputFolder_WhenPullOCIFiles_CorrectFilesWillBePulledAsync(string outputFolder)
        {
            if (!_isOrasValid)
            {
                return;
            }

            string imageReference = _testOneLayerImageReference;
            var testManager = new OCIFileManager(imageReference, outputFolder);
            await testManager.PullOCIImageAsync();
            testManager.UnpackOCIImage();
            Assert.Equal(842, Directory.EnumerateFiles(outputFolder, "*.*", SearchOption.AllDirectories).Count());
            ClearFolder(outputFolder);
        }

        [Fact]
        public async Task GivenAnImageReferenceAndOutputFolder_WhenPullOCIFiles_CorrectFilesWillBeWrittenToFolderAsync()
        {
            if (!_isOrasValid)
            {
                return;
            }

            string imageReference = _testMultiLayersImageReference;
            string outputFolder = "TestData/testMultiLayers";
            var testManager = new OCIFileManager(imageReference, outputFolder);
            await testManager.PullOCIImageAsync();
            testManager.UnpackOCIImage();
            Assert.Equal(9, Directory.EnumerateFiles(outputFolder, "*.*", SearchOption.AllDirectories).Count());
            ClearFolder(outputFolder);
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
            var ex = await Record.ExceptionAsync(async () => await testManager.PushOCIImageAsync());
            Assert.Null(ex);
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
            if (!Directory.Exists(directory))
            {
                return;
            }

            DirectoryInfo folder = new DirectoryInfo(directory);
            folder.Delete(true);
        }
    }
}
