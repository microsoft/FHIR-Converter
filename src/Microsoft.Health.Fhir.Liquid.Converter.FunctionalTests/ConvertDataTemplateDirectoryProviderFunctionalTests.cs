// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Models.Hl7v2;
using Microsoft.Health.Fhir.Liquid.Converter.Processors;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Health.Fhir.Liquid.Converter.FunctionalTests
{
    public class ConvertDataTemplateDirectoryProviderFunctionalTests : BaseConvertDataFunctionalTests
    {
        private static readonly ProcessorSettings _processorSettings = new ProcessorSettings();
        private readonly ITestOutputHelper _outputHelper;

        public ConvertDataTemplateDirectoryProviderFunctionalTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public void GivenCcdaMessageForTimezoneTesting_WhenConvert_ExpectedResultShouldBeReturned()
        {
            var inputFile = Path.Combine("TestData", "TimezoneHandling", "Input", "CcdaTestTimezoneInput.ccda");
            var ccdaProcessor = new CcdaProcessor(_processorSettings, FhirConverterLogging.CreateLogger<CcdaProcessor>());
            var templateDirectory = Path.Join("TestData", "TimezoneHandling", "Template");

            var inputContent = File.ReadAllText(inputFile);
            var actualContent = ccdaProcessor.Convert(inputContent, "CcdaTestTimezoneTemplate", new TemplateProvider(templateDirectory, DataType.Ccda));

            var actualObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(actualContent);

            Assert.Equal("2001-01", actualObject["datetime1"]);
            Assert.Equal("2001-01-01", actualObject["datetime2"]);
            Assert.Equal("2001-01-01", actualObject["datetime3"]);
            Assert.Contains("2001-11-11T12:00:00", actualObject["datetime4"].ToString());
            Assert.Contains("2001-11-11T12:23:00", actualObject["datetime5"].ToString());
            Assert.Equal("2020-01-01T01:01:01+08:00", actualObject["datetime6"]);
        }

        [Fact]
        public void GivenHl7v2MessageForTimeZoneTesting_WhenConvert_ExpectedResultShouldBeReturned()
        {
            var inputFile = Path.Combine("TestData", "TimezoneHandling", "Input", "Hl7v2TestTimezoneInput.hl7v2");
            var hl7v2Processor = new Hl7v2Processor(_processorSettings, FhirConverterLogging.CreateLogger<Hl7v2Processor>());
            var templateDirectory = Path.Join("TestData", "TimezoneHandling", "Template");

            var inputContent = File.ReadAllText(inputFile);
            var traceInfo = new Hl7v2TraceInfo();
            var actualContent = hl7v2Processor.Convert(inputContent, "Hl7v2TestTimezoneTemplate", new TemplateProvider(templateDirectory, DataType.Hl7v2), traceInfo);

            var actualObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(actualContent);

            Assert.Equal("2001-01", actualObject["datetime1"]);
            Assert.Equal("2001-01-01", actualObject["datetime2"]);
            Assert.Equal("2001-01-01", actualObject["datetime3"]);
            Assert.Contains("2001-11-11T12:00:00", actualObject["datetime4"].ToString());
            Assert.Contains("2001-11-11T12:23:00", actualObject["datetime5"].ToString());
            Assert.Equal("2020-01-01T01:01:01+08:00", actualObject["datetime6"]);
        }

        [Theory]
        [MemberData(nameof(GetDataForHl7v2))]
        public void GivenHl7v2Message_WhenConverting_ExpectedFhirResourceShouldBeReturned(string rootTemplate, string inputFile, string expectedFile)
        {
            var templateDirectory = Path.Join(AppDomain.CurrentDomain.BaseDirectory, Constants.TemplateDirectory, "Hl7v2");
            var templateProvider = new TemplateProvider(templateDirectory, DataType.Hl7v2);

            ConvertHl7v2MessageAndValidateExpectedResponse(templateProvider, rootTemplate, inputFile, expectedFile);
        }

        [Theory]
        [MemberData(nameof(GetDataForCcda))]
        public void GivenCcdaDocument_WhenConverting_ExpectedFhirResourceShouldBeReturned(string rootTemplate, string inputFile, string expectedFile)
        {
            var templateDirectory = Path.Join(AppDomain.CurrentDomain.BaseDirectory, Constants.TemplateDirectory, "Ccda");
            var templateProvider = new TemplateProvider(templateDirectory, DataType.Ccda);

            ConvertCCDAMessageAndValidateExpectedResponse(templateProvider, rootTemplate, inputFile, expectedFile);
        }

        // TODO: uncomment once we have test data again
        // [Theory]
        // [MemberData(nameof(GetDataForEcr))]
        // public void GivenEcrDocument_WhenConverting_ExpectedFhirResourceShouldBeReturned(string rootTemplate, string inputFile, string expectedFile)
        // {
        //     var templateDirectory = Path.Join(AppDomain.CurrentDomain.BaseDirectory, Constants.TemplateDirectory, "eCR");
        //     var templateProvider = new TemplateProvider(templateDirectory, DataType.Ccda);

        //     ConvertCCDAMessageAndValidateExpectedResponse(templateProvider, rootTemplate, inputFile, expectedFile);
        // }

        [Theory]
        [MemberData(nameof(GetDataForStu3ToR4))]
        public void GivenStu3FhirData_WhenConverting_ExpectedR4FhirResourceShouldBeReturned(string rootTemplate, string inputFile, string expectedFile)
        {
            var fhirProcessor = new FhirProcessor(_processorSettings, FhirConverterLogging.CreateLogger<FhirProcessor>());
            var templateDirectory = Path.Join(AppDomain.CurrentDomain.BaseDirectory, Constants.TemplateDirectory, "Stu3ToR4");
            var templateProvider = new TemplateProvider(templateDirectory, DataType.Fhir);

            ConvertFHIRMessageAndValidateExpectedResponse(templateProvider, rootTemplate, inputFile, expectedFile);
        }

        [Theory]
        [MemberData(nameof(GetDataForJson))]
        public void GivenJsonData_WhenConverting_ExpectedFhirResourceShouldBeReturned(string rootTemplate, string inputFile, string expectedFile)
        {
            var templateDirectory = Path.Join(AppDomain.CurrentDomain.BaseDirectory, Constants.TemplateDirectory, "Json");
            var templateProvider = new TemplateProvider(templateDirectory, DataType.Json);

            ConvertJsonMessageAndValidateExpectedResponse(templateProvider, rootTemplate, inputFile, expectedFile);
        }

        [Fact]
        public void GivenAnInvalidTemplate_WhenConverting_ExceptionsShouldBeThrown()
        {
            var hl7v2Processor = new Hl7v2Processor(_processorSettings, FhirConverterLogging.CreateLogger<Hl7v2Processor>());
            var templateCollection = new List<Dictionary<string, Template>>
            {
                new Dictionary<string, Template>
                {
                    { "template", Template.Parse("{% include 'template' -%}") },
                },
            };

            var exception = Assert.Throws<RenderException>(() => hl7v2Processor.Convert(@"MSH|^~\&|", "template", new TemplateProvider(templateCollection)));
            Assert.True(exception.InnerException is DotLiquid.Exceptions.StackLevelException);
        }
    }
}
