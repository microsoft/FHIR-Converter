// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Fhir.Liquid.Converter.Models;

namespace Microsoft.Health.Fhir.Liquid.Converter.Processors
{
    public class VersionConversionProcessor : JsonProcessor
    { 
        public VersionConversionProcessor(ProcessorSettings processorSettings = null)
            : base(processorSettings)
        {
        }
    }
}
