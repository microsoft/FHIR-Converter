// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Processors;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Health.Fhir.Liquid.Converter.FunctionalTests
{
    public class ConvertDataTemplateCollectionProviderFunctionalTests : BaseConvertDataFunctionalTests, IClassFixture<TemplateCollectionProviderTestFixture>
    {
        private static readonly ProcessorSettings _processorSettings = new ProcessorSettings();
        private readonly TemplateCollectionProviderTestFixture _fixture;
        private readonly ITestOutputHelper _outputHelper;

        public ConvertDataTemplateCollectionProviderFunctionalTests(ITestOutputHelper outputHelper, TemplateCollectionProviderTestFixture fixture)
        {
            _outputHelper = outputHelper;
            _fixture = fixture;
        }

        [Theory]
        [MemberData(nameof(GetDataForHl7v2))]
        public void GivenHl7v2MessageAndDefaultTemplateProvider_WhenConvertCalled_ExpectedFhirResourceShouldBeReturned(string rootTemplate, string inputFile, string expectedFile)
        {
            ConvertHl7v2MessageAndValidateExpectedResponse(_fixture.TemplateProvider, rootTemplate, inputFile, expectedFile);
        }

        [Theory]
        [MemberData(nameof(GetDataForCcda))]
        public void GivenCcdaDocumentAndDefaultTemplateProvider_WhenConvertCalled_ExpectedFhirResourceShouldBeReturned(string rootTemplate, string inputFile, string expectedFile)
        {
            ConvertCCDAMessageAndValidateExpectedResponse(_fixture.TemplateProvider, rootTemplate, inputFile, expectedFile);
        }

        [Theory]
        [MemberData(nameof(GetDataForStu3ToR4))]
        public void GivenStu3FhirDataAndDefaultTemplateProvider_WhenConvertCalled_ExpectedR4FhirResourceShouldBeReturned(string rootTemplate, string inputFile, string expectedFile)
        {
            ConvertFHIRMessageAndValidateExpectedResponse(_fixture.TemplateProvider, rootTemplate, inputFile, expectedFile);
        }

        [Theory]
        [MemberData(nameof(GetDataForJson))]
        public void GivenJsonDataAndDefaultTemplateProvider_WhenConvertCalled_ExpectedFhirResourceShouldBeReturned(string rootTemplate, string inputFile, string expectedFile)
        {
            ConvertJsonMessageAndValidateExpectedResponse(_fixture.TemplateProvider, rootTemplate, inputFile, expectedFile);
        }

        [Theory]
        [MemberData(nameof(GetDataForFhirToHl7v2))]
        public void GivenFhirDataAndDefaultTemplateProvider_WhenConvertCalled_ExpectedHl7v2MessageShouldBeReturned(string rootTemplate, string inputFile, string expectedFile)
        {
            ConvertFhirBundleToHl7v2AndValidateExpectedResponse(_fixture.TemplateProvider, rootTemplate, inputFile, expectedFile);
        }

        [Fact]
        public void GivenAnInvalidTemplate_WhenConvertCalled_ExceptionsShouldBeThrown()
        {
            var hl7v2Processor = new Hl7v2Processor(_processorSettings, FhirConverterLogging.CreateLogger<Hl7v2Processor>());
            var templateCollection = new List<Dictionary<string, Template>>
            {
                new Dictionary<string, Template>
                {
                    { "template", Template.Parse("{% include 'template' -%}") },
                },
            };

            var exception = Assert.Throws<RenderException>(() => hl7v2Processor.Convert(@"MSH|^~\&|", "template", _fixture.TemplateProvider));
            Assert.Equal(FhirConverterErrorCode.TemplateNotFound, exception.FhirConverterErrorCode);
        }
    }
}
