// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Azure.ContainerRegistry;
using Microsoft.Azure.ContainerRegistry.Models;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Microsoft.Health.Fhir.TemplateManagement.Utilities;
using Microsoft.Rest;
using Microsoft.Rest.Azure;

namespace Microsoft.Health.Fhir.TemplateManagement.Client
{
    public class AcrClient : IOciClient
    {
        private readonly IAzureContainerRegistryClient _client;

        // Accept media type for manifest.
        private readonly List<string> _acceptedManifestMediatype =
            new List<string>()
            {
                Constants.V2MediaTypeManifest,
                Constants.OCIMediaTypeImageManifest,
            };

        public AcrClient(string registry, string token)
        {
            EnsureArg.IsNotNull(registry, nameof(registry));
            EnsureArg.IsNotNull(token, nameof(token));

            _client = new AzureContainerRegistryClient(registry, new AcrClientCredentials(token));
        }

        public AcrClient(IAzureContainerRegistryClient client)
        {
            EnsureArg.IsNotNull(client, nameof(client));
            _client = client;
        }

        public async Task<Stream> PullBlobAsStreamAcync(string name, string digest, CancellationToken cancellationToken = default)
        {
            Stream rawStream;
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                rawStream = await _client.Blob.GetAsync(name, digest, cancellationToken);
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

        public async Task<ArtifactBlob> GetBlobAsync(string name, string reference, CancellationToken cancellationToken = default)
        {
            Stream rawStream = await PullBlobAsStreamAcync(name, reference, cancellationToken);
            using var streamReader = new MemoryStream();
            rawStream.CopyTo(streamReader);
            var content = streamReader.ToArray();
            ValidationUtility.ValidateOneBlob(content, reference);
            return new Models.ArtifactBlob() { Digest = reference, Content = content };
        }

        public async Task<ManifestWrapper> GetManifestAsync(string imageName, string reference, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var manifestInfo = await _client.Manifests.GetAsync(imageName, reference, string.Join(",", _acceptedManifestMediatype), cancellationToken);
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

        public Task<ArtifactImage> PullImageAsync(string name, string reference, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<string> PushImageAsync(string name, string reference, ArtifactImage image, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
