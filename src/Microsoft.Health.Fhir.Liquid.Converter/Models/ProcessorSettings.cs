// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.Liquid.Converter.Models
{
    public class ProcessorSettings
    {
        // Time out for rendering templates in milliseconds. By default no time out is set, which is zero in DotLiquid.
        public int TimeOut { get; set; } = 0;
    }
}
