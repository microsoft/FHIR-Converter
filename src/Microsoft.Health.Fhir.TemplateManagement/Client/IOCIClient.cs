// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.Azure.ContainerRegistry.Models;

namespace Microsoft.Health.Fhir.TemplateManagement.Client
{
    public interface IOCIClient
    {
        Task<ManifestWrapper> PullImageAsync(string outputFolder);

        Task PushImageAsync(string inputFolder);

        void InitClientEnvironment();
    }
}