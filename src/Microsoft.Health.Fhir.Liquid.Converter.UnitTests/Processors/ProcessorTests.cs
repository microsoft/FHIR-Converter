﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Processors;
using Newtonsoft.Json;
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
        private static readonly string _fhirBundleTestData;
        private static readonly string _fhirBundleExpectedData;
        private static readonly ProcessorSettings _processorSettings;
        private static readonly Hl7v2Processor _hl7v2Processor;
        private static readonly CcdaProcessor _ccdaProcessor;
        private static readonly JsonProcessor _jsonProcessor;
        private static readonly FhirProcessor _fhirProcessor;

        static ProcessorTests()
        {
            _hl7v2TestData = File.ReadAllText(Path.Join(TestConstants.SampleDataDirectory, "Hl7v2", "LRI_2.0-NG_CBC_Typ_Message.hl7"));
            _ccdaTestData = File.ReadAllText(Path.Join(TestConstants.SampleDataDirectory, "Ccda", "CCD.ccda"));
            _jsonTestData = File.ReadAllText(Path.Join(TestConstants.SampleDataDirectory, "Json", "ExamplePatient.json"));
            _jsonExpectData = File.ReadAllText(Path.Join(TestConstants.ExpectedDirectory, "ExamplePatient.json"));
            _fhirStu3TestData = File.ReadAllText(Path.Join(TestConstants.SampleDataDirectory, "Stu3", "Patient.json"));
            _fhirBundleTestData = File.ReadAllText(Path.Join(TestConstants.SampleDataDirectory, "Json", "Bundle.json"));
            _fhirBundleExpectedData = File.ReadAllText(Path.Join(TestConstants.ExpectedDirectory, "BundleToHl7v2.hl7"));
            _processorSettings = new ProcessorSettings();

            _hl7v2Processor = new Hl7v2Processor(_processorSettings, FhirConverterLogging.CreateLogger<Hl7v2Processor>());
            _ccdaProcessor = new CcdaProcessor(_processorSettings, FhirConverterLogging.CreateLogger<CcdaProcessor>());
            _jsonProcessor = new JsonProcessor(_processorSettings, FhirConverterLogging.CreateLogger<JsonProcessor>());
            _fhirProcessor = new FhirProcessor(_processorSettings, FhirConverterLogging.CreateLogger<FhirProcessor>());
        }

        public static IEnumerable<object[]> GetValidInputsWithTemplateDirectory()
        {
            yield return new object[] { _hl7v2Processor, new TemplateProvider(TestConstants.Hl7v2TemplateDirectory, DataType.Hl7v2), _hl7v2TestData, "ORU_R01" };
            yield return new object[] { _ccdaProcessor, new TemplateProvider(TestConstants.CcdaTemplateDirectory, DataType.Ccda), _ccdaTestData, "CCD" };
            yield return new object[] { _jsonProcessor, new TemplateProvider(TestConstants.JsonTemplateDirectory, DataType.Json), _jsonTestData, "ExamplePatient" };
            yield return new object[] { _fhirProcessor, new TemplateProvider(TestConstants.FhirStu3TemplateDirectory, DataType.Fhir), _fhirStu3TestData, "Patient" };
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

            yield return new object[] { _hl7v2Processor, new TemplateProvider(templateCollection), _hl7v2TestData };
            yield return new object[] { _ccdaProcessor, new TemplateProvider(templateCollection), _ccdaTestData };
            yield return new object[] { _jsonProcessor, new TemplateProvider(templateCollection), _jsonTestData };
            yield return new object[] { _fhirProcessor, new TemplateProvider(templateCollection), _fhirStu3TestData };
        }

        public static IEnumerable<object[]> GetMockDefaultTemplateCollection()
        {
            var rootTemplate = @"{% include 'Sub/Template1' -%}";
            var hl7v2SubTemplate = @"{""Hl7v2"":""subtemplate1""}";
            var ccdaSubTemplate = @"{""Ccda"":""subtemplate1""}";
            var jsonSubTemplate = @"{""Json"":""subtemplate1""}";
            var fhirSubTemplate = @"{""Fhir"":""subtemplate1""}";

            var templateCollection = new List<Dictionary<string, Template>>
            {
                new Dictionary<string, Template>
                {
                    { "Hl7v2/Template1", Template.Parse(rootTemplate) },
                    { "Hl7v2/Sub/Template1", Template.Parse(hl7v2SubTemplate) },
                    { "Ccda/Template1", Template.Parse(rootTemplate) },
                    { "Ccda/Sub/Template1", Template.Parse(ccdaSubTemplate) },
                    { "Json/Template1", Template.Parse(rootTemplate) },
                    { "Json/Sub/Template1", Template.Parse(jsonSubTemplate) },
                    { "Fhir/Template1", Template.Parse(rootTemplate) },
                    { "Fhir/Sub/Template1", Template.Parse(fhirSubTemplate) },
                },
            };

            yield return new object[] { _hl7v2Processor, new TemplateProvider(templateCollection, isDefaultTemplateProvider: true), _hl7v2TestData, hl7v2SubTemplate };
            yield return new object[] { _ccdaProcessor, new TemplateProvider(templateCollection, isDefaultTemplateProvider: true), _ccdaTestData, ccdaSubTemplate };
            yield return new object[] { _jsonProcessor, new TemplateProvider(templateCollection, isDefaultTemplateProvider: true), _jsonTestData, jsonSubTemplate };
            yield return new object[] { _fhirProcessor, new TemplateProvider(templateCollection, isDefaultTemplateProvider: true), _fhirStu3TestData, fhirSubTemplate };
        }

        public static IEnumerable<object[]> GetNestedTemplateCollection()
        {
            var rootTemplate = @"{% include 'Sub/Template1' -%}";
            var subTemplate = @"{""root"":""subtemplate1""}";
            var folder1SubTemplate = @"{""Folder1"":""subtemplate1""}";
            var folder2SubTemplate = @"{""Folder2"":""subtemplate1""}";

            var templateCollection = new List<Dictionary<string, Template>>
            {
                new Dictionary<string, Template>
                {
                    { "Template1", Template.Parse(rootTemplate) },
                    { "Sub/Template1", Template.Parse(subTemplate) },
                    { "Folder1/Template1", Template.Parse(rootTemplate) },
                    { "Folder1/Sub/Template1", Template.Parse(folder1SubTemplate) },
                    { "Folder2/Template1", Template.Parse(rootTemplate) },
                    { "Folder2/Sub/Template1", Template.Parse(folder2SubTemplate) },
                },
            };

            yield return new object[] { _hl7v2Processor, new TemplateProvider(templateCollection), _hl7v2TestData, subTemplate, folder1SubTemplate, folder2SubTemplate };
            yield return new object[] { _ccdaProcessor, new TemplateProvider(templateCollection), _ccdaTestData, subTemplate, folder1SubTemplate, folder2SubTemplate };
            yield return new object[] { _jsonProcessor, new TemplateProvider(templateCollection), _jsonTestData, subTemplate, folder1SubTemplate, folder2SubTemplate };
            yield return new object[] { _fhirProcessor, new TemplateProvider(templateCollection), _fhirStu3TestData, subTemplate, folder1SubTemplate, folder2SubTemplate };
        }

        public static IEnumerable<object[]> GetValidInputsWithProcessSettings()
        {
            var positiveTimeOutSettings = new ProcessorSettings
            {
                TimeOut = 1, // expect operation to timeout after 1ms
            };

            var negativeTimeOutSettings = new ProcessorSettings
            {
                TimeOut = -1, // expect operation to not timeout
            };

            yield return new object[]
            {
                new Hl7v2Processor(new ProcessorSettings(), FhirConverterLogging.CreateLogger<Hl7v2Processor>()), new Hl7v2Processor(positiveTimeOutSettings, FhirConverterLogging.CreateLogger<Hl7v2Processor>()), new Hl7v2Processor(negativeTimeOutSettings, FhirConverterLogging.CreateLogger<Hl7v2Processor>()),
                new TemplateProvider(TestConstants.TestTemplateDirectory, DataType.Hl7v2), _hl7v2TestData,
            };
            yield return new object[]
            {
                new CcdaProcessor(new ProcessorSettings(), FhirConverterLogging.CreateLogger<CcdaProcessor>()), new CcdaProcessor(positiveTimeOutSettings, FhirConverterLogging.CreateLogger<CcdaProcessor>()), new CcdaProcessor(negativeTimeOutSettings, FhirConverterLogging.CreateLogger<CcdaProcessor>()),
                new TemplateProvider(TestConstants.TestTemplateDirectory, DataType.Ccda), _ccdaTestData,
            };
            yield return new object[]
            {
                new JsonProcessor(new ProcessorSettings(), FhirConverterLogging.CreateLogger<JsonProcessor>()), new JsonProcessor(positiveTimeOutSettings, FhirConverterLogging.CreateLogger<JsonProcessor>()), new JsonProcessor(negativeTimeOutSettings, FhirConverterLogging.CreateLogger<JsonProcessor>()),
                new TemplateProvider(TestConstants.TestTemplateDirectory, DataType.Json), _jsonTestData,
            };
            yield return new object[]
            {
                new FhirProcessor(new ProcessorSettings(), FhirConverterLogging.CreateLogger<FhirProcessor>()), new FhirProcessor(positiveTimeOutSettings, FhirConverterLogging.CreateLogger<FhirProcessor>()), new FhirProcessor(negativeTimeOutSettings, FhirConverterLogging.CreateLogger<FhirProcessor>()),
                new TemplateProvider(TestConstants.TestTemplateDirectory, DataType.Fhir), _fhirStu3TestData,
            };
        }

        public static IEnumerable<object[]> GetValidInputsWithLargeForLoop()
        {
            yield return new object[]
            {
                _hl7v2Processor,
                new TemplateProvider(TestConstants.TestTemplateDirectory, DataType.Hl7v2),
                _hl7v2TestData,
            };
            yield return new object[]
            {
                _ccdaProcessor,
                new TemplateProvider(TestConstants.TestTemplateDirectory, DataType.Ccda),
                _ccdaTestData,
            };
            yield return new object[]
            {
                _jsonProcessor,
                new TemplateProvider(TestConstants.TestTemplateDirectory, DataType.Json),
                _jsonTestData,
            };
            yield return new object[]
            {
                _fhirProcessor,
                new TemplateProvider(TestConstants.TestTemplateDirectory, DataType.Fhir),
                _fhirStu3TestData,
            };
        }

        public static IEnumerable<object[]> GetValidInputsWithNestingTooDeep()
        {
            yield return new object[]
            {
                _hl7v2Processor,
                new TemplateProvider(TestConstants.TestTemplateDirectory, DataType.Hl7v2),
                _hl7v2TestData,
            };
            yield return new object[]
            {
                _ccdaProcessor,
                new TemplateProvider(TestConstants.TestTemplateDirectory, DataType.Ccda),
                _ccdaTestData,
            };
            yield return new object[]
            {
                _jsonProcessor,
                new TemplateProvider(TestConstants.TestTemplateDirectory, DataType.Json),
                _jsonTestData,
            };
            yield return new object[]
            {
                _fhirProcessor,
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
        [MemberData(nameof(GetMockDefaultTemplateCollection))]
        public void GivenDefaultTemplateCollection_WhenConvert_CorrectResultShouldBeReturned(IFhirConverter processor, ITemplateProvider templateProvider, string data, string expectedTemplate)
        {
            var result = processor.Convert(data, "Template1", templateProvider);
            Assert.Equal(expectedTemplate, Regex.Replace(result, @"\s", string.Empty));

            result = processor.Convert(data, "Sub/Template1", templateProvider);
            Assert.Equal(expectedTemplate, Regex.Replace(result, @"\s", string.Empty));

            var exception = Assert.Throws<RenderException>(() => processor.Convert(data, "NonExistentTemplateName", templateProvider));
            Assert.Equal(FhirConverterErrorCode.TemplateNotFound, exception.FhirConverterErrorCode);
        }

        [Theory]
        [MemberData(nameof(GetNestedTemplateCollection))]
        public void GivenNestedTemplateCollection_WhenConvert_CorrectResultShouldBeReturned(IFhirConverter processor, ITemplateProvider templateProvider, string data, string expectedSubTemplate, string expectedFolder1SubTemplate, string expectedFolder2SubTemplate)
        {
            var result = processor.Convert(data, "Template1", templateProvider);
            Assert.Equal(expectedSubTemplate, Regex.Replace(result, @"\s", string.Empty));

            result = processor.Convert(data, "Folder1/Template1", templateProvider);
            Assert.Equal(expectedFolder1SubTemplate, Regex.Replace(result, @"\s", string.Empty));

            result = processor.Convert(data, "Folder2/Template1", templateProvider);
            Assert.Equal(expectedFolder2SubTemplate, Regex.Replace(result, @"\s", string.Empty));

            result = processor.Convert(data, "Folder2/Sub/Template1", templateProvider);
            Assert.Equal(expectedFolder2SubTemplate, Regex.Replace(result, @"\s", string.Empty));

            var exception = Assert.Throws<RenderException>(() => processor.Convert(data, "NonExistentTemplateName", templateProvider));
            Assert.Equal(FhirConverterErrorCode.TemplateNotFound, exception.FhirConverterErrorCode);
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
            try
            {
                var exception = Assert.Throws<RenderException>(() => positiveTimeOutProcessor.Convert(data, "TimeOutTemplate", templateProvider));
                Assert.Equal(FhirConverterErrorCode.TimeoutError, exception.FhirConverterErrorCode);
                Assert.True(exception.InnerException is OperationCanceledException);
            }
            catch (Xunit.Sdk.ThrowsException)
            {
                Console.WriteLine("A RenderException was not thrown because the cancellation token timeout of 1ms was not reached");
            }

            // Negative time out ProcessorSettings: no time out
            result = negativeTimeOutProcessor.Convert(data, "TimeOutTemplate", templateProvider);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void GivenJObjectInput_WhenConvertWithJsonProcessor_CorrectResultShouldBeReturned()
        {
            var processor = _jsonProcessor;
            var templateProvider = new TemplateProvider(TestConstants.JsonTemplateDirectory, DataType.Json);
            var testData = JsonConvert.DeserializeObject<JObject>(_jsonTestData, new JsonSerializerSettings() { DateParseHandling = DateParseHandling.None });
            var result = processor.Convert(testData, "ExamplePatient", templateProvider);
            Assert.True(JToken.DeepEquals(JObject.Parse(_jsonExpectData), JToken.Parse(result)));
        }

        [Fact]
        public void GivenJObjectInput_WhenConvertWithJsonToHl7v2Processor_CorrectResultShouldBeReturned()
        {
            var processor = new FhirToHl7v2Processor(_processorSettings, FhirConverterLogging.CreateLogger<FhirToHl7v2Processor>());
            var templateProvider = new TemplateProvider(TestConstants.TestTemplateDirectory, DataType.Json);
            var testData = JObject.Parse(_fhirBundleTestData);
            var result = processor.Convert(testData, "BundleToHl7v2", templateProvider);
            Assert.Equal(_fhirBundleExpectedData, result);
        }

        [Fact]
        public void GivenJObjectInput_WhenConvertWithJsonToHl7v2ProcessorWithInvalidTemplate_CorrectExceptionShouldBeThrown()
        {
            var processor = new FhirToHl7v2Processor(_processorSettings, FhirConverterLogging.CreateLogger<FhirToHl7v2Processor>());
            var templateProvider = new TemplateProvider(TestConstants.TestTemplateDirectory, DataType.Json);
            var testData = JObject.Parse(_fhirBundleTestData);
            var exception = Assert.Throws<RenderException>(() => processor.Convert(testData, "BundleToHl7v2_MissingRequiredProperty", templateProvider));
            Assert.Equal(FhirConverterErrorCode.PropertyNotFound, exception.FhirConverterErrorCode);
        }

        [Fact]
        public void GivenJObjectInput_WhenConvertWithJsonToHl7v2ProcessorWithInvalidMessageDefinitionSegment_CorrectExceptionShouldBeThrown()
        {
            var processor = new FhirToHl7v2Processor(_processorSettings, FhirConverterLogging.CreateLogger<FhirToHl7v2Processor>());
            var templateProvider = new TemplateProvider(TestConstants.TestTemplateDirectory, DataType.Json);
            var testData = JObject.Parse(_fhirBundleTestData);
            var exception = Assert.Throws<RenderException>(() => processor.Convert(testData, "BundleToHl7v2_InvalidSegmentObject", templateProvider));
            Assert.Equal(FhirConverterErrorCode.TemplateSyntaxError, exception.FhirConverterErrorCode);
        }

        [Fact]
        public void GivenJObjectInput_WhenConvertWithJsonToHl7v2ProcessorWithInvalidFieldNumber_CorrectExceptionShouldBeThrown()
        {
            var processor = new FhirToHl7v2Processor(_processorSettings, FhirConverterLogging.CreateLogger<FhirToHl7v2Processor>());
            var templateProvider = new TemplateProvider(TestConstants.TestTemplateDirectory, DataType.Json);
            var testData = JObject.Parse(_fhirBundleTestData);
            var exception = Assert.Throws<RenderException>(() => processor.Convert(testData, "BundleToHl7v2_InvalidFieldNumber", templateProvider));
            Assert.Equal(FhirConverterErrorCode.TemplateSyntaxError, exception.FhirConverterErrorCode);
        }
    }
}
