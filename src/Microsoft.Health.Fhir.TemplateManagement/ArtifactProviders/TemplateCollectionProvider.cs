// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DotLiquid;
using EnsureThat;
using Microsoft.Azure.ContainerRegistry.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Health.Fhir.TemplateManagement.Client;
using Microsoft.Health.Fhir.TemplateManagement.Configurations;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Microsoft.Health.Fhir.TemplateManagement.Storage;

namespace Microsoft.Health.Fhir.TemplateManagement.ArtifactProviders
{
    public class TemplateCollectionProvider : OciArtifactProvider, ITemplateCollectionProvider
    {
        private readonly IMemoryCache _templateCache;
        private readonly TemplateCollectionConfiguration _configuration;
        private SemaphoreSlim _manifestSemaphore;
        private SemaphoreSlim _layerSemaphore;

        public TemplateCollectionProvider(ImageInfo imageInfo, IOciClient client, IMemoryCache templateCache, TemplateCollectionConfiguration configuration)
            : base(imageInfo, client)
        {
            EnsureArg.IsNotNull(imageInfo, nameof(imageInfo));
            EnsureArg.IsNotNull(client, nameof(client));
            EnsureArg.IsNotNull(templateCache, nameof(templateCache));
            EnsureArg.IsNotNull(configuration, nameof(configuration));

            _templateCache = templateCache;
            _configuration = configuration;
            _manifestSemaphore = new SemaphoreSlim(1, 1);
            _layerSemaphore = new SemaphoreSlim(1, 1);
        }

        public async Task<List<Dictionary<string, Template>>> GetTemplateCollectionAsync(CancellationToken cancellationToken = default)
        {
            if (ImageInfo.IsDefaultTemplate())
            {
                return GetTemplateCollectionFromDefaultTemplates();
            }

            List<Dictionary<string, Template>> result = new List<Dictionary<string, Template>>();
            var templateImage = await GetOciArtifactAsync(cancellationToken);
            var templateLayers = templateImage.Blobs;

            for (var layerNumber = templateLayers.Count - 1; layerNumber >= 0; layerNumber--)
            {
                if (templateLayers[layerNumber] is TemplateLayer templateLayer)
                {
                    result.Add(templateLayer.TemplateContent);

                    // The first layer is base layer. Others are user layers.
                    // Add base layer to long expiration cache.
                    if (layerNumber == 0)
                    {
                        AddToCache(templateLayer.Digest, templateLayer, templateLayer.Size, CachePolicy.LongExpire);
                        continue;
                    }

                    // Add user layers to short expiration cache
                    AddToCache(templateLayer.Digest, templateLayer, templateLayer.Size, CachePolicy.ShortExpire);
                }
                else
                {
                    throw new TemplateManagementException("Get templates failed");
                }
            }

            return result;
        }

        public override async Task<ManifestWrapper> GetManifestAsync(CancellationToken cancellationToken = default)
        {
            var cacheKey = ImageInfo.ImageReference;
            var cachePolicy = CachePolicy.ShortExpire;
            if (!string.IsNullOrEmpty(ImageInfo.Digest))
            {
                cachePolicy = CachePolicy.LongExpire;
            }

            var manifestInfo = _templateCache.Get(cacheKey) as ManifestWrapper;
            if (manifestInfo == null)
            {
                await _manifestSemaphore.WaitAsync(cancellationToken);
                try
                {
                    // try to read from cache one last time after entering the semaphoreslim
                    manifestInfo = _templateCache.Get(cacheKey) as ManifestWrapper;
                    if (manifestInfo == null)
                    {
                        manifestInfo = await base.GetManifestAsync(cancellationToken);
                        ValidateImageSize(manifestInfo, _configuration.TemplateCollectionSizeLimitMegabytes);
                        AddToCache(cacheKey, manifestInfo, Constants.ManifestObjectSizeInByte, cachePolicy);
                    }
                }
                finally
                {
                    _manifestSemaphore.Release();
                }
            }

            return manifestInfo;
        }

        public override async Task<ArtifactBlob> GetLayerAsync(string layerDigest, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNull(layerDigest, nameof(layerDigest));

            var oneTemplateLayer = _templateCache.Get(layerDigest) as TemplateLayer;
            if (oneTemplateLayer == null)
            {
                await _layerSemaphore.WaitAsync(cancellationToken);
                try
                {
                    // try to read from cache one last time after entering the semaphoreslim
                    oneTemplateLayer = _templateCache.Get(layerDigest) as TemplateLayer;
                    if (oneTemplateLayer == null)
                    {
                        var artifactsLayer = await base.GetLayerAsync(layerDigest, cancellationToken);
                        oneTemplateLayer = TemplateLayerParser.ParseArtifactsLayerToTemplateLayer(artifactsLayer as ArtifactBlob);
                        AddToCache(oneTemplateLayer.Digest, oneTemplateLayer, oneTemplateLayer.Size, CachePolicy.ShortExpire);
                    }
                }
                finally
                {
                    _layerSemaphore.Release();
                }
            }

            return oneTemplateLayer;
        }

        private List<Dictionary<string, Template>> GetTemplateCollectionFromDefaultTemplates()
        {
            string defaultImageReference = DefaultTemplateInfo.DefaultTemplateMap.GetValueOrDefault(ImageInfo.ImageReference)?.ImageReference;
            if (_templateCache.Get(defaultImageReference) is TemplateLayer oneTemplateLayer)
            {
                return new List<Dictionary<string, Template>> { oneTemplateLayer.TemplateContent };
            }

            throw new DefaultTemplatesInitializeException(TemplateManagementErrorCode.InitializeDefaultTemplateFailed, "Default templates not found.");
        }

        private void AddToCache(string key, object data, long size, CachePolicy policy)
        {
            _templateCache.GetOrCreate(key, entry =>
            {
                if (policy == CachePolicy.ShortExpire)
                {
                    entry.Priority = CacheItemPriority.Low;
                    entry.Size = size;
                    entry.SetAbsoluteExpiration(_configuration.ShortCacheTimeSpan);
                }
                else if (policy == CachePolicy.LongExpire)
                {
                    entry.Priority = CacheItemPriority.High;
                    entry.Size = size;
                    entry.SetAbsoluteExpiration(_configuration.LongCacheTimeSpan);
                }

                return data;
            });
        }

        private void ValidateImageSize(ManifestWrapper manifestInfo, int templateCollectionSizeLimitMegabytes)
        {
            long imageSize = 0;
            foreach (var oneLayer in manifestInfo.Layers)
            {
                imageSize += (long)oneLayer.Size;
            }

            if (imageSize / 1024f / 1024f > templateCollectionSizeLimitMegabytes)
            {
                throw new ImageTooLargeException(TemplateManagementErrorCode.ImageSizeTooLarge, $"Image size is larger than the size limitation: {templateCollectionSizeLimitMegabytes} Megabytes");
            }
        }
    }
}
