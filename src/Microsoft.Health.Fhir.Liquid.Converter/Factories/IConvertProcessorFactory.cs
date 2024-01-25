// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Processors;

namespace Microsoft.Health.Fhir.Liquid.Converter.Factories
{
    public interface IConvertProcessorFactory
    {
        public IFhirConverter GetProcessor(DataType inputDataType, ConvertDataOutputFormat outputFormat, ProcessorSettings processorSettings = null);
    }
}
