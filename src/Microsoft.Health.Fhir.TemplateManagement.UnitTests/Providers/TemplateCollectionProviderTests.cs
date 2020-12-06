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
using DotLiquid;
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
            TemplateLayer defaultTempalteLayer = TemplateLayer.ReadFromEmbeddedResource();
            _cache.Set(ImageInfo.DefaultTemplateImageReference, defaultTempalteLayer, new MemoryCacheEntryOptions() { AbsoluteExpiration = System.Runtime.Caching.ObjectCache.InfiniteAbsoluteExpiration, Size = 0 });

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
            yield return new object[] { "mockregistry/testimagename:v1", "sha256:ceda57333d3ab258176e6f5095f8c348994bc9f30a2db226980f223c62410d03", 817 };
            yield return new object[] { "mockregistry/testimagename:v1", "sha256:fac83d4cd496b0efd76db216568a53e1ff4f9571461deec8835cc4d2e171d063", 767 };
            yield return new object[] { "mockregistry/testimagename:v1", "sha256:ca1d8cff554ca9d7796500548c968f4985528af2676589de3a719048f266a3bc", 813 };
        }

        public static new IEnumerable<object[]> GetImageInfoForArtifact()
        {
            yield return new object[] { "mockregistry/testimagename:v1", new List<int> { 813, 817 } };
            yield return new object[] { "mockregistry/testimagename:v2", new List<int> { 767, 817 } };
            yield return new object[] { "mockregistry/testimagename:default", new List<int> { 817 } };
            yield return new object[] { "mockregistry/testimagename@sha256:cbd40a123aea6ef53c6608ad42ebed5c2befe2a7f8923142c04beed0d52c54d3", new List<int> { 813, 817 } };
            yield return new object[] { "mockregistry/testimagename@sha256:150caa8cc8f56008ef19b021888fb40e4160a20ed25762aab785ec3524c04c5e", new List<int> { 767, 817 } };
            yield return new object[] { "mockregistry/testimagename@sha256:204f03ecad424e408dcf28747add065edf9f1bfcf44da5f4edded841818337d2", new List<int> { 817 } };
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

        [Theory]
        [MemberData(nameof(GetValidLayerInfo))]
        public async Task GivenValidLayerInfo_WhenGetLayerFromTemplateCollectionProvider_ACorrectTemplateLayerShouldBeReturnedAsync(string imageReference, string layerDigest, int expectedCounts)
        {
            ImageInfo imageInfo = ImageInfo.CreateFromImageReference(imageReference);
            _templateCollectionProvider = new TemplateCollectionProvider(imageInfo, MockClient, _cache, _defaultConfig);
            TemplateLayer templateLayer = (TemplateLayer)await _templateCollectionProvider.GetLayerAsync(layerDigest);
            Assert.Equal(expectedCounts, ((Dictionary<string, Template>)templateLayer.Content).Count());
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
        public async Task GivenValidLayerInfo_WhenGetLayerFromTemplateCollectionProvider_IfCached_ATemplateLayerShouldBeReturnedWithEmptyACRAsync(string imageReference, string layerDigest, int expectedCounts)
        {
            await PullImageToCacheAsync(_defaultConfig);
            ImageInfo imageInfo = ImageInfo.CreateFromImageReference(imageReference);
            var newTemplateCollectionProvider = new TemplateCollectionProvider(imageInfo, _emptyClient, _cache, _defaultConfig);
            TemplateLayer templateLayer = (TemplateLayer)await newTemplateCollectionProvider.GetLayerAsync(layerDigest);
            Assert.Equal(expectedCounts, ((Dictionary<string, Template>)templateLayer.Content).Count());
        }

        [Theory]
        [MemberData(nameof(GetManifestInfoWithTag))]
        public async Task GivenManifestInfo_WhenGetManifestFromTemplateCollectionProvider_IfCached_AManifestShouldBeReturnedWithEmptyACRAsync(string imageReference, string expectedManifestPath)
        {
            await PullImageToCacheAsync(_defaultConfig);
            ImageInfo imageInfo = ImageInfo.CreateFromImageReference(imageReference);
            var newTemplateCollectionProvider = new TemplateCollectionProvider(imageInfo, _emptyClient, _cache, _defaultConfig);
            var manifest = await newTemplateCollectionProvider.GetManifestAsync();
            Assert.Equal(File.ReadAllText(expectedManifestPath), JsonConvert.SerializeObject(manifest));
        }

        [Theory]
        [MemberData(nameof(GetImageInfoForArtifactWithTag))]
        public async Task GivenImageInfo_WhenGetTemplateCollectionFromTemplateCollectionProvider_IfCached_ATemplateCollectionShouldBeReturnedWithEmptyACRAsync(string imageReference, List<int> expectedTemplatesCounts)
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
        public async Task GivenImageInfo_WhenGetTemplateCollectionFromTemplateCollectionProvider_IfCacheExpire_NotFoundExceptionWillBeThrown(string imageReference, object _)
        {
            TemplateCollectionConfiguration config = new TemplateCollectionConfiguration() { ShortCacheTimeSpan = TimeSpan.FromSeconds(2) };
            await PullImageToCacheAsync(config);
            ImageInfo imageInfo = ImageInfo.CreateFromImageReference(imageReference);
            var newTemplateCollectionProvider = new TemplateCollectionProvider(imageInfo, _emptyClient, _cache, _defaultConfig);
            Thread.Sleep(2000);
            await Assert.ThrowsAsync<ImageNotFoundException>(() => newTemplateCollectionProvider.GetTemplateCollectionAsync());
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

        [Fact]
        public async Task GivenDefaultTemplateReference_WhenGetTemplateCollectionFromTemplateProvider_DefaultTemplateCollectionWillBeReturned()
        {
            string imageReference = ImageInfo.DefaultTemplateImageReference;
            ImageInfo imageInfo = ImageInfo.CreateFromImageReference(imageReference);
            var newTemplateCollectionProvider = new TemplateCollectionProvider(imageInfo, _emptyClient, _cache, _defaultConfig);
            var templateCollection = await newTemplateCollectionProvider.GetTemplateCollectionAsync();
            Assert.Equal(838, templateCollection.First().Count());
        }

        [Fact]
        public async Task GivenDefaultTemplateReference_WhenGetTemplateCollectionFromTemplateProvider_IfNotInitialized_ExceptionWillBeThrown()
        {
            string imageReference = ImageInfo.DefaultTemplateImageReference;
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
