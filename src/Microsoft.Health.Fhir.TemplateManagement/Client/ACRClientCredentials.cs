// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Microsoft.Rest;

namespace Microsoft.Health.Fhir.TemplateManagement.Client
{
    public class AcrClientCredentials : ServiceClientCredentials
    {
        private readonly string _token;

        public AcrClientCredentials(string token)
        {
            EnsureArg.IsNotNull(token, nameof(token));

            _token = token;
        }

        public override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            if (AuthenticationHeaderValue.TryParse(_token, out AuthenticationHeaderValue headerValue))
            {
                request.Headers.Authorization = headerValue;
                return base.ProcessHttpRequestAsync(request, cancellationToken);
            }
            else
            {
                throw new ContainerRegistryAuthenticationException(TemplateManagementErrorCode.RegistryUnauthorized, "Token Invalid");
            }
        }
    }
}
