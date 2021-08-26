// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ContainerRegistry.Models;
using Microsoft.Health.Fhir.TemplateManagement.Models;

namespace Microsoft.Health.Fhir.TemplateManagement.Client
{
    public interface IOCIClient
    {
        Task<ArtifactImage> PullImageAsync(string imageName, string reference, CancellationToken cancellationToken = default);

        Task<ArtifactImage> PushImageAsync(string imageName, string reference, ArtifactImage image, CancellationToken cancellationToken = default);

        Task<ManifestWrapper> GetManifestAsync(string imageName, string reference, CancellationToken cancellationToken = default);

        Task<ArtifactBlob> GetBlobAsync(string imageName, string reference, CancellationToken cancellationToken = default);
    }
}