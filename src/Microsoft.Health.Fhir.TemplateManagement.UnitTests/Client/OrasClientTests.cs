// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Health.Fhir.TemplateManagement.Client;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Microsoft.Health.Fhir.TemplateManagement.Utilities;
using Xunit;

namespace Microsoft.Health.Fhir.TemplateManagement.UnitTests.Client
{
    public class OrasClientTests : IAsyncLifetime
    {
        private readonly string _containerRegistryServer;
        private readonly string _baseLayerTemplatePath = "TestData/TarGzFiles/baseLayer.tar.gz";
        private readonly string _userLayerTemplatePath = "TestData/TarGzFiles/userV1.tar.gz";
        private readonly string _testOneLayerImageReference;
        private readonly string _testMultiLayersImageReference;
        private bool _isOrasValid = true;

        public OrasClientTests()
        {
            _containerRegistryServer = Environment.GetEnvironmentVariable("TestContainerRegistryServer");
            _testOneLayerImageReference = _containerRegistryServer + "/templatetest:v1";
            _testMultiLayersImageReference = _containerRegistryServer + "/templatetest:v2";
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

        public static IEnumerable<object[]> GetInvalidReference()
        {
            yield return new object[] { "testImage", "v1" };
            yield return new object[] { "testImage", string.Empty };
            yield return new object[] { "testImage", ":@" };
        }

        public static IEnumerable<object[]> GetValidFolder()
        {
            yield return new object[] { @"test folder" };
            yield return new object[] { @"testfolder" };
            yield return new object[] { @"test（1）" };
            yield return new object[] { @"&$%^#$%$" };
        }

        public static IEnumerable<object[]> GetInValidFolder()
        {
            yield return new object[] { @"\\\" };
            yield return new object[] { @"*:" };
            yield return new object[] { @" " };
        }

        [Theory]
        [MemberData(nameof(GetInValidFolder))]
        public async Task GivenInValidOutputFolder_WhenPullUseOras_ExceptionWillBeThrownAsync(string outputFolder)
        {
            if (!_isOrasValid)
            {
                return;
            }

            string imageReference = _testOneLayerImageReference;
            var imageInfo = ImageInfo.CreateFromImageReference(imageReference);
            OrasClient orasClient = new OrasClient(_containerRegistryServer, outputFolder);
            await Assert.ThrowsAsync<OciClientException>(async () => await orasClient.PullImageAsync(imageInfo.ImageName, imageInfo.Tag));
        }

        [Theory]
        [MemberData(nameof(GetValidFolder))]
        public async Task GivenValidOutputFolder_WhenPullImageUseOras_ImageWillBePulledAsync(string outputFolder)
        {
            if (!_isOrasValid)
            {
                return;
            }

            outputFolder = "testpull" + outputFolder;

            DirectoryHelper.ClearFolder(outputFolder);

            string imageReference = _testOneLayerImageReference;
            OrasClient orasClient = new OrasClient(_containerRegistryServer, outputFolder);
            var imageInfo = ImageInfo.CreateFromImageReference(imageReference);
            var ex = await Record.ExceptionAsync(async () => await orasClient.PullImageAsync(imageInfo.ImageName, imageInfo.Tag));
            Assert.Null(ex);
            Assert.Single(Directory.EnumerateFiles(outputFolder, "*.*", SearchOption.AllDirectories));

            DirectoryHelper.ClearFolder(outputFolder);
        }

        [Theory]
        [MemberData(nameof(GetInvalidReference))]
        public async Task GivenAnInValidImageReference_WhenPullAndPushImageUseOras_ExceptionWillBeThrown(string imageName, string tag)
        {
            if (!_isOrasValid)
            {
                return;
            }

            DirectoryHelper.ClearFolder("TestData/PushTest");

            Directory.CreateDirectory("TestData/PushTest");
            string imageReference = _containerRegistryServer + "/" + imageName + ":" + tag;
            OrasClient orasClient = new OrasClient(_containerRegistryServer, "TestData/PushTest");

            var blob = new ArtifactBlob();
            await blob.ReadFromFileAsync(_baseLayerTemplatePath);
            var image = new ArtifactImage() { Blobs = new List<ArtifactBlob>() { blob } };
            await Assert.ThrowsAsync<OciClientException>(() => orasClient.PushImageAsync(imageName, tag, image));
            await Assert.ThrowsAsync<OciClientException>(() => orasClient.PullImageAsync(imageName, tag));

            DirectoryHelper.ClearFolder("TestData/PushTest");
        }

        [Theory]
        [MemberData(nameof(GetValidFolder))]
        public async Task GivenValidInputFolder_WhenPushUseOras_ImageWillBePushedAsync(string outputFolder)
        {
            if (!_isOrasValid)
            {
                return;
            }

            outputFolder = "testpush" + outputFolder;

            DirectoryHelper.ClearFolder(outputFolder);

            string imageReference = _testOneLayerImageReference;
            OrasClient orasClient = new OrasClient(_containerRegistryServer, outputFolder);
            var imageInfo = ImageInfo.CreateFromImageReference(imageReference);
            var image = await orasClient.PullImageAsync(imageInfo.ImageName, imageInfo.Tag);

            string testImageReference = _containerRegistryServer + "/test:test";
            imageInfo = ImageInfo.CreateFromImageReference(testImageReference);
            var ex = await Record.ExceptionAsync(async () => await orasClient.PushImageAsync(imageInfo.ImageName, imageInfo.Tag, image));
            Assert.Null(ex);
            DirectoryHelper.ClearFolder(outputFolder);
        }

        [Fact]
        public async Task GivenAValidImageReference_WhenPushImageUseOras_ImageWillBePushedAsync()
        {
            if (!_isOrasValid)
            {
                return;
            }

            DirectoryHelper.ClearFolder("TestData/PushTest");

            Directory.CreateDirectory("TestData/PushTest");
            string imageReference = _containerRegistryServer + "/testimage:test";
            OrasClient orasClient = new OrasClient(_containerRegistryServer, "TestData/PushTest");
            var blob = new ArtifactBlob();
            await blob.ReadFromFileAsync(_baseLayerTemplatePath);
            var image = new ArtifactImage() { Blobs = new List<ArtifactBlob>() { blob } };
            var imageInfo = ImageInfo.CreateFromImageReference(imageReference);
            var ex = await Record.ExceptionAsync(() => orasClient.PushImageAsync(imageInfo.ImageName, imageInfo.Tag, image));
            Assert.Null(ex);
            DirectoryHelper.ClearFolder("TestData/PushTest");
        }

        [Fact]
        public async Task GivenAValidImageReference_WhenPushEmptyImageUseOras_ImageWillNotBePushedAsync()
        {
            if (!_isOrasValid)
            {
                return;
            }

            DirectoryHelper.ClearFolder("TestData/Empty");

            Directory.CreateDirectory("TestData/Empty");
            string imageReference = _containerRegistryServer + "/testimage:test";
            OrasClient orasClient = new OrasClient(_containerRegistryServer, "TestData/Empty");
            var imageInfo = ImageInfo.CreateFromImageReference(imageReference);
            await Assert.ThrowsAsync<OverlayException>(async () => await orasClient.PushImageAsync(imageInfo.ImageName, imageInfo.Tag, new ArtifactImage()));

            DirectoryHelper.ClearFolder("TestData/Empty");
        }

        [Fact]
        public async Task GivenAValidImageReference_WhenPullImageUseOras_ImageWillBePulledAsync()
        {
            if (!_isOrasValid)
            {
                return;
            }

            DirectoryHelper.ClearFolder("TestData/PullTest");

            string imageReference = _testOneLayerImageReference;
            OrasClient orasClient = new OrasClient(_containerRegistryServer, "TestData/PullTest");
            var imageInfo = ImageInfo.CreateFromImageReference(imageReference);
            var ex = await Record.ExceptionAsync(async () => await orasClient.PullImageAsync(imageInfo.ImageName, imageInfo.Tag));
            Assert.Null(ex);

            DirectoryHelper.ClearFolder("TestData/PullTest");
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