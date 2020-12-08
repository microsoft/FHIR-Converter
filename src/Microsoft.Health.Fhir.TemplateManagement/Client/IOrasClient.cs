﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.TemplateManagement.Client
{
    public interface IOrasClient
    {
        Task<bool> PullImageAsync(string outputFolder);

        Task<bool> PushImageAsync(string inputFolder);
    }
}
