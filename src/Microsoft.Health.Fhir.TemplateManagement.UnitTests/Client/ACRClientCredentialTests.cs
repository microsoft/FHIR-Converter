// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Net.Http;
using Microsoft.Health.Fhir.TemplateManagement.Client;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Xunit;

namespace Microsoft.Health.Fhir.TemplateManagement.UnitTests
{
    public class AcrClientCredentialTests
    {
        [Fact]
        public void GivenValidTokenAndRequest_WhenAddCredentialToHttpRequest_ProcessedRequestWillBeReturned()
        {
            string token = "Basic testtoken";
            var credential = new AcrClientCredentials(token);
            Assert.NotNull(credential.ProcessHttpRequestAsync(new HttpRequestMessage()));
        }

        [Fact]
        public async System.Threading.Tasks.Task GivenInValidTokenAndRequest_WhenAddCredentialToHttpRequest_ExceptionWillBeThrown()
        {
            string token = string.Empty;
            var credential = new AcrClientCredentials(token);
            await Assert.ThrowsAsync<ContainerRegistryAuthenticationException>(async () => await credential.ProcessHttpRequestAsync(new HttpRequestMessage()));
        }
    }
}
