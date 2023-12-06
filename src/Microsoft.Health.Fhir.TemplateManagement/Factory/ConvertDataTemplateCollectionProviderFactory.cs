// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Azure.Storage.Blobs;
using EnsureThat;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Health.Fhir.TemplateManagement.ArtifactProviders;
using Microsoft.Health.Fhir.TemplateManagement.Models;

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
        /// Returns the default template collection provider, i.e., template provider that references the default templates packaged within the project.
        /// </summary>
        /// <returns>Returns the default template collection provider, <see cref="DefaultTemplateCollectionProvider">.</returns>
        public IConvertDataTemplateCollectionProvider CreateTemplateCollectionProvider()
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
        /// <param name="blobContainerClient">Blob container client for the blob container to load the templates from.</param>
        /// <returns>Returns a blob template collection provider, <see cref="BlobTemplateCollectionProvider">.</returns>
        public IConvertDataTemplateCollectionProvider CreateTemplateCollectionProvider(BlobContainerClient blobContainerClient)
        {
            EnsureArg.IsNotNull(blobContainerClient, nameof(blobContainerClient));
            EnsureArg.IsNotNullOrWhiteSpace(blobContainerClient.Name, nameof(blobContainerClient.Name));

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
