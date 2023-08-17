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

        // Max iterations for rendering templates.
        public int MaxIterations { get; set; } = 100000;

        // Boolean that determines whether the input message payload is included in the exception details
        public bool IncludeInputInException { get; set; } = false;
    }
}
