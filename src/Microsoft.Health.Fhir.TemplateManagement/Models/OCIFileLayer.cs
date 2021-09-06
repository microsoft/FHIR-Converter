// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Health.Fhir.TemplateManagement.Models
{
    public class OciFileLayer : ArtifactBlob
    {
        public Dictionary<string, byte[]> FileContent { get; set; } = new Dictionary<string, byte[]> { };
    }
}
