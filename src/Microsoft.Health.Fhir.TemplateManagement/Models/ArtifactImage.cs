﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.ContainerRegistry.Models;

namespace Microsoft.Health.Fhir.TemplateManagement.Models
{
    public class ArtifactImage
    {
        public string ImageDigest { get; set; }

        public ManifestWrapper Manifest { get; set; }

        public List<ArtifactBlob> Blobs { get; set; } = new List<ArtifactBlob>();
    }
}