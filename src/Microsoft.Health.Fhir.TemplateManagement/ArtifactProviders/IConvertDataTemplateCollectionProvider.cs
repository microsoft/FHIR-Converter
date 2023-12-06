// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DotLiquid;

namespace Microsoft.Health.Fhir.TemplateManagement.ArtifactProviders
{
    public interface IConvertDataTemplateCollectionProvider
    {
        public Task<List<Dictionary<string, Template>>> GetTemplateCollectionAsync(CancellationToken cancellationToken);
    }
}