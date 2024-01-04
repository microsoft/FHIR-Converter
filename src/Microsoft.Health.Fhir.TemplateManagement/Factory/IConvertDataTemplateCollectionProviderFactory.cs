// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Azure.Storage.Blobs;
using Microsoft.Health.Fhir.TemplateManagement.ArtifactProviders;

namespace Microsoft.Health.Fhir.TemplateManagement.Factory
{
    public interface IConvertDataTemplateCollectionProviderFactory
    {
        public IConvertDataTemplateCollectionProvider CreateTemplateCollectionProvider();
    }
}
