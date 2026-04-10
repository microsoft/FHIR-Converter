// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Processors;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.FunctionalTests
{
    public class FhirR4ConvertDataFunctionalTests
    {
        private static readonly ProcessorSettings _processorSettings = new ProcessorSettings();

        [Fact]
        public void GivenR4PatientWithVariables_WhenConvert_DragonIdentifierAdded()
        {
            var processor = new FhirR4Processor(_processorSettings, FhirConverterLogging.CreateLogger<FhirR4Processor>());
            var templateDirectory = Path.Join(Constants.TemplateDirectory, "FhirR4");
            var templateProvider = new TemplateProvider(templateDirectory, DataType.FhirR4);

            var inputContent = File.ReadAllText(Path.Join(Constants.SampleDataDirectory, "FhirR4", "PatientWithIdentifiers.json"));
            var expectedContent = File.ReadAllText(Path.Join(Constants.ExpectedDataFolder, "FhirR4", "DragonPatientMrn.json"));
            var variables = new Dictionary<string, string>
            {
                { "matchCode", "MR" },
                { "dragonSystem", "urn:oid:1.2.3.4.5.6.7.8.9-dragon-copilot" },
            };

            var result = processor.Convert(inputContent, "DragonPatientMrn", templateProvider, variables);

            var expectedObject = JObject.Parse(expectedContent);
            var actualObject = JObject.Parse(result);

            Assert.True(JToken.DeepEquals(expectedObject, actualObject));
        }

        [Fact]
        public void GivenR4PatientWithNoMatch_WhenConvert_RenderExceptionThrown()
        {
            var processor = new FhirR4Processor(_processorSettings, FhirConverterLogging.CreateLogger<FhirR4Processor>());
            var templateDirectory = Path.Join(Constants.TemplateDirectory, "FhirR4");
            var templateProvider = new TemplateProvider(templateDirectory, DataType.FhirR4);

            var inputContent = File.ReadAllText(Path.Join(Constants.SampleDataDirectory, "FhirR4", "PatientNoMR.json"));
            var variables = new Dictionary<string, string>
            {
                { "matchCode", "MR" },
                { "dragonSystem", "urn:oid:1.2.3.4.5.6.7.8.9-dragon-copilot" },
            };

            var exception = Assert.Throws<RenderException>(() =>
                processor.Convert(inputContent, "DragonPatientMrn", templateProvider, variables));
            Assert.Contains("No identifier found", exception.Message);
        }

        [Fact]
        public void GivenR4PatientWithDuplicateMatch_WhenConvert_RenderExceptionThrown()
        {
            var processor = new FhirR4Processor(_processorSettings, FhirConverterLogging.CreateLogger<FhirR4Processor>());
            var templateDirectory = Path.Join(Constants.TemplateDirectory, "FhirR4");
            var templateProvider = new TemplateProvider(templateDirectory, DataType.FhirR4);

            var inputContent = File.ReadAllText(Path.Join(Constants.SampleDataDirectory, "FhirR4", "PatientDuplicateMR.json"));
            var variables = new Dictionary<string, string>
            {
                { "matchCode", "MR" },
                { "dragonSystem", "urn:oid:1.2.3.4.5.6.7.8.9-dragon-copilot" },
            };

            var exception = Assert.Throws<RenderException>(() =>
                processor.Convert(inputContent, "DragonPatientMrn", templateProvider, variables));
            Assert.Contains("Multiple identifiers", exception.Message);
            Assert.Contains("MR", exception.Message);
        }

        [Fact]
        public void GivenR4PatientViaFactoryInterface_WhenConvert_CorrectResultReturned()
        {
            var factory = new ConvertProcessorFactory(new Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory());
            var converter = factory.GetProcessor(DataType.FhirR4, ConvertDataOutputFormat.Fhir);

            Assert.IsAssignableFrom<IFhirConverterWithVariables>(converter);

            var variableConverter = (IFhirConverterWithVariables)converter;
            var templateDirectory = Path.Join(Constants.TemplateDirectory, "FhirR4");
            var templateProvider = new TemplateProvider(templateDirectory, DataType.FhirR4);
            var inputContent = File.ReadAllText(Path.Join(Constants.SampleDataDirectory, "FhirR4", "PatientWithIdentifiers.json"));
            var variables = new Dictionary<string, string>
            {
                { "matchCode", "MR" },
                { "dragonSystem", "urn:oid:1.2.3.4.5.6.7.8.9-dragon-copilot" },
            };

            var result = variableConverter.Convert(inputContent, "DragonPatientMrn", templateProvider, variables);
            var resultObj = JObject.Parse(result);

            Assert.Equal("Patient", resultObj["resourceType"]?.ToString());
            var identifiers = resultObj["identifier"] as JArray;
            Assert.Equal(4, identifiers.Count);
        }

        [Fact]
        public void GivenR4PatientWithMissingDragonSystem_WhenConvert_RenderExceptionThrown()
        {
            var processor = new FhirR4Processor(_processorSettings, FhirConverterLogging.CreateLogger<FhirR4Processor>());
            var templateDirectory = Path.Join(Constants.TemplateDirectory, "FhirR4");
            var templateProvider = new TemplateProvider(templateDirectory, DataType.FhirR4);

            var inputContent = File.ReadAllText(Path.Join(Constants.SampleDataDirectory, "FhirR4", "PatientWithIdentifiers.json"));
            var variables = new Dictionary<string, string>
            {
                { "matchCode", "MR" },
            };

            var exception = Assert.Throws<RenderException>(() =>
                processor.Convert(inputContent, "DragonPatientMrn", templateProvider, variables));
            Assert.Contains("dragonSystem", exception.Message);
        }
    }
}
