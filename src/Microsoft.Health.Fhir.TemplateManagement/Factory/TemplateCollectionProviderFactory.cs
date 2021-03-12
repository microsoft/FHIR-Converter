// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
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

        private void InitDefaultTemplates()
        {
            foreach (var templateInfo in Constants.DefaultTemplateInfo)
            {
                TemplateLayer templateLayer = TemplateLayer.ReadFromEmbeddedResource(templateInfo.Value.Item1);
                _templateCache.Set(templateInfo.Value.Item2, templateLayer, new MemoryCacheEntryOptions() { AbsoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration, Size = templateLayer.Size, Priority = Extensions.Caching.Memory.CacheItemPriority.NeverRemove });
                _templateCache.Set(templateLayer.Digest, templateLayer, new MemoryCacheEntryOptions() { AbsoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration, Size = templateLayer.Size, Priority = Extensions.Caching.Memory.CacheItemPriority.NeverRemove });
                if (templateInfo.Key == Constants.DefaultDataType)
                {
                    _templateCache.Set(ImageInfo.DefaultTemplateImageReference, templateLayer, new MemoryCacheEntryOptions() { AbsoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration, Size = templateLayer.Size, Priority = Extensions.Caching.Memory.CacheItemPriority.NeverRemove });
                }
            }
        }

        public void InitDefaultTemplates(string path)
        {
            path ??= Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.DefaultTemplateInfo.GetValueOrDefault(Constants.DefaultDataType).Item1);

            TemplateLayer defaultTemplateLayer = TemplateLayer.ReadFromFile(path);
            _templateCache.Set(ImageInfo.DefaultTemplateImageReference, defaultTemplateLayer, new MemoryCacheEntryOptions() { AbsoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration, Size = defaultTemplateLayer.Size, Priority = Extensions.Caching.Memory.CacheItemPriority.NeverRemove });
            _templateCache.Set(defaultTemplateLayer.Digest, defaultTemplateLayer, new MemoryCacheEntryOptions() { AbsoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration, Size = defaultTemplateLayer.Size, Priority = Extensions.Caching.Memory.CacheItemPriority.NeverRemove });
        }
    }
}
