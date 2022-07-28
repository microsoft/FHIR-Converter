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
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.Processors
{
    public class ProcessorTests
    {
        private static readonly string _hl7v2TestData;
        private static readonly string _ccdaTestData;
        private static readonly string _jsonTestData;
        private static readonly string _jsonExpectData;
        private static readonly string _fhirStu3TestData;
        private static readonly ProcessorSettings _processorSettings;

        static ProcessorTests()
        {
            _hl7v2TestData = File.ReadAllText(Path.Join(TestConstants.SampleDataDirectory, "Hl7v2", "LRI_2.0-NG_CBC_Typ_Message.hl7"));
            _ccdaTestData = File.ReadAllText(Path.Join(TestConstants.SampleDataDirectory, "Ccda", "CCD.ccda"));
            _jsonTestData = File.ReadAllText(Path.Join(TestConstants.SampleDataDirectory, "Json", "ExamplePatient.json"));
            _jsonExpectData = File.ReadAllText(Path.Join(TestConstants.ExpectedDirectory, "ExamplePatient.json"));
            _fhirStu3TestData = File.ReadAllText(Path.Join(TestConstants.SampleDataDirectory, "Stu3", "Patient.json"));
            _processorSettings = new ProcessorSettings();
        }

        public static IEnumerable<object[]> GetValidInputsWithTemplateDirectory()
        {
            yield return new object[] { new Hl7v2Processor(_processorSettings), new TemplateProvider(TestConstants.Hl7v2TemplateDirectory, DataType.Hl7v2), _hl7v2TestData, "ORU_R01" };
            yield return new object[] { new CcdaProcessor(_processorSettings), new TemplateProvider(TestConstants.CcdaTemplateDirectory, DataType.Ccda), _ccdaTestData, "CCD" };
            yield return new object[] { new JsonProcessor(_processorSettings), new TemplateProvider(TestConstants.JsonTemplateDirectory, DataType.Json), _jsonTestData, "ExamplePatient" };
            yield return new object[] { new FhirProcessor(_processorSettings), new TemplateProvider(TestConstants.FhirStu3TemplateDirectory, DataType.Fhir), _fhirStu3TestData, "Patient" };
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

            yield return new object[] { new Hl7v2Processor(_processorSettings), new TemplateProvider(templateCollection), _hl7v2TestData };
            yield return new object[] { new CcdaProcessor(_processorSettings), new TemplateProvider(templateCollection), _ccdaTestData };
            yield return new object[] { new JsonProcessor(_processorSettings), new TemplateProvider(templateCollection), _jsonTestData };
            yield return new object[] { new FhirProcessor(_processorSettings), new TemplateProvider(templateCollection), _fhirStu3TestData };
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
                new Hl7v2Processor(new ProcessorSettings()), new Hl7v2Processor(positiveTimeOutSettings), new Hl7v2Processor(negativeTimeOutSettings),
                new TemplateProvider(TestConstants.TestTemplateDirectory, DataType.Hl7v2), _hl7v2TestData,
            };
            yield return new object[]
            {
                new CcdaProcessor(new ProcessorSettings()), new CcdaProcessor(positiveTimeOutSettings), new CcdaProcessor(negativeTimeOutSettings),
                new TemplateProvider(TestConstants.TestTemplateDirectory, DataType.Ccda), _ccdaTestData,
            };
            yield return new object[]
            {
                new JsonProcessor(new ProcessorSettings()), new JsonProcessor(positiveTimeOutSettings), new JsonProcessor(negativeTimeOutSettings),
                new TemplateProvider(TestConstants.TestTemplateDirectory, DataType.Json), _jsonTestData,
            };
            yield return new object[]
            {
                new FhirProcessor(new ProcessorSettings()), new FhirProcessor(positiveTimeOutSettings), new FhirProcessor(negativeTimeOutSettings),
                new TemplateProvider(TestConstants.TestTemplateDirectory, DataType.Fhir), _fhirStu3TestData,
            };
        }

        public static IEnumerable<object[]> GetValidInputsWithLargeForLoop()
        {
            yield return new object[]
            {
                new Hl7v2Processor(_processorSettings),
                new TemplateProvider(TestConstants.TestTemplateDirectory, DataType.Hl7v2),
                _hl7v2TestData,
            };
            yield return new object[]
            {
                new CcdaProcessor(_processorSettings),
                new TemplateProvider(TestConstants.TestTemplateDirectory, DataType.Ccda),
                _ccdaTestData,
            };
            yield return new object[]
            {
                new JsonProcessor(_processorSettings),
                new TemplateProvider(TestConstants.TestTemplateDirectory, DataType.Json),
                _jsonTestData,
            };
            yield return new object[]
            {
                new FhirProcessor(_processorSettings),
                new TemplateProvider(TestConstants.TestTemplateDirectory, DataType.Fhir),
                _fhirStu3TestData,
            };
        }

        public static IEnumerable<object[]> GetValidInputsWithNestingTooDeep()
        {
            yield return new object[]
            {
                new Hl7v2Processor(_processorSettings),
                new TemplateProvider(TestConstants.TestTemplateDirectory, DataType.Hl7v2),
                _hl7v2TestData,
            };
            yield return new object[]
            {
                new CcdaProcessor(_processorSettings),
                new TemplateProvider(TestConstants.TestTemplateDirectory, DataType.Ccda),
                _ccdaTestData,
            };
            yield return new object[]
            {
                new JsonProcessor(_processorSettings),
                new TemplateProvider(TestConstants.TestTemplateDirectory, DataType.Json),
                _jsonTestData,
            };
            yield return new object[]
            {
                new FhirProcessor(_processorSettings),
                new TemplateProvider(TestConstants.TestTemplateDirectory, DataType.Fhir),
                _fhirStu3TestData,
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
            IFhirConverter defaultSettingProcessor,
            IFhirConverter positiveTimeOutProcessor,
            IFhirConverter negativeTimeOutProcessor,
            ITemplateProvider templateProvider,
            string data)
        {
            // Default ProcessorSettings: no time out
            var result = defaultSettingProcessor.Convert(data, "TimeOutTemplate", templateProvider);
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

        [Theory]
        [MemberData(nameof(GetValidInputsWithNestingTooDeep))]
        public void GivenTemplateWithNestingTooDeep_WhenConvert_ExceptionShouldBeThrown(IFhirConverter processor, ITemplateProvider templateProvider, string data)
        {
            var exception = Assert.Throws<RenderException>(() => processor.Convert(data, "NestingTooDeepTemplate", templateProvider));
            Assert.Contains("Nesting too deep", exception.Message);

            exception = Assert.Throws<RenderException>(() => processor.Convert(data, "NestingTooDeepDiffTemplate", templateProvider));
            Assert.Contains("Nesting too deep", exception.Message);
        }

        [Fact]
        public void GivenJObjectInput_WhenConvertWithJsonProcessor_CorrectResultShouldBeReturned()
        {
            var processor = new JsonProcessor(_processorSettings);
            var templateProvider = new TemplateProvider(TestConstants.JsonTemplateDirectory, DataType.Json);
            var testData = JObject.Parse(_jsonTestData);
            var result = processor.Convert(testData, "ExamplePatient", templateProvider);
            Assert.True(JToken.DeepEquals(JObject.Parse(_jsonExpectData), JToken.Parse(result)));
        }
    }
}
