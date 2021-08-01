﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Health.Fhir.TemplateManagement.Models
{
    public class OverlayMetadata
    {
        public int SequenceNumber { get; set; } = -1;

        public Dictionary<string, string> FileDigests { get; set; }

        public string Signature { get; set; }
    }
}
