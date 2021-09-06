// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.ContainerRegistry;
using Microsoft.Azure.ContainerRegistry.Models;
using Microsoft.Health.Fhir.TemplateManagement.Client;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Rest;
using Microsoft.Rest.Azure;
using Moq;
using Xunit;

namespace Microsoft.Health.Fhir.TemplateManagement.UnitTests
{
    public class AcrClientTests
    {
        private readonly string _registry = "testregistry";
        private readonly string _token = "testtoken";
        private readonly string _imageName = "testname";
        private readonly string _label = "latest";
        private readonly string _digest = "testdigest";

        [Fact]
        public void GivenARegistryAndToken_WhenCreateAcrClient_ACorrectAcrClientWillBeConstructed()
        {
            var acrClient = new AcrClient(_registry, _token);
            Assert.NotNull(acrClient);
        }

        [Fact]
        public void GivenARequestFromAcrClient_WhenPullBlob_IfUnAuthed_ContainerRegistryAuthenticationExceptionWillBeThrown()
        {
            var acrClientMock = new Mock<IAzureContainerRegistryClient>();
            acrClientMock.Setup(p => p.Blob).Throws(new CloudException() { Response = new HttpResponseMessageWrapper(new HttpResponseMessage() { StatusCode = HttpStatusCode.Unauthorized }, "test") });
            var client = new AcrClient(acrClientMock.Object);
            Assert.ThrowsAsync<ContainerRegistryAuthenticationException>(() => client.GetBlobAsync(_imageName, _digest));
            Assert.ThrowsAsync<ContainerRegistryAuthenticationException>(() => client.PullBlobAsStreamAcync(_imageName, _digest));
        }

        [Fact]
        public void GivenARequestFromAcrClient_WhenPullBlob_IfAccessForbidden_ContainerRegistryAuthenticationExceptionWillBeThrown()
        {
            var acrClientMock = new Mock<IAzureContainerRegistryClient>();
            acrClientMock.Setup(p => p.Blob).Throws(new CloudException() { Response = new HttpResponseMessageWrapper(new HttpResponseMessage() { StatusCode = HttpStatusCode.Forbidden }, "test") });
            var client = new AcrClient(acrClientMock.Object);
            Assert.ThrowsAsync<ContainerRegistryAuthenticationException>(() => client.GetBlobAsync(_imageName, _digest));
            Assert.ThrowsAsync<ContainerRegistryAuthenticationException>(() => client.PullBlobAsStreamAcync(_imageName, _digest));
        }

        [Fact]
        public void GivenARequestFromAcrClient_WhenPullBlob_IfNotFound_ImageNotFoundExceptionWillBeThrown()
        {
            var acrClientMock = new Mock<IAzureContainerRegistryClient>();
            acrClientMock.Setup(p => p.Blob).Throws(new CloudException() { Response = new HttpResponseMessageWrapper(new HttpResponseMessage() { StatusCode = HttpStatusCode.NotFound }, "test") });
            var client = new AcrClient(acrClientMock.Object);
            Assert.ThrowsAsync<ImageNotFoundException>(() => client.GetBlobAsync(_imageName, _digest));
            Assert.ThrowsAsync<ImageNotFoundException>(() => client.PullBlobAsStreamAcync(_imageName, _digest));
        }

        [Fact]
        public void GivenARequestFromAcrClient_WhenPullBlob_IfFailed_ImageFetchExceptionWillBeThrown()
        {
            var acrClientMock = new Mock<IAzureContainerRegistryClient>();
            acrClientMock.Setup(p => p.Blob).Throws(new CloudException() { Response = new HttpResponseMessageWrapper(new HttpResponseMessage() { StatusCode = HttpStatusCode.BadRequest }, "test") });
            var client = new AcrClient(acrClientMock.Object);
            Assert.ThrowsAsync<ImageFetchException>(() => client.GetBlobAsync(_imageName, _digest));
            Assert.ThrowsAsync<ImageFetchException>(() => client.PullBlobAsStreamAcync(_imageName, _digest));

            acrClientMock.Setup(p => p.Blob).Throws(new Exception("test"));
            Assert.ThrowsAsync<ImageFetchException>(() => client.GetBlobAsync(_imageName, _digest));
            Assert.ThrowsAsync<ImageFetchException>(() => client.PullBlobAsStreamAcync(_imageName, _digest));
        }

        [Fact]
        public void GivenARequestFromAcrClient_WhenPullManifest_IfUnAuthed_ContainerRegistryAuthenticationExceptionWillBeThrown()
        {
            var acrClientMock = new Mock<IAzureContainerRegistryClient>();
            acrClientMock.Setup(p => p.Blob).Throws(new AcrErrorsException() { Response = new HttpResponseMessageWrapper(new HttpResponseMessage() { StatusCode = HttpStatusCode.Unauthorized }, "test") });
            var client = new AcrClient(acrClientMock.Object);
            Assert.ThrowsAsync<ContainerRegistryAuthenticationException>(() => client.GetManifestAsync(_imageName, _label));
        }

        [Fact]
        public void GivenARequestFromAcrClient_WhenPullManifest_IfAccessForbidden_ContainerRegistryAuthenticationExceptionWillBeThrown()
        {
            var acrClientMock = new Mock<IAzureContainerRegistryClient>();
            acrClientMock.Setup(p => p.Blob).Throws(new AcrErrorsException() { Response = new HttpResponseMessageWrapper(new HttpResponseMessage() { StatusCode = HttpStatusCode.Forbidden }, "test") });
            var client = new AcrClient(acrClientMock.Object);
            Assert.ThrowsAsync<ContainerRegistryAuthenticationException>(() => client.GetManifestAsync(_imageName, _label));
        }

        [Fact]
        public void GivenARequestFromAcrClient_WhenPullManifest_IfNotFound_ImageNotFoundExceptionWillBeThrown()
        {
            var acrClientMock = new Mock<IAzureContainerRegistryClient>();
            acrClientMock.Setup(p => p.Blob).Throws(new AcrErrorsException() { Response = new HttpResponseMessageWrapper(new HttpResponseMessage() { StatusCode = HttpStatusCode.NotFound }, "test") });
            var client = new AcrClient(acrClientMock.Object);
            Assert.ThrowsAsync<ImageNotFoundException>(() => client.GetManifestAsync(_imageName, _label));
        }

        [Fact]
        public void GivenARequestFromAcrClient_WhenPullManifest_IfFailed_ImageFetchExceptionWillBeThrown()
        {
            var acrClientMock = new Mock<IAzureContainerRegistryClient>();
            acrClientMock.Setup(p => p.Blob).Throws(new AcrErrorsException() { Response = new HttpResponseMessageWrapper(new HttpResponseMessage() { StatusCode = HttpStatusCode.BadRequest }, "test") });
            var client = new AcrClient(acrClientMock.Object);
            Assert.ThrowsAsync<ImageFetchException>(() => client.GetManifestAsync(_imageName, _label));
            acrClientMock.Setup(p => p.Blob).Throws(new Exception("test"));
            Assert.ThrowsAsync<ImageFetchException>(() => client.GetManifestAsync(_imageName, _label));
        }
    }
}
