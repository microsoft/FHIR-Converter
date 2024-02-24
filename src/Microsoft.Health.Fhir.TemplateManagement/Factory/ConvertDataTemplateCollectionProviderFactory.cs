// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;
using EnsureThat;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Health.Fhir.TemplateManagement.ArtifactProviders;
using Microsoft.Health.Fhir.TemplateManagement.Configurations;

namespace Microsoft.Health.Fhir.TemplateManagement.Factory
{
    public class ConvertDataTemplateCollectionProviderFactory : IConvertDataTemplateCollectionProviderFactory
    {
        private readonly IMemoryCache _memoryCache;
        private readonly TemplateCollectionConfiguration _templateCollectionConfiguration;

        private readonly string _defaultTemplateProviderCacheKey = "default-template-provider";
        private readonly string _storageTemplateProviderCachePrefix = "storage-template-provider-";

        public ConvertDataTemplateCollectionProviderFactory(IMemoryCache memoryCache, IOptions<TemplateCollectionConfiguration> templateCollectionConfiguration)
        {
            _memoryCache = EnsureArg.IsNotNull(memoryCache, nameof(memoryCache));
            _templateCollectionConfiguration = EnsureArg.IsNotNull(templateCollectionConfiguration?.Value, nameof(templateCollectionConfiguration));
        }

        /// <summary>
        /// Returns the appropriate template collection provider based on the configuration.
        /// E.g., If storage account configuration is provided, then a blob template collection provider is returned.
        /// Otherwise, the default template collection provider is returned.
        /// </summary>
        /// <returns>Returns a template collection provider based on the configuration.</returns>
        public IConvertDataTemplateCollectionProvider CreateTemplateCollectionProvider()
        {
            var templateHostingConfiguration = _templateCollectionConfiguration.TemplateHostingConfiguration;

            if (templateHostingConfiguration?.StorageAccountConfiguration?.ContainerUrl != null)
            {
                return CreateBlobTemplateCollectionProvider(templateHostingConfiguration.StorageAccountConfiguration);
            }

            return CreateDefaultTemplateCollectionProvider();
        }

        /// <summary>
        /// Returns the default template collection provider, i.e., template provider that references the default templates packaged within the project.
        /// </summary>
        /// <returns>Returns the default template collection provider, <see cref="DefaultTemplateCollectionProvider">.</returns>
        private IConvertDataTemplateCollectionProvider CreateDefaultTemplateCollectionProvider()
        {
            var cacheKey = _defaultTemplateProviderCacheKey;
            if (_memoryCache.TryGetValue(cacheKey, out var templateProviderCache))
            {
                return (IConvertDataTemplateCollectionProvider)templateProviderCache;
            }

            var templateProvider = new DefaultTemplateCollectionProvider(_memoryCache, _templateCollectionConfiguration);
            _memoryCache.Set(cacheKey, templateProvider);
            return templateProvider;
        }

        /// <summary>
        /// Returns a blob template collection provider, i.e., template provider that references the templates stored in a blob container.
        /// </summary>
        /// <param name="storageAccountConfiguration">Storage account configuration containing information of the blob container to load the templates from.</param>
        /// <returns>Returns a blob template collection provider, <see cref="BlobTemplateCollectionProvider">.</returns>
        private IConvertDataTemplateCollectionProvider CreateBlobTemplateCollectionProvider(StorageAccountConfiguration storageAccountConfiguration)
        {
            EnsureArg.IsNotNull(storageAccountConfiguration, nameof(storageAccountConfiguration));
            EnsureArg.IsNotNull(storageAccountConfiguration.ContainerUrl, nameof(storageAccountConfiguration.ContainerUrl));

            TokenCredential tokenCredential = new DefaultAzureCredential();
            var blobContainerClient = new BlobContainerClient(storageAccountConfiguration.ContainerUrl, tokenCredential);

            var cacheKey = _storageTemplateProviderCachePrefix + blobContainerClient.Name;
            if (_memoryCache.TryGetValue(cacheKey, out var templateProviderCache))
            {
                return (IConvertDataTemplateCollectionProvider)templateProviderCache;
            }

            var templateProvider = new BlobTemplateCollectionProvider(blobContainerClient, _memoryCache, _templateCollectionConfiguration);
            _memoryCache.Set(cacheKey, templateProvider);
            return templateProvider;
        }
    }
}
