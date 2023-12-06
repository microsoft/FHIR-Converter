// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Health.Fhir.TemplateManagement.ArtifactProviders;
using Microsoft.Health.Fhir.TemplateManagement.Factory;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Fhir.TemplateManagement.UnitTests.Factory
{
    public class ConvertDataTemplateCollectionProviderFactoryTests
    {
        private readonly TemplateCollectionConfiguration _config;
        private readonly IMemoryCache _cache;
        private readonly IConvertDataTemplateCollectionProviderFactory _factory;

        public ConvertDataTemplateCollectionProviderFactoryTests()
        {
            _config = new TemplateCollectionConfiguration();
            _cache = new MemoryCache(new MemoryCacheOptions());
            _factory = new ConvertDataTemplateCollectionProviderFactory(_cache, Options.Create(_config));
        }

        [Fact]
        public void GivenValidMockBlobClient_WhenCreateTemplateCollectionProvider_ThenBlobTemplateCollectionProviderReturned()
        {
            var blobClient = Substitute.For<BlobContainerClient>();
            blobClient.Name.Returns("test");

            var templateCollectionProvider = _factory.CreateTemplateCollectionProvider(blobClient);

            Assert.NotNull(templateCollectionProvider);
            Assert.True(templateCollectionProvider is BlobTemplateCollectionProvider);
            Assert.True(_cache.TryGetValue("storage-template-provider-test", out IConvertDataTemplateCollectionProvider cachedTemplateCollectionProvider));
            Assert.Equal(templateCollectionProvider, cachedTemplateCollectionProvider);

            // Call again to test cache
            cachedTemplateCollectionProvider = _factory.CreateTemplateCollectionProvider(blobClient);

            Assert.NotNull(cachedTemplateCollectionProvider);
            Assert.Equal(templateCollectionProvider, cachedTemplateCollectionProvider);
        }

        [Fact]
        public void GivenInvalidBlobClient_WhenCreateTemplateCollectionProvider_ThenBlobTemplateCollectionProviderReturned()
        {
            // Null blob container client
            Assert.Throws<ArgumentNullException>(() => _factory.CreateTemplateCollectionProvider(blobContainerClient: null));

            // Invalid name blob container client
            var blobClient = Substitute.For<BlobContainerClient>();
            Assert.Throws<ArgumentException>(() => _factory.CreateTemplateCollectionProvider(blobClient));
        }

        [Fact]
        public void GivenNoClientConfiguration_WhenCreateTemplateCollectionProvider_ThenDefaultTemplateCollectionProviderReturned()
        {
            var templateCollectionProvider = _factory.CreateTemplateCollectionProvider();

            Assert.NotNull(templateCollectionProvider);
            Assert.True(templateCollectionProvider is DefaultTemplateCollectionProvider);
            Assert.True(_cache.TryGetValue("default-template-provider", out IConvertDataTemplateCollectionProvider cachedTemplateCollectionProvider));
            Assert.Equal(templateCollectionProvider, cachedTemplateCollectionProvider);

            // Call again to test cache
            cachedTemplateCollectionProvider = _factory.CreateTemplateCollectionProvider();

            Assert.NotNull(cachedTemplateCollectionProvider);
            Assert.Equal(templateCollectionProvider, cachedTemplateCollectionProvider);
        }
    }
}
