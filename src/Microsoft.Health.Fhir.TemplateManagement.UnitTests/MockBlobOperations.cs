// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ContainerRegistry;
using Microsoft.Azure.ContainerRegistry.Models;
using Microsoft.Rest.Azure;

namespace Microsoft.Health.Fhir.TemplateManagement.UnitTests
{
    public class MockBlobOperations : IBlobOperations
    {
        private int _index = 0;

        public Task<AzureOperationResponse> CancelUploadWithHttpMessagesAsync(string location, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<AzureOperationHeaderResponse<BlobCheckChunkHeaders>> CheckChunkWithHttpMessagesAsync(string name, string digest, string range, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<AzureOperationHeaderResponse<BlobCheckHeaders>> CheckWithHttpMessagesAsync(string name, string digest, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<AzureOperationResponse<Stream, BlobDeleteHeaders>> DeleteWithHttpMessagesAsync(string name, string digest, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<AzureOperationHeaderResponse<BlobEndUploadHeaders>> EndUploadWithHttpMessagesAsync(string digest, string location, Stream value = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<AzureOperationResponse<Stream, BlobGetChunkHeaders>> GetChunkWithHttpMessagesAsync(string name, string digest, string range, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<AzureOperationHeaderResponse<BlobGetStatusHeaders>> GetStatusWithHttpMessagesAsync(string location, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<AzureOperationResponse<Stream, BlobGetHeaders>> GetWithHttpMessagesAsync(string name, string digest, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (_index == 0)
            {
                _index += 1;
                throw new HttpRequestException();
            }
            else
            {
                return Task.FromResult(result: new AzureOperationResponse<Stream, BlobGetHeaders>() { Body = new MemoryStream(), Request = new HttpRequestMessage() });
            }
        }

        public Task<AzureOperationHeaderResponse<BlobMountHeaders>> MountWithHttpMessagesAsync(string name, string fromParameter, string mount, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<AzureOperationHeaderResponse<BlobStartUploadHeaders>> StartUploadWithHttpMessagesAsync(string name, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<AzureOperationHeaderResponse<BlobUploadHeaders>> UploadWithHttpMessagesAsync(Stream value, string location, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
