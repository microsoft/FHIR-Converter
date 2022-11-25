// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Microsoft.Health.Fhir.TemplateManagement.Utilities;
using Xunit;

namespace Microsoft.Health.Fhir.TemplateManagement.UnitTests
{
    public class OciFileManagerTests : IAsyncLifetime
    {
        private readonly string _containerRegistryServer;
        private readonly string _testOneLayerImageReference;
        private readonly string _testMultiLayersImageReference;
        private string _testOneLayerImageDigest;
        private string _testMultiLayerImageDigest;
        private bool _isOrasValid = true;
        private const string _orasCacheEnvironmentVariableName = "ORAS_CACHE";

        public OciFileManagerTests()
        {
            _containerRegistryServer = Environment.GetEnvironmentVariable("TestContainerRegistryServer");
            _testOneLayerImageReference = _containerRegistryServer + "/templatetest:user1";
            _testMultiLayersImageReference = _containerRegistryServer + "/templatetest:user2";

            OrasUtility.InitOrasCache();
        }

        public async Task InitializeAsync()
        {
            _testOneLayerImageDigest = await OrasUtility.PushOneLayerImageAsync(_testOneLayerImageReference);
            _testMultiLayerImageDigest = await OrasUtility.PushMultiLayersImageAsync(_testMultiLayersImageReference);
            _isOrasValid = !(_testOneLayerImageDigest == null || _testMultiLayerImageDigest == null);
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
        [MemberData(nameof(GetInValidOutputFolder))]
        public async Task GivenInValidOutputFolder_WhenPullOciFiles_ExceptionWillBeThrownAsync(string outputFolder)
        {
            if (!_isOrasValid)
            {
                return;
            }

            string imageReference = _testOneLayerImageReference;
            var testManager = new OciFileManager(_containerRegistryServer, outputFolder);
            var imageInfo = ImageInfo.CreateFromImageReference(imageReference);
            await Assert.ThrowsAsync<OciClientException>(async () => await testManager.PullOciImageAsync(imageInfo.ImageName, imageInfo.Tag, true));
        }

        [Theory]
        [MemberData(nameof(GetValidOutputFolder))]
        public async Task GivenValidOutputFolder_WhenPullOciFiles_CorrectFilesWillBePulledAsync(string outputFolder)
        {
            if (!_isOrasValid)
            {
                return;
            }

            string imageReference = _testOneLayerImageReference;
            var testManager = new OciFileManager(_containerRegistryServer, outputFolder);
            var imageInfo = ImageInfo.CreateFromImageReference(imageReference);
            await testManager.PullOciImageAsync(imageInfo.ImageName, imageInfo.Tag, true);
            Assert.Equal(843, Directory.EnumerateFiles(outputFolder, "*.*", SearchOption.AllDirectories).Count());
            DirectoryHelper.ClearFolder(outputFolder);

            await testManager.PullOciImageAsync(imageInfo.ImageName, _testOneLayerImageDigest, true);
            Assert.Equal(843, Directory.EnumerateFiles(outputFolder, "*.*", SearchOption.AllDirectories).Count());
            DirectoryHelper.ClearFolder(outputFolder);
        }

        [Fact]
        public async Task GivenAnImageReferenceAndOutputFolder_WhenPullOciFiles_CorrectFilesWillBeWrittenToFolderAsync()
        {
            if (!_isOrasValid)
            {
                return;
            }

            string imageReference = _testMultiLayersImageReference;
            string outputFolder = "TestData/testMultiLayers";
            var testManager = new OciFileManager(_containerRegistryServer, outputFolder);
            var imageInfo = ImageInfo.CreateFromImageReference(imageReference);
            await testManager.PullOciImageAsync(imageInfo.ImageName, imageInfo.Tag, true);
            Assert.Equal(10, Directory.EnumerateFiles(outputFolder, "*.*", SearchOption.AllDirectories).Count());
            DirectoryHelper.ClearFolder(outputFolder);

            await testManager.PullOciImageAsync(imageInfo.ImageName, _testMultiLayerImageDigest, true);
            Assert.Equal(10, Directory.EnumerateFiles(outputFolder, "*.*", SearchOption.AllDirectories).Count());
            DirectoryHelper.ClearFolder(outputFolder);
        }

        [Fact]
        public async Task GivenAnImageReferenceAndInputFolder_WhenPushOciFiles_CorrectImageWillBePushedAsync()
        {
            if (!_isOrasValid)
            {
                return;
            }

            string imageReference = _containerRegistryServer + "/templatetest:test";
            string inputFolder = "TestData/UserFolder";
            var testManager = new OciFileManager(_containerRegistryServer, inputFolder);
            var imageInfo = ImageInfo.CreateFromImageReference(imageReference);
            var ex = await Record.ExceptionAsync(async () => await testManager.PushOciImageAsync(imageInfo.ImageName, imageInfo.Tag, true));
            Assert.Null(ex);
        }
    }
}
