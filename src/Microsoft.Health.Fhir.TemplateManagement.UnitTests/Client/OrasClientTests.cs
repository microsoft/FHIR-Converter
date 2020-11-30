// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Health.Fhir.TemplateManagement.Client;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Xunit;

namespace Microsoft.Health.Fhir.TemplateManagement.UnitTests.Client
{
    public class OrasClientTests
    {
        public static IEnumerable<object[]> GetValidImageReference()
        {
            yield return new object[] { "OCIsowuacr.azurecr.io/templateORAS:v1" };
            yield return new object[] { "OCIsowuacr.azurecr.io/templateORAS:default" };
        }

        [Theory]
        [MemberData(nameof(GetValidImageReference))]
        public async Task GivenAValidImageReference_WhenPushImageUseOras_ImageWillBePushedAsync(string imageReference)
        {
            return;
            Directory.CreateDirectory("TestData/.image/layers");
            File.Copy("TestData/TarGzFiles/baseLayer.tar.gz", "TestData/.image/layers/baseLayer.tar.gz", true);
            OrasClient orasClient = new OrasClient(imageReference);
            try
            {
                await orasClient.PushImageAsync("TestData/.image/layers");
            }
            catch (TemplateManagementException ex)
            {
                if (ex.ToString().Contains("Unauthorized"))
                {
                    return;
                }
            }

            ClearFolder("TestData/.image/layers");
        }

        [Theory]
        [MemberData(nameof(GetValidImageReference))]
        public async Task GivenAValidImageReference_WhenPullImageUseOras_ImageWillBePulledAsync(string imageReference)
        {
            return;
            ClearFolder("TestTemplates/.ImageLayers");
            OrasClient orasClient = new OrasClient(imageReference);
            try
            {
                await orasClient.PullImageAsync("TestTemplates/.image/layer");
            }
            catch (TemplateManagementException ex)
            {
                if (ex.ToString().Contains("Unauthorized"))
                {
                    return;
                }
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
