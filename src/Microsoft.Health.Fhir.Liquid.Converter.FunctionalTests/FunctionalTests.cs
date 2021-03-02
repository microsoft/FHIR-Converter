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
using Microsoft.Health.Fhir.Liquid.Converter.Hl7v2.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.FunctionalTests
{
    public class FunctionalTests
    {
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
                Path.Join(Constants.SampleDataDirectory, "Hl7v2", Convert.ToString(item[1])),
                Path.Join(Constants.ExpectedDataFolder, "Hl7v2", Convert.ToString(item[0]), Convert.ToString(item[2])),
            });
        }

        [Theory]
        [MemberData(nameof(GetDataForHl7v2))]
        public void GivenHl7v2Message_WhenConverting_ExpectedFhirResourceShouldBeReturned(string rootTemplate, string inputFile, string expectedFile)
        {
            var hl7v2Processor = new Hl7v2Processor();
            var templateDirectory = Path.Join(AppDomain.CurrentDomain.BaseDirectory, Constants.TemplateDirectory, "Hl7v2");

            var inputContent = File.ReadAllText(inputFile);
            var expectedContent = File.ReadAllText(expectedFile);
            var traceInfo = new Hl7v2TraceInfo();
            var actualContent = hl7v2Processor.Convert(inputContent, rootTemplate, new Hl7v2TemplateProvider(templateDirectory), traceInfo);

            JsonSerializer serializer = new JsonSerializer();
            var expectedObject = serializer.Deserialize<JObject>(new JsonTextReader(new StringReader(expectedContent)));
            var actualObject = serializer.Deserialize<JObject>(new JsonTextReader(new StringReader(actualContent)));
            Assert.True(JToken.DeepEquals(expectedObject, actualObject));
            Assert.True(traceInfo.UnusedSegments.Count > 0);
        }

        [Fact]
        public void GivenAnInvalidTemplate_WhenConverting_ExceptionsShouldBeThrown()
        {
            var hl7v2Processor = new Hl7v2Processor();
            var templateCollection = new List<Dictionary<string, Template>>
            {
                new Dictionary<string, Template>
                {
                    { "template", Template.Parse("{% include 'template' -%}") },
                },
            };

            var exception = Assert.Throws<RenderException>(() => hl7v2Processor.Convert(@"MSH|^~\&|", "template", new Hl7v2TemplateProvider(templateCollection)));
            Assert.True(exception.InnerException is DotLiquid.Exceptions.StackLevelException);
        }

        [Fact]
        public void GivenEscapedMessage_WhenConverting_ExpectedCharacterShouldbeReturned()
        {
            var hl7v2Processor = new Hl7v2Processor();
            var templateDirectory = Path.Join(AppDomain.CurrentDomain.BaseDirectory, Constants.TemplateDirectory, "Hl7v2");
            var inputContent = string.Join("\n", new List<string>
            {
                @"MSH|^~\&|FOO|BAR|FOO|BAR|20201225000000|FOO|ADT^A01|123456|P|2.3|||||||||||",
                @"PR1|1|FOO|FOO^ESCAPED ONE \T\ ESCAPED TWO^BAR|ESCAPED THREE \T\ ESCAPED FOUR|20201225000000||||||||||",
            });
            var result = JObject.Parse(hl7v2Processor.Convert(inputContent, "ADT_A01", new Hl7v2TemplateProvider(templateDirectory)));

            var texts = result.SelectTokens("$.entry[?(@.resource.resourceType == 'Procedure')].resource.code.text").Select(Convert.ToString);
            var expected = new List<string> { "ESCAPED ONE & ESCAPED TWO", "ESCAPED THREE & ESCAPED FOUR" };
            Assert.NotEmpty(texts.Intersect(expected));
        }
    }
}
