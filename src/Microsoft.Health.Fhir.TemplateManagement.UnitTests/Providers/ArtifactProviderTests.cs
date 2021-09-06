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
        private OciArtifactProvider _artifactProvider;

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
            yield return new object[] { "mockregistry/testimagename:v1", "sha256:4c084996bcc80c70aac0a1bc24b0e44fb8f202f983bb598bc34a2cc974417480" };
            yield return new object[] { "mockregistry/testimagename:v1", "sha256:871b9b857c293d6df8e50ce7ef9f5d67e6fb3ed2926da2485c9ce570c0ce6ac4" };
            yield return new object[] { "mockregistry/testimagename:v1", "sha256:86ab65f5fb50b9b94d6283dadd3e108e539c94b4e6b57146158506b0e19769b8" };
        }

        public static IEnumerable<object[]> GetManifestInfo()
        {
            yield return new object[] { "mockregistry/testimagename:v1", "TestData/ExpectedManifest/manifestv1" };
            yield return new object[] { "mockregistry/testimagename:v2", "TestData/ExpectedManifest/manifestv2" };
            yield return new object[] { "mockregistry/testimagename:default", "TestData/ExpectedManifest/manifestdefault" };
            yield return new object[] { "mockregistry/testimagename@sha256:ba9c764587ae875bc955498d8d347aa9e4c9f3e072c7907d778e53340b249df7", "TestData/ExpectedManifest/manifestv1" };
            yield return new object[] { "mockregistry/testimagename@sha256:7d6cfa607d8eaa95b4ef48248693f9faa01b65e0be550d238c61051e54078a5f", "TestData/ExpectedManifest/manifestv2" };
            yield return new object[] { "mockregistry/testimagename@sha256:41e3b808c19715765a5f8c63f3c306deac861490c5cd450d062eb2f64501413b", "TestData/ExpectedManifest/manifestdefault" };
        }

        public static IEnumerable<object[]> GetImageInfoForArtifact()
        {
            yield return new object[] { "mockregistry/testimagename:v1", 2 };
            yield return new object[] { "mockregistry/testimagename:v2", 2 };
            yield return new object[] { "mockregistry/testimagename:default", 1 };
            yield return new object[] { "mockregistry/testimagename@sha256:ba9c764587ae875bc955498d8d347aa9e4c9f3e072c7907d778e53340b249df7", 2 };
            yield return new object[] { "mockregistry/testimagename@sha256:7d6cfa607d8eaa95b4ef48248693f9faa01b65e0be550d238c61051e54078a5f", 2 };
            yield return new object[] { "mockregistry/testimagename@sha256:41e3b808c19715765a5f8c63f3c306deac861490c5cd450d062eb2f64501413b", 1 };
        }

        [Theory]
        [MemberData(nameof(GetValidLayerInfo))]
        public async Task GivenValidLayerInfo_WhenGetLayerFromAcr_ACorrectArtifactLayerShouldBeReturnedAsync(string imageReference, string layerDigest)
        {
            ImageInfo imageInfo = ImageInfo.CreateFromImageReference(imageReference);
            _artifactProvider = new OciArtifactProvider(imageInfo, MockClient);
            ArtifactBlob artifactLayer = await _artifactProvider.GetLayerAsync(layerDigest);
            var ex = Record.Exception(() => ValidationUtility.ValidateOneBlob((byte[])artifactLayer.Content, layerDigest));
            Assert.Null(ex);
        }

        [Theory]
        [MemberData(nameof(GetManifestInfo))]
        public async Task GivenManifestInfo_WhenGetManifestFromAcr_ACorrectManifestShouldBeReturnedAsync(string imageReference, string expectedManifestPath)
        {
            ImageInfo imageInfo = ImageInfo.CreateFromImageReference(imageReference);
            _artifactProvider = new OciArtifactProvider(imageInfo, MockClient);
            var manifest = await _artifactProvider.GetManifestAsync();
            Assert.Equal(File.ReadAllText(expectedManifestPath), JsonConvert.SerializeObject(manifest));
        }

        [Theory]
        [MemberData(nameof(GetImageInfoForArtifact))]
        public async Task GivenImageInfo_WhenGetOciArtifactFromAcr_ACorrectOciArtifactShouldBeReturnedAsync(string imageReference, int expectedLayerCounts)
        {
            ImageInfo imageInfo = ImageInfo.CreateFromImageReference(imageReference);
            _artifactProvider = new OciArtifactProvider(imageInfo, MockClient);
            var artifact = await _artifactProvider.GetOciArtifactAsync();
            Assert.Equal(expectedLayerCounts, artifact.Blobs.Count());
        }

        [Theory]
        [MemberData(nameof(GetImageInfoForArtifact))]
        public async Task GivenImageInfo_WhenGetOciArtifactFromAcr_IfTokenUnAuth_ExceptionWillBeThrownAsync(string imageReference, int expectedLayerCounts)
        {
            ImageInfo imageInfo = ImageInfo.CreateFromImageReference(imageReference);
            _artifactProvider = new OciArtifactProvider(imageInfo, MockClientWithUnAuthToken);
            await Assert.ThrowsAsync<ContainerRegistryAuthenticationException>(async () => await _artifactProvider.GetOciArtifactAsync());
            Assert.IsType<int>(expectedLayerCounts);
        }
    }
}
