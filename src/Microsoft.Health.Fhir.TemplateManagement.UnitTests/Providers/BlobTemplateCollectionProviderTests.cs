// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Health.Fhir.TemplateManagement.ArtifactProviders;
using Microsoft.Health.Fhir.TemplateManagement.Configurations;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Moq;
using Xunit;

namespace Microsoft.Health.Fhir.TemplateManagement.UnitTests.Providers
{
    public class BlobTemplateCollectionProviderTests
    {
        [Fact]
        public async Task GivenBlobTemplateProvider_WhenGetTemplateCollectionFromTemplateProvider_ThenTemplatesAreReturned()
        {
            var templateConfiguration = new TemplateCollectionConfiguration();

            int templateCount = 1;
            var blobItemProperties = BlobsModelFactory.BlobItemProperties(accessTierInferred: true, contentLength: 100);
            var blobTemplateProvider = new BlobTemplateCollectionProvider(GetBlobContainerClientMock(templateCount, blobItemProperties), new MemoryCache(new MemoryCacheOptions()), templateConfiguration);

            var templateCollection = await blobTemplateProvider.GetTemplateCollectionAsync();

            Assert.Single(templateCollection);
            Assert.Equal(templateCount, templateCollection[0].Count);
        }

        [Fact]
        public async Task GivenBlobTemplateProviderWithLargeTemplates_WhenGetTemplateCollectionFromTemplateProvider_ThenExceptionThrown()
        {
            var templateConfiguration = new TemplateCollectionConfiguration();

            int templateCount = 2;
            var blobItemProperties = BlobsModelFactory.BlobItemProperties(accessTierInferred: true, contentLength: 10 * 1024 * 1024);
            var blobTemplateProvider = new BlobTemplateCollectionProvider(GetBlobContainerClientMock(templateCount, blobItemProperties), new MemoryCache(new MemoryCacheOptions()), templateConfiguration);

            var ex = await Assert.ThrowsAsync<TemplateCollectionExceedsSizeLimitException>(async() => await blobTemplateProvider.GetTemplateCollectionAsync());

            Assert.Equal(TemplateManagementErrorCode.BlobTemplateCollectionTooLarge, ex.TemplateManagementErrorCode);
        }

        [Fact]
        public async Task GivenBlobTemplateProviderWithEmptyTemplates_WhenGetTemplateCollectionFromTemplateProvider_ThenEmptyTemplatesAreReturned()
        {
            var templateConfiguration = new TemplateCollectionConfiguration();

            int templateCount = 1;
            var blobItemProperties = BlobsModelFactory.BlobItemProperties(accessTierInferred: true, contentLength: 0);
            var blobTemplateProvider = new BlobTemplateCollectionProvider(GetBlobContainerClientMock(templateCount, blobItemProperties, templateContent: string.Empty), new MemoryCache(new MemoryCacheOptions()), templateConfiguration);

            var templateCollection = await blobTemplateProvider.GetTemplateCollectionAsync();

            Assert.Single(templateCollection);
            Assert.Equal(templateCount, templateCollection[0].Count);

            blobTemplateProvider = new BlobTemplateCollectionProvider(GetBlobContainerClientMock(templateCount, blobItemProperties, templateContent: null), new MemoryCache(new MemoryCacheOptions()), templateConfiguration);

            templateCollection = await blobTemplateProvider.GetTemplateCollectionAsync();

            Assert.Single(templateCollection);
            Assert.Equal(templateCount, templateCollection[0].Count);
        }

        [Fact]
        public async Task GivenBlobTemplateProviderWithoutTemplates_WhenGetTemplateCollectionFromTemplateProvider_ThenEmptyTemplateCollectionReturned()
        {
            var templateConfiguration = new TemplateCollectionConfiguration();

            var blobTemplateProvider = new BlobTemplateCollectionProvider(GetBlobContainerClientMock(templateCount: 0), new MemoryCache(new MemoryCacheOptions()), templateConfiguration);

            var templateCollection = await blobTemplateProvider.GetTemplateCollectionAsync();

            Assert.Empty(templateCollection);
        }

        public static BlobContainerClient GetBlobContainerClientMock(
            int templateCount = 1,
            BlobItemProperties blobItemProperties = null,
            string templateContent = "sample_content")
        {
            var mock = new Mock<BlobContainerClient>();

            mock
                .Setup(x => x.GetBlobClient(It.IsAny<string>()))
                .Returns(GetBlobClientMock(templateContent));

            BlobItem[] blobs = new BlobItem[templateCount];

            for (int i = 0; i < templateCount; i++)
            {
                blobs[i] = BlobsModelFactory.BlobItem($"blob_name-{i}.liquid", properties: blobItemProperties);
            }

            Page<BlobItem> page = Page<BlobItem>.FromValues(blobs, null, Mock.Of<Response>());

            AsyncPageable<BlobItem> blobPages = AsyncPageable<BlobItem>.FromPages(new[] { page });

            mock
                .Setup(x => x.GetBlobsAsync(It.IsAny<BlobTraits>(), It.IsAny<BlobStates>(), It.IsAny<string>(), default))
                .Returns(blobPages);

            return mock.Object;
        }

        public static BlobClient GetBlobClientMock(string templateContent = null)
        {
            var mock = new Mock<BlobClient>();
            var mockBlobData = templateContent == null ? new BinaryData(new ReadOnlyMemory<byte>(null)) : new BinaryData(templateContent);

            var mockResponse = new Mock<Response>();

            mock
                .Setup(x => x.DownloadContentAsync(It.IsAny<CancellationToken>()))
                .Returns(
                    Task.FromResult(
                        Response.FromValue(
                            BlobsModelFactory.BlobDownloadResult(mockBlobData), mockResponse.Object)));

            return mock.Object;
        }
    }
}
