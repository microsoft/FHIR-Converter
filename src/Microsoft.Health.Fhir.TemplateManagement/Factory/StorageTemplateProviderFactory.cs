// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Health.Fhir.TemplateManagement.ArtifactProviders;

namespace Microsoft.Health.Fhir.TemplateManagement.Factory
{
    public class StorageTemplateProviderFactory : ITemplateProviderFactory
    {
        private BlobContainerClient _blobContainerClient;

        private IMemoryCache _memoryCache;

        private string _templateProviderCachePrefix = "storage-template-provider-";

        public StorageTemplateProviderFactory(BlobContainerClient blobContainerClient, IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
            _blobContainerClient = blobContainerClient;
        }

        public IConvertDataTemplateProvider GetContainerRegistryTemplateProvider()
        {
            throw new NotImplementedException();
        }

        public IConvertDataTemplateProvider GetDefaultTemplateProvider()
        {
            throw new NotImplementedException();
        }

        public IConvertDataTemplateProvider GetStorageContainerTemplateProvider()
        {
            if (_memoryCache.TryGetValue(_templateProviderCachePrefix + _blobContainerClient.Name, out var templateProviderCache) == true)
            {
                return (IConvertDataTemplateProvider)templateProviderCache;
            }

            var templateProvider = new BlobTemplateProvider(_blobContainerClient, _memoryCache);
            _memoryCache.CreateEntry(_templateProviderCachePrefix + _blobContainerClient.Name).Value = new BlobTemplateProvider(_blobContainerClient, _memoryCache);
            return templateProvider;
        }
    }
}
