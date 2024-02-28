// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Health.Fhir.TemplateManagement.ArtifactProviders;
using Microsoft.Health.Fhir.TemplateManagement.Configurations;
using Microsoft.Health.Fhir.TemplateManagement.Factory;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Fhir.TemplateManagement.UnitTests.Factory
{
    public class ConvertDataTemplateCollectionProviderFactoryTests
    {
        [Fact]
        public void GivenValidStorageAccountConfiguration_WhenCreateTemplateCollectionProvider_ThenBlobTemplateCollectionProviderReturned()
        {
            var config = new TemplateCollectionConfiguration()
            {
                TemplateHostingConfiguration = new TemplateHostingConfiguration()
                {
                    StorageAccountConfiguration = new StorageAccountConfiguration()
                    {
                        ContainerUrl = new Uri("https://test.blob.core.windows.net/test"),
                    },
                },
            };

            var cache = new MemoryCache(new MemoryCacheOptions());
            var factory = new ConvertDataTemplateCollectionProviderFactory(cache, Options.Create(config));

            var templateCollectionProvider = factory.CreateTemplateCollectionProvider();

            Assert.NotNull(templateCollectionProvider);
            Assert.True(templateCollectionProvider is BlobTemplateCollectionProvider);
            Assert.True(cache.TryGetValue("storage-template-provider-test", out IConvertDataTemplateCollectionProvider cachedTemplateCollectionProvider));
            Assert.Equal(templateCollectionProvider, cachedTemplateCollectionProvider);

            // Call again to test cache
            cachedTemplateCollectionProvider = factory.CreateTemplateCollectionProvider();

            Assert.NotNull(cachedTemplateCollectionProvider);
            Assert.Equal(templateCollectionProvider, cachedTemplateCollectionProvider);
        }

        [Fact]
        public void GivenEmptyStorageAccountConfiguration_WhenCreateTemplateCollectionProvider_ThenDefaultTemplateProviderReturned()
        {
            var config = new TemplateCollectionConfiguration()
            {
                TemplateHostingConfiguration = new TemplateHostingConfiguration()
                {
                    StorageAccountConfiguration = new StorageAccountConfiguration()
                    {
                        ContainerUrl = null,
                    },
                },
            };

            var cache = new MemoryCache(new MemoryCacheOptions());
            var factory = new ConvertDataTemplateCollectionProviderFactory(cache, Options.Create(config));

            var templateCollectionProvider = factory.CreateTemplateCollectionProvider();

            Assert.NotNull(templateCollectionProvider);
            Assert.True(templateCollectionProvider is DefaultTemplateCollectionProvider);
        }

        [Fact]
        public void GivenNoTemplateHostingConfiguration_WhenCreateTemplateCollectionProvider_ThenDefaultTemplateCollectionProviderReturned()
        {
            var config = new TemplateCollectionConfiguration();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var factory = new ConvertDataTemplateCollectionProviderFactory(cache, Options.Create(config));

            var templateCollectionProvider = factory.CreateTemplateCollectionProvider();

            Assert.NotNull(templateCollectionProvider);
            Assert.True(templateCollectionProvider is DefaultTemplateCollectionProvider);
            Assert.True(cache.TryGetValue("default-template-provider", out IConvertDataTemplateCollectionProvider cachedTemplateCollectionProvider));
            Assert.Equal(templateCollectionProvider, cachedTemplateCollectionProvider);

            // Call again to test cache
            cachedTemplateCollectionProvider = factory.CreateTemplateCollectionProvider();

            Assert.NotNull(cachedTemplateCollectionProvider);
            Assert.Equal(templateCollectionProvider, cachedTemplateCollectionProvider);
        }
    }
}
