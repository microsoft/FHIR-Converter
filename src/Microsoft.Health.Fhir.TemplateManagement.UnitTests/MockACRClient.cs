// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Azure.ContainerRegistry;
using Microsoft.Rest;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.TemplateManagement.UnitTests
{
    public class MockACRClient : IAzureContainerRegistryClient
    {
        public JsonSerializerSettings SerializationSettings => throw new NotImplementedException();

        public JsonSerializerSettings DeserializationSettings => throw new NotImplementedException();

        public ServiceClientCredentials Credentials => throw new NotImplementedException();

        public string LoginUri { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string AcceptLanguage { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int? LongRunningOperationRetryTimeout { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool? GenerateClientRequestId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IV2SupportOperations V2Support => throw new NotImplementedException();

        public IManifestsOperations Manifests => throw new NotImplementedException();

        public IRepositoryOperations Repository => throw new NotImplementedException();

        public ITagOperations Tag => throw new NotImplementedException();

        public IRefreshTokensOperations RefreshTokens => throw new NotImplementedException();

        public IAccessTokensOperations AccessTokens => throw new NotImplementedException();

        IBlobOperations IAzureContainerRegistryClient.Blob { get; } = new MockBlobOperations();

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
