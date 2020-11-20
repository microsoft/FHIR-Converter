// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.ContainerRegistry.Models;
using Microsoft.Health.Fhir.TemplateManagement.ArtifactProviders;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Microsoft.Health.Fhir.TemplateManagement.Utilities;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Health.Fhir.TemplateManagement.UnitTests.Providers
{
    public class ArtifactProviderTests
    {
        private OCIArtifactProvider _artifactProvider;

        public ArtifactProviderTests()
        {
            MockClient = new MockClient(TestRegistry, MockToken);
            MockClientWithUnAuthToken = new MockClient(TestRegistry, "fakeToken");
            InitImages();
        }

        protected MockClient MockClient { get; }

        protected MockClient MockClientWithUnAuthToken { get; }

        protected string MockToken { get; } = "realToken";

        protected string TestRegistry { get; } = "mockregistry";

        private void InitImages()
        {
            PushDefaultImage();
            PushV1Image();
            PushV2Image();
        }

        private void PushDefaultImage()
        {
            ImageInfo imageInfo = ImageInfo.CreateFromImageReference($"{TestRegistry}/testimagename:default");
            ManifestWrapper defaultManifest = JsonConvert.DeserializeObject<ManifestWrapper>(File.ReadAllText("TestData/ExpectedManifest/manifestdefault"));
            MockClient.PushManifest(imageInfo, defaultManifest);
            MockClientWithUnAuthToken.PushManifest(imageInfo, defaultManifest);
            List<byte[]> layers = new List<byte[]>
            {
                File.ReadAllBytes("TestData/TarGzFiles/baseLayer.tar.gz"),
            };
            MockClient.PushLayers(imageInfo, layers);
            MockClientWithUnAuthToken.PushLayers(imageInfo, layers);
        }

        private void PushV1Image()
        {
            ImageInfo imageInfo = ImageInfo.CreateFromImageReference($"{TestRegistry}/testimagename:v1");
            ManifestWrapper defaultManifest = JsonConvert.DeserializeObject<ManifestWrapper>(File.ReadAllText("TestData/ExpectedManifest/manifestv1"));
            MockClient.PushManifest(imageInfo, defaultManifest);
            List<byte[]> layers = new List<byte[]>
            {
                File.ReadAllBytes("TestData/TarGzFiles/userV1.tar.gz"),
                File.ReadAllBytes("TestData/TarGzFiles/baseLayer.tar.gz"),
            };
            MockClient.PushLayers(imageInfo, layers);
        }

        private void PushV2Image()
        {
            ImageInfo imageInfo = ImageInfo.CreateFromImageReference($"{TestRegistry}/testimagename:v2");
            ManifestWrapper defaultManifest = JsonConvert.DeserializeObject<ManifestWrapper>(File.ReadAllText("TestData/ExpectedManifest/manifestv2"));
            MockClient.PushManifest(imageInfo, defaultManifest);
            List<byte[]> layers = new List<byte[]>
            {
                File.ReadAllBytes("TestData/TarGzFiles/userV2.tar.gz"),
                File.ReadAllBytes("TestData/TarGzFiles/baseLayer.tar.gz"),
            };
            MockClient.PushLayers(imageInfo, layers);
        }

        public static IEnumerable<object[]> GetValidLayerInfo()
        {
            yield return new object[] { "mockregistry/testimagename:v1", "sha256:ceda57333d3ab258176e6f5095f8c348994bc9f30a2db226980f223c62410d03" };
            yield return new object[] { "mockregistry/testimagename:v1", "sha256:fac83d4cd496b0efd76db216568a53e1ff4f9571461deec8835cc4d2e171d063" };
            yield return new object[] { "mockregistry/testimagename:v1", "sha256:ca1d8cff554ca9d7796500548c968f4985528af2676589de3a719048f266a3bc" };
        }

        public static IEnumerable<object[]> GetManifestInfo()
        {
            yield return new object[] { "mockregistry/testimagename:v1", "TestData/ExpectedManifest/manifestv1" };
            yield return new object[] { "mockregistry/testimagename:v2", "TestData/ExpectedManifest/manifestv2" };
            yield return new object[] { "mockregistry/testimagename:default", "TestData/ExpectedManifest/manifestdefault" };
            yield return new object[] { "mockregistry/testimagename@sha256:cbd40a123aea6ef53c6608ad42ebed5c2befe2a7f8923142c04beed0d52c54d3", "TestData/ExpectedManifest/manifestv1" };
            yield return new object[] { "mockregistry/testimagename@sha256:150caa8cc8f56008ef19b021888fb40e4160a20ed25762aab785ec3524c04c5e", "TestData/ExpectedManifest/manifestv2" };
            yield return new object[] { "mockregistry/testimagename@sha256:204f03ecad424e408dcf28747add065edf9f1bfcf44da5f4edded841818337d2", "TestData/ExpectedManifest/manifestdefault" };
        }

        public static IEnumerable<object[]> GetImageInfoForArtifact()
        {
            yield return new object[] { "mockregistry/testimagename:v1", 2 };
            yield return new object[] { "mockregistry/testimagename:v2", 2 };
            yield return new object[] { "mockregistry/testimagename:default", 1 };
            yield return new object[] { "mockregistry/testimagename@sha256:cbd40a123aea6ef53c6608ad42ebed5c2befe2a7f8923142c04beed0d52c54d3", 2 };
            yield return new object[] { "mockregistry/testimagename@sha256:150caa8cc8f56008ef19b021888fb40e4160a20ed25762aab785ec3524c04c5e", 2 };
            yield return new object[] { "mockregistry/testimagename@sha256:204f03ecad424e408dcf28747add065edf9f1bfcf44da5f4edded841818337d2", 1 };
        }

        [Theory]
        [MemberData(nameof(GetValidLayerInfo))]
        public async Task GivenValidLayerInfo_WhenGetLayerFromACR_ACorrectArtifactLayerShouldBeReturnedAsync(string imageReference, string layerDigest)
        {
            ImageInfo imageInfo = ImageInfo.CreateFromImageReference(imageReference);
            _artifactProvider = new OCIArtifactProvider(imageInfo, MockClient);
            ArtifactLayer artifactLayer = await _artifactProvider.GetLayerAsync(layerDigest);
            var ex = Record.Exception(() => ValidationUtility.ValidateOneBlob((byte[])artifactLayer.Content, layerDigest));
            Assert.Null(ex);
        }

        [Theory]
        [MemberData(nameof(GetManifestInfo))]
        public async Task GivenManifestInfo_WhenGetManifestFromACR_ACorrectManifestShouldBeReturnedAsync(string imageReference, string expectedManifestPath)
        {
            ImageInfo imageInfo = ImageInfo.CreateFromImageReference(imageReference);
            _artifactProvider = new OCIArtifactProvider(imageInfo, MockClient);
            var manifest = await _artifactProvider.GetManifestAsync();
            Assert.Equal(File.ReadAllText(expectedManifestPath), JsonConvert.SerializeObject(manifest));
        }

        [Theory]
        [MemberData(nameof(GetImageInfoForArtifact))]
        public async Task GivenImageInfo_WhenGetOCIArtifactFromACR_ACorrectOCIArtifactShouldBeReturnedAsync(string imageReference, int expectedLayerCounts)
        {
            ImageInfo imageInfo = ImageInfo.CreateFromImageReference(imageReference);
            _artifactProvider = new OCIArtifactProvider(imageInfo, MockClient);
            var artifact = await _artifactProvider.GetOCIArtifactAsync();
            Assert.Equal(expectedLayerCounts, artifact.Count());
        }

        [Theory]
        [MemberData(nameof(GetImageInfoForArtifact))]
        public async Task GivenImageInfo_WhenGetOCIArtifactFromACR_IfTokenUnAuth_ExceptionWillBeThrownAsync(string imageReference, object _)
        {
            ImageInfo imageInfo = ImageInfo.CreateFromImageReference(imageReference);
            _artifactProvider = new OCIArtifactProvider(imageInfo, MockClientWithUnAuthToken);
            await Assert.ThrowsAsync<ContainerRegistryAuthenticationException>(async () => await _artifactProvider.GetOCIArtifactAsync());
        }
    }
}
