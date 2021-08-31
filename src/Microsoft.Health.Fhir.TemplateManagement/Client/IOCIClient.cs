// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ContainerRegistry.Models;
using Microsoft.Health.Fhir.TemplateManagement.Models;

namespace Microsoft.Health.Fhir.TemplateManagement.Client
{
    public interface IOciClient
    {
        Task<ArtifactImage> PullImageAsync(string name, string reference, CancellationToken cancellationToken = default);

        Task<string> PushImageAsync(string name, string tag, ArtifactImage image, CancellationToken cancellationToken = default);

        Task<ManifestWrapper> GetManifestAsync(string name, string digest, CancellationToken cancellationToken = default);

        Task<ArtifactBlob> GetBlobAsync(string name, string digest, CancellationToken cancellationToken = default);
    }
}