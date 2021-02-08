// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.Health.Fhir.TemplateManagement.Models;

namespace Microsoft.Health.Fhir.TemplateManagement.Client
{
    public interface IOrasClient
    {
        Task<OCIOperationResult> PullImageAsync(string outputFolder);

        Task<OCIOperationResult> PushImageAsync(string inputFolder);
    }
}