﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Health.Fhir.TemplateManagement.Client;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Xunit;

namespace Microsoft.Health.Fhir.TemplateManagement.UnitTests
{
    public class OCIFileManagerTests : IAsyncLifetime
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
        }

        public async Task InitializeAsync()
        {
            await PushOneLayerImageAsync();
            await PushMultiLayersImageAsync();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
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
            yield return new object[] { @"\\" };
            yield return new object[] { @"*:" };
            yield return new object[] { @" " };
        }

        public static IEnumerable<object[]> GetInValidImageReferenceInfo()
        {
            yield return new object[] { "testacr.azurecr.io@v1" };
            yield return new object[] { "testacr.azurecr.io:templateset:v1" };
            yield return new object[] { "testacr.azurecr.io_v1" };
            yield return new object[] { "testacr.azurecr.io:v1" };
            yield return new object[] { "testacr.azurecr.io/" };
            yield return new object[] { "/testacr.azurecr.io" };
            yield return new object[] { "testacr.azurecr.io/name:" };
            yield return new object[] { "testacr.azurecr.io/:tag" };
            yield return new object[] { "testacr.azurecr.io/name@" };
            yield return new object[] { "testacr.azurecr.io/INVALID" };
            yield return new object[] { "testacr.azurecr.io/invalid_" };
            yield return new object[] { "testacr.azurecr.io/in*valid" };
            yield return new object[] { "testacr.azurecr.io/org/org/in*valid" };
            yield return new object[] { "testacr.azurecr.io/invalid____set" };
            yield return new object[] { "testacr.azurecr.io/invalid....set" };
            yield return new object[] { "testacr.azurecr.io/invalid._set" };
            yield return new object[] { "testacr.azurecr.io/_invalid" };
        }

        [Theory]
        [MemberData(nameof(GetInValidImageReferenceInfo))]
        public void GivenInValidImageReference_WhenPullOCIFiles_ExceptionWillBeThrownAsync(string imageReference)
        {
            if (!_isOrasValid)
            {
                return;
            }

            string outputFolder = "test";
            Assert.Throws<ImageReferenceException>(() => new OCIFileManager(imageReference, outputFolder));
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

        private void ClearFolder(string directory)
        {
            if (!Directory.Exists(directory))
            {
                return;
            }

            DirectoryInfo folder = new DirectoryInfo(directory);
            folder.Delete(true);
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
    }
}
