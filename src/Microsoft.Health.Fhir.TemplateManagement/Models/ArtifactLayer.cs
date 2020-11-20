// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.TemplateManagement.Models
{
    public class ArtifactLayer
    {
        // Size of the layer content.
        public long Size { get; set; }

        // Sha256 digest of the layer.
        public string Digest { get; set; }

        // Content of the layer.
        public virtual object Content { get; set; }
    }
}
