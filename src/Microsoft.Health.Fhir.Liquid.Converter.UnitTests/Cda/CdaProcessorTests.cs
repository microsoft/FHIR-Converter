// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.Cda;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.Cda
{
    public class CdaProcessorTests
    {
        private static readonly string TestData;

        static CdaProcessorTests()
        {
            TestData = File.ReadAllText(Path.Join(Constants.SampleDataDirectory, "Cda", "CCD.cda"));
        }

        [Fact]
        public void GivenAValidTemplateDirectory_WhenConvert_CorrectResultShouldBeReturned()
        {
            var processor = new CdaProcessor();
            var templateProvider = new CdaTemplateProvider(Constants.CdaTemplateDirectory);
            var result = processor.Convert(TestData, "CCD", templateProvider);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void GivenAValidTemplateCollection_WhenConvert_CorrectResultShouldBeReturned()
        {
            var processor = new CdaProcessor();
            var templateCollection = new List<Dictionary<string, Template>>
            {
                new Dictionary<string, Template>
                {
                    { "TemplateName", Template.Parse(@"{""a"":""b""}") },
                },
            };

            var templateProvider = new CdaTemplateProvider(templateCollection);
            var result = processor.Convert(TestData, "TemplateName", templateProvider);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void GivenInvalidTemplateProviderOrName_WhenConvert_ExceptionsShouldBeThrown()
        {
            var processor = new CdaProcessor();
            var templateCollection = new List<Dictionary<string, Template>>
            {
                new Dictionary<string, Template>
                {
                    { "TemplateName", Template.Parse(@"{""a"":""b""}") },
                },
            };

            var templateProvider = new CdaTemplateProvider(templateCollection);

            // Null, empty or nonexistent root template
            var exception = Assert.Throws<RenderException>(() => processor.Convert(TestData, null, templateProvider));
            Assert.Equal(FhirConverterErrorCode.NullOrEmptyRootTemplate, exception.FhirConverterErrorCode);

            exception = Assert.Throws<RenderException>(() => processor.Convert(TestData, string.Empty, templateProvider));
            Assert.Equal(FhirConverterErrorCode.NullOrEmptyRootTemplate, exception.FhirConverterErrorCode);

            exception = Assert.Throws<RenderException>(() => processor.Convert(TestData, "NonExistentTemplateName", templateProvider));
            Assert.Equal(FhirConverterErrorCode.TemplateNotFound, exception.FhirConverterErrorCode);

            // Null TemplateProvider
            exception = Assert.Throws<RenderException>(() => processor.Convert(TestData, "TemplateName", null));
            Assert.Equal(FhirConverterErrorCode.NullTemplateProvider, exception.FhirConverterErrorCode);
        }

        [Fact]
        public void GivenProcessorSettings_WhenConvert_CorrectResultsShouldBeReturned()
        {
            // Null ProcessorSettings: no time out
            var processor = new CdaProcessor(null);
            var templateProvider = new CdaTemplateProvider(Constants.CdaTemplateDirectory);
            var result = processor.Convert(TestData, "CCD", templateProvider);
            Assert.True(result.Length > 0);

            // Default ProcessorSettings: no time out
            processor = new CdaProcessor(new ProcessorSettings());
            result = processor.Convert(TestData, "CCD", templateProvider);
            Assert.True(result.Length > 0);

            // Positive time out ProcessorSettings: exception thrown when time out
            var settings = new ProcessorSettings()
            {
                TimeOut = 1,
            };

            processor = new CdaProcessor(settings);
            var exception = Assert.Throws<RenderException>(() => processor.Convert(TestData, "CCD", templateProvider));
            Assert.Equal(FhirConverterErrorCode.TimeoutError, exception.FhirConverterErrorCode);
            Assert.True(exception.InnerException is TimeoutException);

            // Negative time out ProcessorSettings: no time out
            settings = new ProcessorSettings()
            {
                TimeOut = -1,
            };

            processor = new CdaProcessor(settings);
            result = processor.Convert(TestData, "CCD", templateProvider);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void GivenCancellationToken_WhenConvert_CorrectResultsShouldBeReturned()
        {
            var processor = new CdaProcessor();
            var templateProvider = new CdaTemplateProvider(Constants.CdaTemplateDirectory);
            var cts = new CancellationTokenSource();
            var result = processor.Convert(TestData, "CCD", templateProvider, cts.Token);
            Assert.True(result.Length > 0);

            cts.Cancel();
            Assert.Throws<OperationCanceledException>(() => processor.Convert(TestData, "CCD", templateProvider, cts.Token));
        }
    }
}
