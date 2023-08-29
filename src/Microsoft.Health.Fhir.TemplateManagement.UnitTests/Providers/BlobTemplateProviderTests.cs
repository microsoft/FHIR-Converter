// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Health.Fhir.TemplateManagement.ArtifactProviders;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Moq;
using Xunit;

namespace Microsoft.Health.Fhir.TemplateManagement.UnitTests.Providers
{
    public class BlobTemplateProviderTests
    {
        [Fact]
        public async Task GivenBlobTemplateProvider_WhenGetTemplateCollectionFromTemplateProvider_ThenTemplatesAreReturned()
        {
            var templateConfiguration = new TemplateCollectionConfiguration();

            var blobTemplateProvider = new BlobTemplateProvider(GetBlobContainerClientMock(), new MemoryCache(new MemoryCacheOptions()), templateConfiguration);

            var templateColllection = await blobTemplateProvider.GetTemplateCollectionAsync();

            Assert.NotEmpty(templateColllection);
        }

        public static BlobContainerClient GetBlobContainerClientMock()
        {
            var mock = new Mock<BlobContainerClient>();

            mock
                .Setup(x => x.GetBlobClient(It.IsAny<string>()))
                .Returns(GetBlobClientMock());

            var blobs = new BlobItem[]
            {
                BlobsModelFactory.BlobItem("blob_name"),
            };

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
                .Setup(x => x.DownloadContentAsync())
                .Returns(
                    Task.FromResult(
                        Response.FromValue(
                            BlobsModelFactory.BlobDownloadResult(
                                new BinaryData("sample_template")), mockResponse.Object)));

            return mock.Object;
        }
    }
}
