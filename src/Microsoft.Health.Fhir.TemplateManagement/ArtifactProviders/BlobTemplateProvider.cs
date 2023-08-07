// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DotLiquid;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Health.Fhir.Liquid.Converter.Utilities;

namespace Microsoft.Health.Fhir.TemplateManagement.ArtifactProviders
{
    public class BlobTemplateProvider : IConvertDataTemplateProvider
    {
        private BlobContainerClient _blobContainerClient;

        private readonly IMemoryCache _templateCache;

        private readonly string _blobTemplateCacheKey = "cached-blob-templates";

        private readonly int _segmentSize = 100;

        public BlobTemplateProvider(BlobContainerClient blobContainerClient, IMemoryCache templateCache)
        {
            _blobContainerClient = blobContainerClient;
            _templateCache = templateCache;
        }

        public async Task<List<Dictionary<string, Template>>> GetTemplateCollectionAsync(CancellationToken cancellationToken = default)
        {
            // read templates from cache if available
            if (_templateCache.TryGetValue(_blobTemplateCacheKey, out var templateCache))
            {
                return (List<Dictionary<string, Template>>)templateCache;
            }

            var templateLookup = new Dictionary<string, Template>();
            var templateNames = await ListBlobsFlatListing(_blobContainerClient, _segmentSize);

            foreach (var templateName in templateNames)
            {
                var template = await DownloadBlobToStringAsync(_blobContainerClient, templateName);
                var renamedTemplate = TrimBlobName(templateName);
                Template liquidTemplate = TemplateUtility.ParseTemplate(renamedTemplate, template);
                templateLookup.Add(renamedTemplate, liquidTemplate);
            }

            var templates = new List<Dictionary<string, Template>>
            {
                templateLookup,
            };

            _templateCache.Set(_blobTemplateCacheKey, templates);

            return templates;
        }

        private static async Task<List<string>> ListBlobsFlatListing(BlobContainerClient blobContainerClient, int? segmentSize)
        {
            var blobs = new List<string>();

            var resultSegment = blobContainerClient.GetBlobsAsync()
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

        public static async Task<string> DownloadBlobToStringAsync(BlobContainerClient blobContainerClient, string blobName)
        {
            var blobClient = blobContainerClient.GetBlobClient(blobName);
            BlobDownloadResult downloadResult = await blobClient.DownloadContentAsync();
            return downloadResult.Content.ToString();
        }

        // TODO: eventually want to remove this and create a common utility for ACR and storage templates
        private string TrimBlobName(string templateName)
        {
            return templateName
                .Replace("Hl7v2/_", string.Empty)
                .Replace("Hl7v2/", string.Empty)
                .Replace(".liquid", string.Empty)
                .Replace(".json", string.Empty)
                .Replace("/_", "/");
        }
    }
}
