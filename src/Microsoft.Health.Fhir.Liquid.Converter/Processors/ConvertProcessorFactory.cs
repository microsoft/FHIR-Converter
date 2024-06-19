// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using EnsureThat;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Fhir.Liquid.Converter.Models;

namespace Microsoft.Health.Fhir.Liquid.Converter.Processors
{
    public class ConvertProcessorFactory : IConvertProcessorFactory
    {
        private readonly ILoggerFactory _loggerFactory;

        public ConvertProcessorFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = EnsureArg.IsNotNull(loggerFactory, nameof(loggerFactory));
        }

        public IFhirConverter GetProcessor(DataType inputDataType, ConvertDataOutputFormat outputFormat, ProcessorSettings processorSettings = null)
        {
            processorSettings ??= new ProcessorSettings();

            IFhirConverter converter;

            switch (inputDataType, outputFormat)
            {
                case (DataType.Ccda, ConvertDataOutputFormat.Fhir):
                    converter = new CcdaProcessor(processorSettings, _loggerFactory.CreateLogger<CcdaProcessor>());
                    break;
                case (DataType.Fhir, ConvertDataOutputFormat.Fhir):
                    converter = new FhirProcessor(processorSettings, _loggerFactory.CreateLogger<FhirProcessor>());
                    break;
                case (DataType.Hl7v2, ConvertDataOutputFormat.Fhir):
                    converter = new Hl7v2Processor(processorSettings, _loggerFactory.CreateLogger<Hl7v2Processor>());
                    break;
                case (DataType.Json, ConvertDataOutputFormat.Fhir):
                    converter = new JsonProcessor(processorSettings, _loggerFactory.CreateLogger<JsonProcessor>());
                    break;
                case (DataType.Fhir, ConvertDataOutputFormat.Hl7v2):
                    converter = new FhirToHl7v2Processor(processorSettings, _loggerFactory.CreateLogger<FhirToHl7v2Processor>());
                    break;
                default:
                    throw new InvalidOperationException($"Input Data Type {inputDataType} and Output Format {outputFormat} pairing is not supported.");
            }

            return converter;
        }
    }
}
