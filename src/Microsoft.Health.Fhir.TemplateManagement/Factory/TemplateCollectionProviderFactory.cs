// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using EnsureThat;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Health.Fhir.TemplateManagement.ArtifactProviders;
using Microsoft.Health.Fhir.TemplateManagement.Client;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Models;

namespace Microsoft.Health.Fhir.TemplateManagement
{
    public class TemplateCollectionProviderFactory : ITemplateCollectionProviderFactory
    {
        private readonly IMemoryCache _templateCache;

        private readonly TemplateCollectionConfiguration _configuration;

        public TemplateCollectionProviderFactory(IMemoryCache cache, IOptions<TemplateCollectionConfiguration> configuration)
        {
            EnsureArg.IsNotNull(cache, nameof(cache));
            EnsureArg.IsNotNull(configuration, nameof(configuration));

            _templateCache = cache;
            _configuration = configuration.Value;

            InitDefaultTemplates();
        }

        public IOCIArtifactProvider CreateProvider(string imageReference, string token)
        {
            EnsureArg.IsNotNull(imageReference, nameof(imageReference));
            EnsureArg.IsNotNull(token, nameof(token));

            return CreateTemplateCollectionProvider(imageReference, token);
        }

        public ITemplateCollectionProvider CreateTemplateCollectionProvider(string imageReference, string token)
        {
            // TODO: different default template for ccda
            EnsureArg.IsNotNull(imageReference, nameof(imageReference));
            EnsureArg.IsNotNull(token, nameof(token));

            if (!ImageInfo.IsDefaultTemplateImageReference(imageReference)
                    && !AuthenticationHeaderValue.TryParse(token, out _))
            {
                throw new ContainerRegistryAuthenticationException(TemplateManagementErrorCode.RegistryUnauthorized, "Invalid authentication token");
            }

            ImageInfo imageInfo = ImageInfo.CreateFromImageReference(imageReference);
            var client = new ACRClient(imageInfo.Registry, token);
            return new TemplateCollectionProvider(imageInfo, client, _templateCache, _configuration);
        }

        public void InitDefaultTemplates()
        {
            TemplateLayer hl7v2DefaultTemplateLayer = TemplateLayer.ReadFromEmbeddedResource(Constants.Hl7v2DefaultTemplatePath);
            _templateCache.Set(ImageInfo.DefaultTemplateImageReference, hl7v2DefaultTemplateLayer, new MemoryCacheEntryOptions() { AbsoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration, Size = hl7v2DefaultTemplateLayer.Size, Priority = Extensions.Caching.Memory.CacheItemPriority.NeverRemove });
            _templateCache.Set(ImageInfo.Hl7v2TemplateImageReference, hl7v2DefaultTemplateLayer, new MemoryCacheEntryOptions() { AbsoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration, Size = hl7v2DefaultTemplateLayer.Size, Priority = Extensions.Caching.Memory.CacheItemPriority.NeverRemove });
            _templateCache.Set(hl7v2DefaultTemplateLayer.Digest, hl7v2DefaultTemplateLayer, new MemoryCacheEntryOptions() { AbsoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration, Size = hl7v2DefaultTemplateLayer.Size, Priority = Extensions.Caching.Memory.CacheItemPriority.NeverRemove });

            TemplateLayer cdaDefaultTemplateLayer = TemplateLayer.ReadFromEmbeddedResource(Constants.CdaDefaultTemplatePath);
            _templateCache.Set(ImageInfo.CcdaTemplateImageReference, cdaDefaultTemplateLayer, new MemoryCacheEntryOptions() { AbsoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration, Size = cdaDefaultTemplateLayer.Size, Priority = Extensions.Caching.Memory.CacheItemPriority.NeverRemove });
            _templateCache.Set(cdaDefaultTemplateLayer.Digest, cdaDefaultTemplateLayer, new MemoryCacheEntryOptions() { AbsoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration, Size = cdaDefaultTemplateLayer.Size, Priority = Extensions.Caching.Memory.CacheItemPriority.NeverRemove });
        }

        public void InitDefaultTemplates(string path)
        {
            TemplateLayer defaultTemplateLayer = TemplateLayer.ReadFromFile(path);
            _templateCache.Set(ImageInfo.DefaultTemplateImageReference, defaultTemplateLayer, new MemoryCacheEntryOptions() { AbsoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration, Size = defaultTemplateLayer.Size, Priority = Extensions.Caching.Memory.CacheItemPriority.NeverRemove });
            _templateCache.Set(defaultTemplateLayer.Digest, defaultTemplateLayer, new MemoryCacheEntryOptions() { AbsoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration, Size = defaultTemplateLayer.Size, Priority = Extensions.Caching.Memory.CacheItemPriority.NeverRemove });
        }
    }
}
