// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.ContainerRegistry.Models;
using Microsoft.Health.Fhir.TemplateManagement.Models;

namespace Microsoft.Health.Fhir.TemplateManagement.Client
{
    public class ArtifactImage
    {
        public string ImageDigest { get; set; }

        public ManifestWrapper Manifest { get; set; }

        public List<ArtifactBlob> Blobs { get; set; } = new List<ArtifactBlob>();
    }
}
