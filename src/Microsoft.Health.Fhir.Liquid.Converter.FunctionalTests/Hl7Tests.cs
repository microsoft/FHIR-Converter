// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Models.Hl7v2;
using Microsoft.Health.Fhir.Liquid.Converter.Processors;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.FunctionalTests
{
    public class Hl7Tests
    {
        public static IEnumerable<object[]> GetDataForFhir()
        {
            var data = new List<string[]>
            {
                // rootTemplate, expected, input
                new[] { @"ADT_A08", @"ADT-A08-01-expected.hl7", @"ADT-A08-01-expected.json" },
            };
            return data.Select(item => new[]
            {
                item[0],
                Path.Join(Constants.ExpectedDataFolder, "FhirToHL7v2", item[0], item[1]),
                Path.Join(Constants.ExpectedDataFolder, "Hl7v2", item[0], item[2]),
            });
        }

        [Theory]
        [MemberData(nameof(GetDataForFhir))]
        public void GivenFhirMessage_WhenConverting_ExpectedHl7v2ResourceShouldBeReturned(string rootTemplate, string expectedFile, string inputFile)
        {
            var fhirProcessor = new FhirProcessor();
            var templateDirectory = Path.Join(AppDomain.CurrentDomain.BaseDirectory, Constants.TemplateDirectory, "FhirToHL7v2");

            var inputContent = File.ReadAllText(inputFile);
            var expectedContent = File.ReadAllText(expectedFile);

            var actualContent = fhirProcessor.Convert(inputContent, rootTemplate, new TemplateProvider(templateDirectory, DataType.Fhir));

            Assert.Equal(actualContent, expectedContent);

            Console.WriteLine(actualContent);
        }
    }
}