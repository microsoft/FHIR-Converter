// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ContainerRegistry.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Health.Fhir.TemplateManagement.ArtifactProviders;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Health.Fhir.TemplateManagement.UnitTests.Providers
{
    public class TemplateCollectionProviderTests : ArtifactProviderTests
    {
        private TemplateCollectionProvider _templateCollectionProvider;
        private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions() { SizeLimit = 100000000 });
        private readonly MockClient _emptyClient = new MockClient(string.Empty, "realToken");
        private readonly TemplateCollectionConfiguration _defaultConfig = new TemplateCollectionConfiguration();

        public TemplateCollectionProviderTests()
        {
            MemoryCacheEntryOptions memoryOption = new MemoryCacheEntryOptions()
                {
                    AbsoluteExpiration = System.Runtime.Caching.ObjectCache.InfiniteAbsoluteExpiration,
                    Size = 0,
                };

            TemplateLayer hl7V2DefaultTemplateLayer = TemplateLayer.ReadFromEmbeddedResource("Hl7v2DefaultTemplates.tar.gz");
            _cache.Set("microsofthealth/fhirconverter:default", hl7V2DefaultTemplateLayer, memoryOption);
            _cache.Set("microsofthealth/hl7v2templates:default", hl7V2DefaultTemplateLayer, memoryOption);
            TemplateLayer ccdaDefaultTemplateLayer = TemplateLayer.ReadFromEmbeddedResource("CcdaDefaultTemplates.tar.gz");
            _cache.Set("microsofthealth/ccdatemplates:default", ccdaDefaultTemplateLayer, memoryOption);
            TemplateLayer jsonDefaultTemplateLayer = TemplateLayer.ReadFromEmbeddedResource("JsonDefaultTemplates.tar.gz");
            _cache.Set("microsofthealth/jsontemplates:default", jsonDefaultTemplateLayer, memoryOption);

            PushLargeSizeManifest();
        }

        private void PushLargeSizeManifest()
        {
            ImageInfo imageInfo = ImageInfo.CreateFromImageReference($"{TestRegistry}/testimagename:toolargeimage");
            ManifestWrapper defaultManifest = JsonConvert.DeserializeObject<ManifestWrapper>(File.ReadAllText("TestData/InvalidManifest/layerSizeTooLargeManifest"));
            MockClient.PushManifest(imageInfo, defaultManifest);
        }

        public static new IEnumerable<object[]> GetValidLayerInfo()
        {
            yield return new object[] { "mockregistry/testimagename:v1", "sha256:4c084996bcc80c70aac0a1bc24b0e44fb8f202f983bb598bc34a2cc974417480", 767 };
            yield return new object[] { "mockregistry/testimagename:v1", "sha256:871b9b857c293d6df8e50ce7ef9f5d67e6fb3ed2926da2485c9ce570c0ce6ac4", 813 };
            yield return new object[] { "mockregistry/testimagename:v1", "sha256:86ab65f5fb50b9b94d6283dadd3e108e539c94b4e6b57146158506b0e19769b8", 817 };
        }

        public static new IEnumerable<object[]> GetImageInfoForArtifact()
        {
            yield return new object[] { "mockregistry/testimagename:v1", new List<int> { 813, 817 } };
            yield return new object[] { "mockregistry/testimagename:v2", new List<int> { 767, 817 } };
            yield return new object[] { "mockregistry/testimagename:default", new List<int> { 817 } };
            yield return new object[] { "mockregistry/testimagename@sha256:ba9c764587ae875bc955498d8d347aa9e4c9f3e072c7907d778e53340b249df7", new List<int> { 813, 817 } };
            yield return new object[] { "mockregistry/testimagename@sha256:7d6cfa607d8eaa95b4ef48248693f9faa01b65e0be550d238c61051e54078a5f", new List<int> { 767, 817 } };
            yield return new object[] { "mockregistry/testimagename@sha256:41e3b808c19715765a5f8c63f3c306deac861490c5cd450d062eb2f64501413b", new List<int> { 817 } };
        }

        public static IEnumerable<object[]> GetManifestInfoWithTag()
        {
            yield return new object[] { "mockregistry/testimagename:v1", "TestData/ExpectedManifest/manifestv1" };
            yield return new object[] { "mockregistry/testimagename:v2", "TestData/ExpectedManifest/manifestv2" };
            yield return new object[] { "mockregistry/testimagename:default", "TestData/ExpectedManifest/manifestdefault" };
        }

        public static IEnumerable<object[]> GetImageInfoForArtifactWithTag()
        {
            yield return new object[] { "mockregistry/testimagename:v1", new List<int> { 813, 817 } };
            yield return new object[] { "mockregistry/testimagename:v2", new List<int> { 767, 817 } };
            yield return new object[] { "mockregistry/testimagename:default", new List<int> { 817 } };
        }

        public static IEnumerable<object[]> GetDefaultTemplatesInfo()
        {
            yield return new object[] { "microsofthealth/fhirconverter:default", 852 };
            yield return new object[] { "microsofthealth/hl7v2templates:default", 852 };
            yield return new object[] { "microsofthealth/ccdatemplates:default", 821 };
            yield return new object[] { "microsofthealth/jsontemplates:default", 2 };
        }

        [Theory]
        [MemberData(nameof(GetValidLayerInfo))]
        public async Task GivenValidLayerInfo_WhenGetLayerFromTemplateCollectionProvider_ACorrectTemplateLayerShouldBeReturnedAsync(string imageReference, string layerDigest, int expectedCounts)
        {
            ImageInfo imageInfo = ImageInfo.CreateFromImageReference(imageReference);
            _templateCollectionProvider = new TemplateCollectionProvider(imageInfo, MockClient, _cache, _defaultConfig);
            TemplateLayer templateLayer = (TemplateLayer)await _templateCollectionProvider.GetLayerAsync(layerDigest);
            Assert.Equal(expectedCounts, templateLayer.TemplateContent.Count());
        }

        [Theory]
        [MemberData(nameof(GetManifestInfo))]
        public async Task GivenManifestInfo_WhenGetManifestFromTemplateCollectionProvider_ACorrectManifestShouldBeReturnedAsync(string imageReference, string expectedManifestPath)
        {
            ImageInfo imageInfo = ImageInfo.CreateFromImageReference(imageReference);
            _templateCollectionProvider = new TemplateCollectionProvider(imageInfo, MockClient, _cache, _defaultConfig);
            var manifest = await _templateCollectionProvider.GetManifestAsync();
            Assert.Equal(File.ReadAllText(expectedManifestPath), JsonConvert.SerializeObject(manifest));
        }

        [Fact]
        public async Task GivenManifestInfo_WhenGetManifestFromTemplateCollectionProvider_IfImageTooLarge_ExceptionWillBeThrown()
        {
            ImageInfo imageInfo = ImageInfo.CreateFromImageReference($"{TestRegistry}/testimagename:toolargeimage");
            _templateCollectionProvider = new TemplateCollectionProvider(imageInfo, MockClient, _cache, _defaultConfig);
            await Assert.ThrowsAsync<ImageTooLargeException>(() => _templateCollectionProvider.GetManifestAsync());
        }

        [Theory]
        [MemberData(nameof(GetImageInfoForArtifact))]
        public async Task GivenImageInfo_WhenGetTemplateCollectionFromTemplateCollectionProvider_ACorrectTemplateCollectionShouldBeReturnedAsync(string imageReference, List<int> expectedTemplatesCounts)
        {
            ImageInfo imageInfo = ImageInfo.CreateFromImageReference(imageReference);
            _templateCollectionProvider = new TemplateCollectionProvider(imageInfo, MockClient, _cache, _defaultConfig);
            var templateCollection = await _templateCollectionProvider.GetTemplateCollectionAsync();
            Assert.Equal(expectedTemplatesCounts.Count(), templateCollection.Count());
            for (var i = 0; i < expectedTemplatesCounts.Count(); i++)
            {
                Assert.Equal(expectedTemplatesCounts[i], templateCollection[i].Count());
            }
        }

        [Theory]
        [MemberData(nameof(GetValidLayerInfo))]
        public async Task GivenValidLayerInfo_WhenGetLayerFromTemplateCollectionProvider_IfCached_ATemplateLayerShouldBeReturnedWithEmptyAcrAsync(string imageReference, string layerDigest, int expectedCounts)
        {
            await PullImageToCacheAsync(_defaultConfig);
            ImageInfo imageInfo = ImageInfo.CreateFromImageReference(imageReference);
            var newTemplateCollectionProvider = new TemplateCollectionProvider(imageInfo, _emptyClient, _cache, _defaultConfig);
            TemplateLayer templateLayer = (TemplateLayer)await newTemplateCollectionProvider.GetLayerAsync(layerDigest);
            Assert.Equal(expectedCounts, templateLayer.TemplateContent.Count());
        }

        [Theory]
        [MemberData(nameof(GetManifestInfoWithTag))]
        public async Task GivenManifestInfo_WhenGetManifestFromTemplateCollectionProvider_IfCached_AManifestShouldBeReturnedWithEmptyAcrAsync(string imageReference, string expectedManifestPath)
        {
            await PullImageToCacheAsync(_defaultConfig);
            ImageInfo imageInfo = ImageInfo.CreateFromImageReference(imageReference);
            var newTemplateCollectionProvider = new TemplateCollectionProvider(imageInfo, _emptyClient, _cache, _defaultConfig);
            var manifest = await newTemplateCollectionProvider.GetManifestAsync();
            Assert.Equal(File.ReadAllText(expectedManifestPath), JsonConvert.SerializeObject(manifest));
        }

        [Theory]
        [MemberData(nameof(GetImageInfoForArtifactWithTag))]
        public async Task GivenImageInfo_WhenGetTemplateCollectionFromTemplateCollectionProvider_IfCached_ATemplateCollectionShouldBeReturnedWithEmptyAcrAsync(string imageReference, List<int> expectedTemplatesCounts)
        {
            await PullImageToCacheAsync(_defaultConfig);
            ImageInfo imageInfo = ImageInfo.CreateFromImageReference(imageReference);
            var newTemplateCollectionProvider = new TemplateCollectionProvider(imageInfo, _emptyClient, _cache, _defaultConfig);
            var templateCollection = await newTemplateCollectionProvider.GetTemplateCollectionAsync();
            Assert.Equal(expectedTemplatesCounts.Count(), templateCollection.Count());
            for (var i = 0; i < expectedTemplatesCounts.Count(); i++)
            {
                Assert.Equal(expectedTemplatesCounts[i], templateCollection[i].Count());
            }
        }

        [Theory]
        [MemberData(nameof(GetImageInfoForArtifactWithTag))]
        public async Task GivenImageInfo_WhenGetTemplateCollectionFromTemplateCollectionProvider_IfCacheExpire_NotFoundExceptionWillBeThrown(string imageReference, List<int> expectedTemplatesCounts)
        {
            TemplateCollectionConfiguration config = new TemplateCollectionConfiguration() { ShortCacheTimeSpan = TimeSpan.FromSeconds(2) };
            await PullImageToCacheAsync(config);
            ImageInfo imageInfo = ImageInfo.CreateFromImageReference(imageReference);
            var newTemplateCollectionProvider = new TemplateCollectionProvider(imageInfo, _emptyClient, _cache, _defaultConfig);
            Thread.Sleep(2000);
            await Assert.ThrowsAsync<ImageNotFoundException>(() => newTemplateCollectionProvider.GetTemplateCollectionAsync());
            Assert.NotNull(expectedTemplatesCounts);
        }

        [Theory]
        [MemberData(nameof(GetImageInfoForArtifactWithTag))]
        public async Task GivenImageInfo_WhenGetTemplateCollectionFromTemplateCollectionProvider_IfExceedCacheLimitSize_CorrectTemplatesWillBeReturned_ButNotCached(string imageReference, List<int> expectedTemplatesCounts)
        {
            ImageInfo imageInfo = ImageInfo.CreateFromImageReference(imageReference);
            var smallCache = new MemoryCache(new MemoryCacheOptions() { SizeLimit = 100 });
            var newTemplateCollectionProvider = new TemplateCollectionProvider(imageInfo, MockClient, smallCache, _defaultConfig);
            var templateCollection = await newTemplateCollectionProvider.GetTemplateCollectionAsync();

            Assert.Equal(expectedTemplatesCounts.Count(), templateCollection.Count());
            for (var i = 0; i < expectedTemplatesCounts.Count(); i++)
            {
                Assert.Equal(expectedTemplatesCounts[i], templateCollection[i].Count());
            }

            Assert.Equal(0, smallCache.Count);
        }

        [Theory]
        [MemberData(nameof(GetDefaultTemplatesInfo))]
        public async Task GivenDefaultTemplateReference_WhenGetTemplateCollectionFromTemplateProvider_DefaultTemplateCollectionWillBeReturned(string imageReference, int expectedTemplatesCounts)
        {
            ImageInfo imageInfo = ImageInfo.CreateFromImageReference(imageReference);
            var newTemplateCollectionProvider = new TemplateCollectionProvider(imageInfo, _emptyClient, _cache, _defaultConfig);
            var templateCollection = await newTemplateCollectionProvider.GetTemplateCollectionAsync();
            Assert.True(expectedTemplatesCounts <= templateCollection.First().Count());
        }

        [Fact]
        public async Task GivenDefaultTemplateReference_WhenGetTemplateCollectionFromTemplateProvider_IfNotInitialized_ExceptionWillBeThrown()
        {
            string imageReference = "microsofthealth/fhirconverter:default";
            ImageInfo imageInfo = ImageInfo.CreateFromImageReference(imageReference);
            var emptyCache = new MemoryCache(new MemoryCacheOptions());
            var newTemplateCollectionProvider = new TemplateCollectionProvider(imageInfo, _emptyClient, emptyCache, _defaultConfig);
            await Assert.ThrowsAsync<DefaultTemplatesInitializeException>(async () => await newTemplateCollectionProvider.GetTemplateCollectionAsync());
        }

        private async Task PullImageToCacheAsync(TemplateCollectionConfiguration config)
        {
            var images = new List<string> { "mockregistry/testimagename:v1", "mockregistry/testimagename:v2", "mockregistry/testimagename:default" };

            foreach (var reference in images)
            {
                ImageInfo imageInfo = ImageInfo.CreateFromImageReference(reference);
                _templateCollectionProvider = new TemplateCollectionProvider(imageInfo, MockClient, _cache, config);
                _ = await _templateCollectionProvider.GetTemplateCollectionAsync();
            }
        }
    }
}
