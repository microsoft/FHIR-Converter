// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Health.Fhir.TemplateManagement.Configurations
{
    public class TemplateCollectionConfiguration
    {
        public TimeSpan ShortCacheTimeSpan { get; set; } = TimeSpan.FromMinutes(20);

        public TimeSpan LongCacheTimeSpan { get; set; } = TimeSpan.FromMinutes(60);

        public int TemplateCollectionSizeLimitMegabytes { get; set; } = 10;

        public TemplateHostingConfiguration TemplateHostingConfiguration { get; set; }
    }
}
