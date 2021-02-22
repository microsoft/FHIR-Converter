// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Azure.ContainerRegistry;
using Microsoft.Azure.ContainerRegistry.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Microsoft.Health.Fhir.TemplateManagement.Utilities;
using Microsoft.Rest;
using Microsoft.Rest.Azure;
using Polly;

namespace Microsoft.Health.Fhir.TemplateManagement.Client
{
    public class ACRClient : IOCIArtifactClient
    {
        private readonly IAzureContainerRegistryClient _client;
        private static readonly ILogger _logger = TemplateManagementLogging.CreateLogger<ACRClient>();
        private readonly AsyncPolicy _retryPolicy = Policy.Handle<HttpRequestException>().WaitAndRetryAsync(1, retryNumber => TimeSpan.FromMilliseconds(200), onRetry: (exception, retryCount, context) =>
        {
            _logger.LogError($"Retry {retryCount} of {context.PolicyKey} at {context.OperationKey}, due to: {exception}.");
        });

        public ACRClient(string registry, string token)
        {
            EnsureArg.IsNotNull(registry, nameof(registry));
            EnsureArg.IsNotNull(token, nameof(token));

            _client = new AzureContainerRegistryClient(registry, new ACRClientCredentials(token));
        }

        public ACRClient(IAzureContainerRegistryClient client)
        {
            EnsureArg.IsNotNull(client, nameof(client));
            _client = client;
        }

        public async Task<Stream> PullBlobAsStreamAcync(string imageName, string digest, CancellationToken cancellationToken = default)
        {
            Stream rawStream;
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                rawStream = await _retryPolicy.ExecuteAsync(async () => await _client.Blob.GetAsync(imageName, digest, cancellationToken));
                return rawStream;
            }
            catch (TemplateManagementException)
            {
                throw;
            }
            catch (CloudException ex)
            {
                ProcessClientResponseError(ex.Response, ex);
                throw;
            }
            catch (Exception ex)
            {
                throw new ImageFetchException(TemplateManagementErrorCode.FetchLayerFailed, $"Pull Image Failed.{ex}", ex);
            }
        }

        public async Task<byte[]> PullBlobAsBytesAcync(string imageName, string digest, CancellationToken cancellationToken = default)
        {
            Stream rawStream = await PullBlobAsStreamAcync(imageName, digest, cancellationToken);
            using var streamReader = new MemoryStream();
            rawStream.CopyTo(streamReader);
            var content = streamReader.ToArray();
            ValidationUtility.ValidateOneBlob(content, digest);
            return content;
        }

        public async Task<ManifestWrapper> PullManifestAcync(string imageName, string label, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var manifestInfo = await _client.Manifests.GetAsync(imageName, label, Constants.MediatypeV2Manifest, cancellationToken);
                return manifestInfo;
            }
            catch (TemplateManagementException)
            {
                throw;
            }
            catch (AcrErrorsException ex)
            {
                ProcessClientResponseError(ex.Response, ex);
                throw;
            }
            catch (Exception ex)
            {
                throw new ImageFetchException(TemplateManagementErrorCode.FetchManifestFailed, $"Pull Image Failed.{ex}", ex);
            }
        }

        private void ProcessClientResponseError(HttpResponseMessageWrapper response, Exception ex)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new ContainerRegistryAuthenticationException(TemplateManagementErrorCode.RegistryUnauthorized, "Registry authentication failed.", ex);
            }
            else if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new ContainerRegistryAuthenticationException(TemplateManagementErrorCode.AccessForbidden, "Token has not been granted 'pull' access.", ex);
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new ImageNotFoundException(TemplateManagementErrorCode.ImageNotFound, "Image Not Found.", ex);
            }
            else
            {
                throw new ImageFetchException(TemplateManagementErrorCode.FetchLayerFailed, $"Pull Image Failed.{ex}", ex);
            }
        }
    }
}
