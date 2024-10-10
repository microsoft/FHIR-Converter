// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DotLiquid;
using EnsureThat;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Health.Fhir.Liquid.Converter.Utilities;
using Microsoft.Health.Fhir.TemplateManagement.Configurations;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Polly;
using Polly.Retry;

namespace Microsoft.Health.Fhir.TemplateManagement.ArtifactProviders
{
    public class BlobTemplateCollectionProvider : IConvertDataTemplateCollectionProvider
    {
        private BlobContainerClient _blobContainerClient;

        private readonly IMemoryCache _templateCache;

        private readonly TemplateCollectionConfiguration _templateCollectionConfiguration;

        private readonly string _blobTemplateCacheKey = "cached-blob-templates";

        private readonly int _segmentSize = 100;

        private readonly AsyncRetryPolicy _downloadRetryPolicy;

        private readonly int _maxParallelism = 50;

        private readonly int _maxTemplateCollectionSizeInBytes;

        public BlobTemplateCollectionProvider(BlobContainerClient blobContainerClient, IMemoryCache templateCache, TemplateCollectionConfiguration templateConfiguration)
        {
            _blobContainerClient = EnsureArg.IsNotNull(blobContainerClient, nameof(blobContainerClient));
            _templateCache = EnsureArg.IsNotNull(templateCache, nameof(templateCache));
            _templateCollectionConfiguration = EnsureArg.IsNotNull(templateConfiguration, nameof(templateConfiguration));
            _downloadRetryPolicy = Policy
                    .Handle<Exception>()
                    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromMilliseconds(10));

            _maxTemplateCollectionSizeInBytes = _templateCollectionConfiguration.TemplateCollectionSizeLimitMegabytes * 1024 * 1024;
        }

        public async Task<List<Dictionary<string, Template>>> GetTemplateCollectionAsync(CancellationToken cancellationToken = default)
        {
            // read templates from cache if available
            if (_templateCache.TryGetValue(_blobTemplateCacheKey, out List<Dictionary<string, Template>> templateCache))
            {
                return templateCache;
            }

            var templateNames = await ListBlobsFlatListing(_blobContainerClient, _segmentSize, cancellationToken);

            var templates = new ConcurrentDictionary<string, string>();

            // download blobs in parallel
            var semaphore = new SemaphoreSlim(_maxParallelism);
            var downloadTasks = new List<Task>();

            foreach (var templateName in templateNames)
            {
                await semaphore.WaitAsync();

                downloadTasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var template = await DownloadBlobToStringAsync(_blobContainerClient, templateName, cancellationToken);
                        templates.TryAdd(templateName, template);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }

            await Task.WhenAll(downloadTasks);

            var parsedtemplates = TemplateUtility.ParseTemplates(templates);

            var templateCollection = new List<Dictionary<string, Template>>();

            if (parsedtemplates.Any())
            {
                templateCollection.Add(parsedtemplates);
            }

            _templateCache.Set(_blobTemplateCacheKey, templateCollection, _templateCollectionConfiguration.ShortCacheTimeSpan);

            return templateCollection;
        }

        private async Task<List<string>> ListBlobsFlatListing(BlobContainerClient blobContainerClient, int? segmentSize, CancellationToken ct)
        {
            var blobs = new List<string>();

            var resultSegment = blobContainerClient.GetBlobsAsync(default, default, null, ct)
                .AsPages(default, segmentSize);

            long totalBlobsSize = 0;

            await foreach (Page<BlobItem> blobPage in resultSegment)
            {
                foreach (BlobItem blobItem in blobPage.Values)
                {
                    blobs.Add(blobItem.Name);

                    totalBlobsSize += blobItem.Properties.ContentLength ?? 0;

                    if (totalBlobsSize > _maxTemplateCollectionSizeInBytes)
                    {
                        throw new TemplateCollectionExceedsSizeLimitException(TemplateManagementErrorCode.BlobTemplateCollectionTooLarge, $"Total blob template collection size is larger than the size limit: {_templateCollectionConfiguration.TemplateCollectionSizeLimitMegabytes}MB.");
                    }
                }
            }

            return blobs;
        }

        public async Task<string> DownloadBlobToStringAsync(BlobContainerClient blobContainerClient, string blobName, CancellationToken ct)
        {
            var blobClient = blobContainerClient.GetBlobClient(blobName);

            PolicyResult<Response<BlobDownloadResult>> policyResponse = await _downloadRetryPolicy.ExecuteAndCaptureAsync(async () => await blobClient.DownloadContentAsync(ct));

            if (policyResponse.FinalException == null)
            {
                var contentBytes = policyResponse.Result.Value.Content.ToArray();
                return Encoding.UTF8.GetString(contentBytes);
            }

            throw policyResponse.FinalException;
        }
    }
}
