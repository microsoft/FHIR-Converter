// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Processors;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.Processors
{
    public class ConvertProcessorFactoryTests
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IConvertProcessorFactory _convertProcessorFactory;

        public ConvertProcessorFactoryTests()
        {
            _loggerFactory = new NullLoggerFactory();
            _convertProcessorFactory = new ConvertProcessorFactory(_loggerFactory);
        }

        public static IEnumerable<object[]> GetValidInputOutputPairings()
        {
            yield return new object[] { DataType.Ccda, ConvertDataOutputFormat.Fhir, typeof(CcdaProcessor) };
            yield return new object[] { DataType.Fhir, ConvertDataOutputFormat.Fhir, typeof(FhirProcessor) };
            yield return new object[] { DataType.Hl7v2, ConvertDataOutputFormat.Fhir, typeof(Hl7v2Processor) };
            yield return new object[] { DataType.Json, ConvertDataOutputFormat.Fhir, typeof(JsonProcessor) };
            yield return new object[] { DataType.Fhir, ConvertDataOutputFormat.Hl7v2, typeof(FhirToHl7v2Processor) };
        }

        [Theory]
        [MemberData(nameof(GetValidInputOutputPairings))]
        public void GivenValidInputAndOutputPairsAndDefaultProcessorSettings_CorrectProcessorReturned(DataType inputType, ConvertDataOutputFormat outputFormat, Type expectedProcessorType)
        {
            IFhirConverter processor = _convertProcessorFactory.GetProcessor(inputType, outputFormat);
            Assert.Equal(expectedProcessorType, processor.GetType());
        }

        [Fact]
        public void GivenInvalidInputAndOutputPairs_ExceptionIsThrown()
        {
            var inputType = DataType.Ccda;
            var outputFormat = ConvertDataOutputFormat.Hl7v2;

            Assert.Throws<InvalidOperationException>(() => _convertProcessorFactory.GetProcessor(inputType, outputFormat));
        }

        [Fact]
        public void GivenCustomProcessorSettings_CorrectProcessorReturned()
        {
            ProcessorSettings settings = new ProcessorSettings
            {
                TimeOut = 1,
                MaxIterations = 10,
                EnableTelemetryLogger = true,
            };

            var inputType = DataType.Ccda;
            var outputFormat = ConvertDataOutputFormat.Fhir;

            // we expect that the IFhirConverter returned is a BaseProcessor. If not it should throw an exception
            BaseProcessor processor = (BaseProcessor)_convertProcessorFactory.GetProcessor(inputType, outputFormat, settings);

            Assert.Equal(1, processor.Settings.TimeOut);
            Assert.Equal(10, processor.Settings.MaxIterations);
            Assert.True(processor.Settings.EnableTelemetryLogger);
        }
    }
}
