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
            var blobTemplateProvider = new BlobTemplateCollectionProvider(GetBlobContainerClientMock(templateCount), new MemoryCache(new MemoryCacheOptions()), templateConfiguration);

            var templateCollection = await blobTemplateProvider.GetTemplateCollectionAsync();

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

        public static BlobContainerClient GetBlobContainerClientMock(int templateCount = 1)
        {
            var mock = new Mock<BlobContainerClient>();

            mock
                .Setup(x => x.GetBlobClient(It.IsAny<string>()))
                .Returns(GetBlobClientMock());

            BlobItem[] blobs = new BlobItem[templateCount];

            for (int i = 0; i < templateCount; i++)
            {
                blobs[i] = BlobsModelFactory.BlobItem($"blob_name-{i}.liquid");
            }

            Page<BlobItem> page = Page<BlobItem>.FromValues(blobs, null, Mock.Of<Response>());

            AsyncPageable<BlobItem> blobPages = AsyncPageable<BlobItem>.FromPages(new[] { page });

            mock
                .Setup(x => x.GetBlobsAsync(It.IsAny<BlobTraits>(), It.IsAny<BlobStates>(), It.IsAny<string>(), default))
                .Returns(blobPages);

            return mock.Object;
        }

        public static BlobClient GetBlobClientMock()
        {
            var mock = new Mock<BlobClient>();

            var mockResponse = new Mock<Response>();

            mock
                .Setup(x => x.DownloadContentAsync(It.IsAny<CancellationToken>()))
                .Returns(
                    Task.FromResult(
                        Response.FromValue(
                            BlobsModelFactory.BlobDownloadResult(
                                new BinaryData("sample_template")), mockResponse.Object)));

            return mock.Object;
        }
    }
}
