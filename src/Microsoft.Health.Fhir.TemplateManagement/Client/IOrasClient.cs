// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.TemplateManagement.Client
{
    public interface IOrasClient
    {
        Task<string> PullImageAsync(string outputFolder);

        Task PushImageAsync(string inputFolder, List<string> filePathList);
    }
}
