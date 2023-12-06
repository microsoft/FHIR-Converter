// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Health.Fhir.TemplateManagement.ArtifactProviders;
using Microsoft.Health.Fhir.TemplateManagement.Factory;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.FunctionalTests
{
    public class TemplateCollectionProviderTestFixture : IAsyncLifetime
    {
        private readonly TemplateCollectionConfiguration _config;
        private readonly IMemoryCache _cache;
        private readonly IConvertDataTemplateCollectionProviderFactory _convertDataTemplateCollectionProviderFactory;

        public TemplateCollectionProviderTestFixture()
        {
            _config = new TemplateCollectionConfiguration();
            _cache = new MemoryCache(new MemoryCacheOptions());
            _convertDataTemplateCollectionProviderFactory = new ConvertDataTemplateCollectionProviderFactory(_cache, Options.Create(_config));
        }

        public IConvertDataTemplateCollectionProvider ConvertDataTemplateCollectionProvider { get; set; }

        public ITemplateProvider TemplateProvider { get; set; }

        // Update this property to test against a template collection within a storage account container.
        // Note - If using custom templates within a storage account, ensure test cases are updated to reflect the new templates.
        // e.g., If the root templates are present within folders (e.g. Hl7v2/), ensure 'rootTemplate' test parameter is reflecting the new path (e.g., Hl7v2/ADT_A01).
        public string TemplateCollectionStorageContainerUri { get; set; }

        public virtual async Task InitializeAsync()
        {
            ConvertDataTemplateCollectionProvider = GetTemplateCollectionProvider();

            var templateCollection = await ConvertDataTemplateCollectionProvider.GetTemplateCollectionAsync(CancellationToken.None);

            TemplateProvider = new TemplateProvider(templateCollection, isDefaultTemplateProvider: ConvertDataTemplateCollectionProvider is DefaultTemplateCollectionProvider);
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        private IConvertDataTemplateCollectionProvider GetTemplateCollectionProvider()
        {
            // If no storage account container URI is specified, the default template collection provider is used.
            if (string.IsNullOrEmpty(TemplateCollectionStorageContainerUri))
            {
                return _convertDataTemplateCollectionProviderFactory.CreateTemplateCollectionProvider();
            }

            TokenCredential tokenCredential = new DefaultAzureCredential();
            var blobContainerClient = new BlobContainerClient(new Uri(TemplateCollectionStorageContainerUri), tokenCredential);

            return _convertDataTemplateCollectionProviderFactory.CreateTemplateCollectionProvider(blobContainerClient);
        }
    }
}
