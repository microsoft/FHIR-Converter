// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Health.Fhir.TemplateManagement.Client;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Xunit;

namespace Microsoft.Health.Fhir.TemplateManagement.UnitTests.Client
{
    public class OrasClientTests
    {
        private readonly string _containerRegistryServer;
        private readonly string _baseLayerTemplatePath = "TestData/TarGzFiles/baseLayer.tar.gz";
        private readonly string _userLayerTemplatePath = "TestData/TarGzFiles/userV1.tar.gz";
        private readonly string _testOneLayerImageReference;
        private readonly string _testMultiLayerImageReference;
        private bool _isOrasValid = true;


        public OrasClientTests()
        {
            _containerRegistryServer = Environment.GetEnvironmentVariable("TestContainerRegistryServer");
            _testOneLayerImageReference = _containerRegistryServer + "/templatetest:v1";
            _testMultiLayerImageReference = _containerRegistryServer + "/templatetest:v2";
            PushOneLayerImage();
            PushMultiLayersImage();
        }


        public static IEnumerable<object[]> GetInvalidReference()
        {
            yield return new object[] { $"/testImage:v1" };
            yield return new object[] { $"/testImage:" };
            yield return new object[] { $"/testImage@" };
        }

        private void PushOneLayerImage()
        {
            string command = $"push {_testOneLayerImageReference} {_baseLayerTemplatePath}";
            OrasExecution(command);
        }

        private void PushMultiLayersImage()
        {
            string command = $"push {_testOneLayerImageReference} {_baseLayerTemplatePath} {_userLayerTemplatePath}";
            OrasExecution(command);
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
            await Assert.ThrowsAsync<OrasException>(() => orasClient.PushImageAsync("TestData/PushTest"));
            await Assert.ThrowsAsync<OrasException>(() => orasClient.PullImageAsync("TestData/PushTest"));
            ClearFolder("TestData/PushTest");
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
            Assert.True(await orasClient.PushImageAsync("TestData/PushTest"));
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
            Assert.False(await orasClient.PushImageAsync("TestData/Empty"));
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
            Assert.True(await orasClient.PullImageAsync("TestData/PullTest"));
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