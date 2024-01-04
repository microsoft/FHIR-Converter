// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DotLiquid;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Health.Fhir.Liquid.Converter.Utilities;
using Microsoft.Health.Fhir.TemplateManagement.Configurations;
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

        public BlobTemplateCollectionProvider(BlobContainerClient blobContainerClient, IMemoryCache templateCache, TemplateCollectionConfiguration templateConfiguration)
        {
            _blobContainerClient = blobContainerClient;
            _templateCache = templateCache;
            _templateCollectionConfiguration = templateConfiguration;
            _downloadRetryPolicy = Policy
                    .Handle<Exception>()
                    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromMilliseconds(10));
        }

        public async Task<List<Dictionary<string, Template>>> GetTemplateCollectionAsync(CancellationToken cancellationToken = default)
        {
            // read templates from cache if available
            if (_templateCache.TryGetValue(_blobTemplateCacheKey, out List<Dictionary<string, Template>> templateCache))
            {
                return templateCache;
            }

            var templateNames = await ListBlobsFlatListing(_blobContainerClient, _segmentSize, cancellationToken);

            var templates = new Dictionary<string, string>();

            foreach (var templateName in templateNames)
            {
                var template = await DownloadBlobToStringAsync(_blobContainerClient, templateName, cancellationToken);
                templates.Add(templateName, template);
            }

            var parsedtemplates = TemplateUtility.ParseTemplates(templates);

            var templateCollection = new List<Dictionary<string, Template>>();

            if (parsedtemplates.Any())
            {
                templateCollection.Add(parsedtemplates);
            }

            _templateCache.Set(_blobTemplateCacheKey, templateCollection, _templateCollectionConfiguration.ShortCacheTimeSpan);

            return templateCollection;
        }

        private static async Task<List<string>> ListBlobsFlatListing(BlobContainerClient blobContainerClient, int? segmentSize, CancellationToken ct)
        {
            var blobs = new List<string>();

            var resultSegment = blobContainerClient.GetBlobsAsync(default, default, null, ct)
                .AsPages(default, segmentSize);

            await foreach (Page<BlobItem> blobPage in resultSegment)
            {
                foreach (BlobItem blobItem in blobPage.Values)
                {
                    blobs.Add(blobItem.Name);
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
                return policyResponse.Result.Value.Content.ToString();
            }
            else
            {
                throw policyResponse.FinalException;
            }
        }
    }
}
