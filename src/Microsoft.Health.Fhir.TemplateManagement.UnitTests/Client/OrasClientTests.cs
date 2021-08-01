﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Health.Fhir.TemplateManagement.Client;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
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
            yield return new object[] { $"/testImage:v1" };
            yield return new object[] { $"/testImage:" };
            yield return new object[] { $"/testImage@" };
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
            yield return new object[] { @"\" };
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
            OrasClient orasClient = new OrasClient(imageReference);
            await Assert.ThrowsAsync<OrasException>(async () => await orasClient.PullImageAsync(outputFolder));
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
            string imageReference = _testOneLayerImageReference;
            OrasClient orasClient = new OrasClient(imageReference);
            var ex = await Record.ExceptionAsync(async () => await orasClient.PullImageAsync(outputFolder));
            Assert.Null(ex);
            Assert.Single(Directory.EnumerateFiles(outputFolder, "*.*", SearchOption.AllDirectories));
            ClearFolder(outputFolder);
        }

        [Theory]
        [MemberData(nameof(GetInvalidReference))]
        public async Task GivenAnInValidImageReference_WhenPullAndPushImageUseOras_ExceptionWillBeThrown(string reference)
        {
            if (!_isOrasValid)
            {
                return;
            }

            Directory.CreateDirectory("TestData/PushTest");
            File.Copy(_baseLayerTemplatePath, "TestData/PushTest/baseLayer.tar.gz", true);
            File.Copy(_userLayerTemplatePath, "TestData/PushTest/userLayer.tar.gz", true);
            string imageReference = _containerRegistryServer + reference;
            OrasClient orasClient = new OrasClient(imageReference);
            await Assert.ThrowsAsync<OrasException>(() => orasClient.PushImageAsync("TestData/PushTest", new List<string> { "baseLayer.tar.gz", "userLayer.tar.gz" }));
            await Assert.ThrowsAsync<OrasException>(() => orasClient.PullImageAsync("TestData/PushTest"));
            ClearFolder("TestData/PushTest");
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
            string imageReference = _testOneLayerImageReference;
            OrasClient orasClient = new OrasClient(imageReference);
            await orasClient.PullImageAsync(outputFolder);

            string testImageReference = _containerRegistryServer + "/testFolder";
            OrasClient testOrasClient = new OrasClient(imageReference);
            var ex = await Record.ExceptionAsync(async () => await orasClient.PushImageAsync(outputFolder, new List<string> { "TestData/TarGzFiles/baseLayer.tar.gz" }));
            Assert.Null(ex);
            ClearFolder(outputFolder);
        }

        [Fact]
        public async Task GivenAValidImageReference_WhenPushImageUseOras_ImageWillBePushedAsync()
        {
            if (!_isOrasValid)
            {
                return;
            }

            Directory.CreateDirectory("TestData/PushTest");
            File.Copy(_baseLayerTemplatePath, "TestData/PushTest/baseLayer.tar.gz", true);
            File.Copy(_userLayerTemplatePath, "TestData/PushTest/userLayer.tar.gz", true);
            string imageReference = _containerRegistryServer + "/testimage:test";
            OrasClient orasClient = new OrasClient(imageReference);
            var ex = await Record.ExceptionAsync(() => orasClient.PushImageAsync("TestData/PushTest", new List<string> { "baseLayer.tar.gz", "userLayer.tar.gz" }));
            Assert.Null(ex);
            ClearFolder("TestData/PushTest");
        }

        [Fact]
        public async Task GivenAValidImageReference_WhenPushEmptyFolderUseOras_ImageWillNotBePushedAsync()
        {
            if (!_isOrasValid)
            {
                return;
            }

            Directory.CreateDirectory("TestData/Empty");
            string imageReference = _containerRegistryServer + "/testimage:test";
            OrasClient orasClient = new OrasClient(imageReference);
            await Assert.ThrowsAsync<OverlayException>(async () => await orasClient.PushImageAsync("TestData/Empty", new List<string> { }));
        }

        [Fact]
        public async Task GivenAValidImageReference_WhenPullImageUseOras_ImageWillBePulledAsync()
        {
            if (!_isOrasValid)
            {
                return;
            }

            string imageReference = _testOneLayerImageReference;
            OrasClient orasClient = new OrasClient(imageReference);
            var ex = await Record.ExceptionAsync(async () => await orasClient.PullImageAsync("TestData/PullTest"));
            Assert.Null(ex);
            ClearFolder("TestData/PullTest");
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