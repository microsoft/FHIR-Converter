// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Net.Http.Headers;
using System.Runtime.Caching;
using EnsureThat;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Health.Fhir.TemplateManagement.ArtifactProviders;
using Microsoft.Health.Fhir.TemplateManagement.Client;
using Microsoft.Health.Fhir.TemplateManagement.Configurations;
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

        public IOciArtifactProvider CreateProvider(string imageReference, string token)
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
            var client = new AcrClient(imageInfo.Registry, token);
            return new TemplateCollectionProvider(imageInfo, client, _templateCache, _configuration);
        }

        private void InitDefaultTemplates()
        {
            var memoryOption = new MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration,
                Priority = Extensions.Caching.Memory.CacheItemPriority.NeverRemove,
            };
            foreach (var templateInfo in DefaultTemplateInfo.DefaultTemplateMap)
            {
                TemplateLayer templateLayer = TemplateLayer.ReadFromEmbeddedResource(templateInfo.Value.TemplatePath);
                memoryOption.SetSize(templateLayer.Size);
                _templateCache.Set(templateInfo.Value.ImageReference, templateLayer, memoryOption);
                _templateCache.Set(templateLayer.Digest, templateLayer, memoryOption);
            }
        }

        public void InitDefaultTemplates(DefaultTemplateInfo templateInfo)
        {
            TemplateLayer defaultTemplateLayer = TemplateLayer.ReadFromFile(templateInfo.TemplatePath);
            var memoryOption = new MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration,
                Size = defaultTemplateLayer.Size,
                Priority = Extensions.Caching.Memory.CacheItemPriority.NeverRemove,
            };
            _templateCache.Set(templateInfo.ImageReference, defaultTemplateLayer, memoryOption);
            _templateCache.Set(defaultTemplateLayer.Digest, defaultTemplateLayer, memoryOption);
        }
    }
}
