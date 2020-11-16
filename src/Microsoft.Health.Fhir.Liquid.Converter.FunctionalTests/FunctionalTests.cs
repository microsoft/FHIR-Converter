// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Hl7v2;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.FunctionalTests
{
    public class FunctionalTests
    {
        private static readonly string DataFolder = @"..\..\..\..\..\data\SampleData";
        private static readonly string ExpectedDataFolder = @"TestData\Expected";
        private static readonly string Hl7v2TemplateFolder = @"..\..\..\..\..\data\Templates\Hl7v2";

        public static IEnumerable<object[]> GetDataForHl7v2()
        {
            var data = new List<object[]>
            {
                new object[] { @"ADT_A01", @"ADT01-23.hl7", @"ADT01-23-expected.json" },
                new object[] { @"ADT_A01", @"ADT01-28.hl7", @"ADT01-28-expected.json" },
                new object[] { @"ADT_A01", @"ADT04-23.hl7", @"ADT04-23-expected.json" },
                new object[] { @"ADT_A01", @"ADT04-251.hl7", @"ADT04-251-expected.json" },
                new object[] { @"ADT_A01", @"ADT04-28.hl7", @"ADT04-28-expected.json" },
                new object[] { @"OML_O21", @"MDHHS-OML-O21-1.hl7", @"MDHHS-OML-O21-1-expected.json" },
                new object[] { @"OML_O21", @"MDHHS-OML-O21-2.hl7", @"MDHHS-OML-O21-2-expected.json" },
                new object[] { @"ORU_R01", @"LAB-ORU-1.hl7", @"LAB-ORU-1-expected.json" },
                new object[] { @"ORU_R01", @"LAB-ORU-2.hl7", @"LAB-ORU-2-expected.json" },
                new object[] { @"ORU_R01", @"LRI_2.0-NG_CBC_Typ_Message.hl7", @"LRI_2.0-NG_CBC_Typ_Message-expected.json" },
                new object[] { @"ORU_R01", @"ORU-R01-RMGEAD.hl7", @"ORU-R01-RMGEAD-expected.json" },
                new object[] { @"VXU_V04", @"IZ_1_1.1_Admin_Child_Max_Message.hl7", @"IZ_1_1.1_Admin_Child_Max_Message-expected.json" },
                new object[] { @"VXU_V04", @"VXU.hl7", @"VXU-expected.json" },
            };
            return data.Select(item => new object[]
            {
                Convert.ToString(item[0]),
                Path.Combine(DataFolder, "Hl7v2", Convert.ToString(item[1])),
                Path.Combine(ExpectedDataFolder, "Hl7v2", Convert.ToString(item[0]), Convert.ToString(item[2])),
            });
        }

        [Theory]
        [MemberData(nameof(GetDataForHl7v2))]
        public void GivenHl7v2Message_WhenConverting_ExpectedFhirResourceShouldBeReturned(string rootTemplate, string inputFile, string expectedFile)
        {
            var hl7v2Processor = new Hl7v2Processor();
            var templateDirectory = Path.Join(AppDomain.CurrentDomain.BaseDirectory, Hl7v2TemplateFolder);

            var inputContent = File.ReadAllText(inputFile);
            var expectedContent = File.ReadAllText(expectedFile);
            var actualContent = hl7v2Processor.Convert(inputContent, rootTemplate, new Hl7v2TemplateProvider(templateDirectory));

            // Remove ID
            var regex = new Regex(@"(?<=(""urn:uuid:|""|/))([A-Za-z0-9\-]{36})(?="")");
            expectedContent = regex.Replace(expectedContent, string.Empty);
            actualContent = regex.Replace(actualContent, string.Empty);

            // Normalize time zone
            JsonSerializer serializer = new JsonSerializer { DateTimeZoneHandling = DateTimeZoneHandling.Utc };
            var expectedObject = serializer.Deserialize<JObject>(new JsonTextReader(new StringReader(expectedContent)));
            var actualObject = serializer.Deserialize<JObject>(new JsonTextReader(new StringReader(actualContent)));
            Assert.True(JToken.DeepEquals(expectedObject, actualObject));
        }

        [Fact]
        public void GivenAnInvalidTemplate_WhenConverting_ExceptionsShouldBeThrown()
        {
            var hl7v2Processor = new Hl7v2Processor();
            var templateSet = new List<Dictionary<string, Template>>
            {
                new Dictionary<string, Template>
                {
                    { "template", Template.Parse("{% include 'template' -%}") },
                },
            };

            var exception = Assert.Throws<RenderException>(() => hl7v2Processor.Convert(@"MSH|^~\&|", "template", new Hl7v2TemplateProvider(templateSet)));
            Assert.True(exception.InnerException is DotLiquid.Exceptions.StackLevelException);
        }
    }
}
