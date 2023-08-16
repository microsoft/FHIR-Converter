// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Azure.Storage.Blobs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Health.Fhir.TemplateManagement.ArtifactProviders;
using Microsoft.Health.Fhir.TemplateManagement.Models;

namespace Microsoft.Health.Fhir.TemplateManagement.Factory
{
    public class StorageTemplateProviderFactory : ITemplateProviderFactory
    {
        private BlobContainerClient _blobContainerClient;

        private IMemoryCache _memoryCache;

        private TemplateCollectionConfiguration _templateCollectionConfiguration;

        private string _templateProviderCachePrefix = "storage-template-provider-";

        public StorageTemplateProviderFactory(BlobContainerClient blobContainerClient, IMemoryCache memoryCache, TemplateCollectionConfiguration templateCollectionConfiguration)
        {
            _blobContainerClient = blobContainerClient;
            _memoryCache = memoryCache;
            _templateCollectionConfiguration = templateCollectionConfiguration;
        }

        public IConvertDataTemplateProvider GetTemplateProvider()
        {
            if (_memoryCache.TryGetValue(_templateProviderCachePrefix + _blobContainerClient.Name, out var templateProviderCache))
            {
                return (IConvertDataTemplateProvider)templateProviderCache;
            }

            var templateProvider = new BlobTemplateProvider(_blobContainerClient, _memoryCache, _templateCollectionConfiguration);
            _memoryCache.CreateEntry(_templateProviderCachePrefix + _blobContainerClient.Name).Value = templateProvider;
            return templateProvider;
        }
    }
}
