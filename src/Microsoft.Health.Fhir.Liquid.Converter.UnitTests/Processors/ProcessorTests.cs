// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Processors;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.Processors
{
    public class ProcessorTests
    {
        private static readonly string _hl7v2TestData;
        private static readonly string _ccdaTestData;
        private static readonly string _jsonTestData;

        static ProcessorTests()
        {
            _hl7v2TestData = File.ReadAllText(Path.Join(TestConstants.SampleDataDirectory, "Hl7v2", "LRI_2.0-NG_CBC_Typ_Message.hl7"));
            _ccdaTestData = File.ReadAllText(Path.Join(TestConstants.SampleDataDirectory, "Ccda", "CCD.ccda"));
            _jsonTestData = File.ReadAllText(Path.Join(TestConstants.SampleDataDirectory, "Json", "ExamplePatient.json"));
        }

        public static IEnumerable<object[]> GetValidInputsWithTemplateDirectory()
        {
            yield return new object[] { new Hl7v2Processor(), new TemplateProvider(TestConstants.Hl7v2TemplateDirectory, DataType.Hl7v2), _hl7v2TestData, "ORU_R01" };
            yield return new object[] { new CcdaProcessor(), new TemplateProvider(TestConstants.CcdaTemplateDirectory, DataType.Ccda), _ccdaTestData, "CCD" };
            yield return new object[] { new JsonProcessor(), new TemplateProvider(TestConstants.JsonTemplateDirectory, DataType.Json), _jsonTestData, "ExamplePatient" };
        }

        public static IEnumerable<object[]> GetValidInputsWithTemplateCollection()
        {
            var templateCollection = new List<Dictionary<string, Template>>
            {
                new Dictionary<string, Template>
                {
                    { "TemplateName", Template.Parse(@"{""a"":""b""}") },
                },
            };

            yield return new object[] { new Hl7v2Processor(), new TemplateProvider(templateCollection), _hl7v2TestData };
            yield return new object[] { new CcdaProcessor(), new TemplateProvider(templateCollection), _ccdaTestData };
            yield return new object[] { new JsonProcessor(), new TemplateProvider(templateCollection), _jsonTestData };
        }

        public static IEnumerable<object[]> GetValidInputsWithProcessSettings()
        {
            var positiveTimeOutSettings = new ProcessorSettings
            {
                TimeOut = 1,
            };

            var negativeTimeOutSettings = new ProcessorSettings
            {
                TimeOut = -1,
            };

            yield return new object[]
            {
                new Hl7v2Processor(null), new Hl7v2Processor(new ProcessorSettings()), new Hl7v2Processor(positiveTimeOutSettings), new Hl7v2Processor(negativeTimeOutSettings),
                new TemplateProvider(TestConstants.TestTemplateDirectory, DataType.Hl7v2), _hl7v2TestData,
            };
            yield return new object[]
            {
                new CcdaProcessor(null), new CcdaProcessor(new ProcessorSettings()), new CcdaProcessor(positiveTimeOutSettings), new CcdaProcessor(negativeTimeOutSettings),
                new TemplateProvider(TestConstants.TestTemplateDirectory, DataType.Ccda), _ccdaTestData,
            };
            yield return new object[]
            {
                new JsonProcessor(null), new JsonProcessor(new ProcessorSettings()), new JsonProcessor(positiveTimeOutSettings), new JsonProcessor(negativeTimeOutSettings),
                new TemplateProvider(TestConstants.TestTemplateDirectory, DataType.Json), _jsonTestData,
            };
        }

        public static IEnumerable<object[]> GetValidInputsWithLargeForLoop()
        {
            yield return new object[]
            {
                new Hl7v2Processor(),
                new TemplateProvider(TestConstants.TestTemplateDirectory, DataType.Hl7v2),
                _hl7v2TestData,
            };
            yield return new object[]
            {
                new CcdaProcessor(),
                new TemplateProvider(TestConstants.TestTemplateDirectory, DataType.Ccda),
                _ccdaTestData,
            };
            yield return new object[]
            {
                new JsonProcessor(),
                new TemplateProvider(TestConstants.TestTemplateDirectory, DataType.Json),
                _jsonTestData,
            };
        }

        [Theory]
        [MemberData(nameof(GetValidInputsWithTemplateDirectory))]
        public void GivenAValidTemplateDirectory_WhenConvert_CorrectResultShouldBeReturned(IFhirConverter processor, ITemplateProvider templateProvider, string data, string rootTemplate)
        {
            var result = processor.Convert(data, rootTemplate, templateProvider);
            Assert.True(result.Length > 0);
        }

        [Theory]
        [MemberData(nameof(GetValidInputsWithTemplateCollection))]
        public void GivenAValidTemplateCollection_WhenConvert_CorrectResultShouldBeReturned(IFhirConverter processor, ITemplateProvider templateProvider, string data)
        {
            var result = processor.Convert(data, "TemplateName", templateProvider);
            Assert.True(result.Length > 0);
        }

        [Theory]
        [MemberData(nameof(GetValidInputsWithTemplateCollection))]
        public void GivenInvalidTemplateProviderOrName_WhenConvert_ExceptionsShouldBeThrown(IFhirConverter processor, ITemplateProvider templateProvider, string data)
        {
            // Null, empty or nonexistent root template
            var exception = Assert.Throws<RenderException>(() => processor.Convert(data, null, templateProvider));
            Assert.Equal(FhirConverterErrorCode.NullOrEmptyRootTemplate, exception.FhirConverterErrorCode);

            exception = Assert.Throws<RenderException>(() => processor.Convert(data, string.Empty, templateProvider));
            Assert.Equal(FhirConverterErrorCode.NullOrEmptyRootTemplate, exception.FhirConverterErrorCode);

            exception = Assert.Throws<RenderException>(() => processor.Convert(data, "NonExistentTemplateName", templateProvider));
            Assert.Equal(FhirConverterErrorCode.TemplateNotFound, exception.FhirConverterErrorCode);

            // Null TemplateProvider
            exception = Assert.Throws<RenderException>(() => processor.Convert(data, "TemplateName", null));
            Assert.Equal(FhirConverterErrorCode.NullTemplateProvider, exception.FhirConverterErrorCode);
        }

        [Theory]
        [MemberData(nameof(GetValidInputsWithProcessSettings))]
        public void GivenProcessorSettings_WhenConvert_CorrectResultsShouldBeReturned(
            IFhirConverter nullSettingProcessor,
            IFhirConverter defaultSettingProcessor,
            IFhirConverter positiveTimeOutProcessor,
            IFhirConverter negativeTimeOutProcessor,
            ITemplateProvider templateProvider,
            string data)
        {
            // Null ProcessorSettings: no time out
            var result = nullSettingProcessor.Convert(data, "TimeOutTemplate", templateProvider);
            Assert.True(result.Length > 0);

            // Default ProcessorSettings: no time out
            result = defaultSettingProcessor.Convert(data, "TimeOutTemplate", templateProvider);
            Assert.True(result.Length > 0);

            // Positive time out ProcessorSettings: exception thrown when time out
            var exception = Assert.Throws<RenderException>(() => positiveTimeOutProcessor.Convert(data, "TimeOutTemplate", templateProvider));
            Assert.Equal(FhirConverterErrorCode.TimeoutError, exception.FhirConverterErrorCode);
            Assert.True(exception.InnerException is TimeoutException);

            // Negative time out ProcessorSettings: no time out
            result = negativeTimeOutProcessor.Convert(data, "TimeOutTemplate", templateProvider);
            Assert.True(result.Length > 0);
        }

        [Theory]
        [MemberData(nameof(GetValidInputsWithTemplateDirectory))]
        public void GivenCancellationToken_WhenConvert_CorrectResultsShouldBeReturned(IFhirConverter processor, ITemplateProvider templateProvider, string data, string rootTemplate)
        {
            var cts = new CancellationTokenSource();
            var result = processor.Convert(data, rootTemplate, templateProvider, cts.Token);
            Assert.True(result.Length > 0);

            cts.Cancel();
            Assert.Throws<OperationCanceledException>(() => processor.Convert(data, rootTemplate, templateProvider, cts.Token));
        }

        [Theory]
        [MemberData(nameof(GetValidInputsWithLargeForLoop))]
        public void GivenTemplateWithLargeForLoop_WhenConvert_ExceptionShouldBeThrown(IFhirConverter processor, ITemplateProvider templateProvider, string data)
        {
            var exception = Assert.Throws<RenderException>(() => processor.Convert(data, "LargeForLoopTemplate", templateProvider));
            Assert.Contains("Render Error - Maximum number of iterations 100000 exceeded", exception.Message);
        }

        [Theory]
        [MemberData(nameof(GetValidInputsWithLargeForLoop))]
        public void GivenTemplateWithNestedForLoop_WhenConvert_CorrectResultShouldBeReturned(IFhirConverter processor, ITemplateProvider templateProvider, string data)
        {
            var result = processor.Convert(data, "NestedForLoopTemplate", templateProvider);
            Assert.True(result.Length > 0);
        }
    }
}
