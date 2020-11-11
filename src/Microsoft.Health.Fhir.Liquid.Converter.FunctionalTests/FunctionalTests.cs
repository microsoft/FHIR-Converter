// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
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
        public static IEnumerable<object[]> GetDataForADTA01()
        {
            yield return new object[] { @"..\..\..\..\..\data\SampleData\Hl7v2\ADT01-23.hl7", @"TestData\Expected\Hl7v2\ADT_A01\ADT01-23-expected.json" };
            yield return new object[] { @"..\..\..\..\..\data\SampleData\Hl7v2\ADT01-28.hl7", @"TestData\Expected\Hl7v2\ADT_A01\ADT01-28-expected.json" };
            yield return new object[] { @"..\..\..\..\..\data\SampleData\Hl7v2\ADT04-23.hl7", @"TestData\Expected\Hl7v2\ADT_A01\ADT04-23-expected.json" };
            yield return new object[] { @"..\..\..\..\..\data\SampleData\Hl7v2\ADT04-251.hl7", @"TestData\Expected\Hl7v2\ADT_A01\ADT04-251-expected.json" };
            yield return new object[] { @"..\..\..\..\..\data\SampleData\Hl7v2\ADT04-28.hl7", @"TestData\Expected\Hl7v2\ADT_A01\ADT04-28-expected.json" };
        }

        public static IEnumerable<object[]> GetDataForVXUV04()
        {
            yield return new object[] { @"..\..\..\..\..\data\SampleData\Hl7v2\IZ_1_1.1_Admin_Child_Max_Message.hl7", @"TestData\Expected\Hl7v2\VXU_V04\IZ_1_1.1_Admin_Child_Max_Message-expected.json" };
            yield return new object[] { @"..\..\..\..\..\data\SampleData\Hl7v2\VXU.hl7", @"TestData\Expected\Hl7v2\VXU_V04\VXU-expected.json" };
        }

        public static IEnumerable<object[]> GetDataForORUR01()
        {
            yield return new object[] { @"..\..\..\..\..\data\SampleData\Hl7v2\LAB-ORU-1.hl7", @"TestData\Expected\Hl7v2\ORU_R01\LAB-ORU-1-expected.json" };
            yield return new object[] { @"..\..\..\..\..\data\SampleData\Hl7v2\LAB-ORU-2.hl7", @"TestData\Expected\Hl7v2\ORU_R01\LAB-ORU-2-expected.json" };
            yield return new object[] { @"..\..\..\..\..\data\SampleData\Hl7v2\LRI_2.0-NG_CBC_Typ_Message.hl7", @"TestData\Expected\Hl7v2\ORU_R01\LRI_2.0-NG_CBC_Typ_Message-expected.json" };
            yield return new object[] { @"..\..\..\..\..\data\SampleData\Hl7v2\ORU-R01-RMGEAD.hl7", @"TestData\Expected\Hl7v2\ORU_R01\ORU-R01-RMGEAD-expected.json" };
        }

        public static IEnumerable<object[]> GetDataForOMLO21()
        {
            yield return new object[] { @"..\..\..\..\..\data\SampleData\Hl7v2\MDHHS-OML-O21-1.hl7", @"TestData\Expected\Hl7v2\OML_O21\MDHHS-OML-O21-1-expected.json" };
            yield return new object[] { @"..\..\..\..\..\data\SampleData\Hl7v2\MDHHS-OML-O21-2.hl7", @"TestData\Expected\Hl7v2\OML_O21\MDHHS-OML-O21-2-expected.json" };
        }

        [Theory]
        [MemberData(nameof(GetDataForADTA01))]
        public void GivenADTA01Message_WhenConverting_ExpectedFhirResourceShouldBeReturned(string inputFile, string expectedFile)
        {
            TestByTemplate(inputFile, expectedFile, "ADT_A01");
        }

        [Theory]
        [MemberData(nameof(GetDataForVXUV04))]
        public void GivenVXUV04Message_WhenConverting_ExpectedFhirResourceShouldBeReturned(string inputFile, string expectedFile)
        {
            TestByTemplate(inputFile, expectedFile, "VXU_V04");
        }

        [Theory]
        [MemberData(nameof(GetDataForORUR01))]
        public void GivenORUR01Message_WhenConverting_ExpectedFhirResourceShouldBeReturned(string inputFile, string expectedFile)
        {
            TestByTemplate(inputFile, expectedFile, "ORU_R01");
        }

        [Theory]
        [MemberData(nameof(GetDataForOMLO21))]
        public void GivenOMLO21Message_WhenConverting_ExpectedFhirResourceShouldBeReturned(string inputFile, string expectedFile)
        {
            TestByTemplate(inputFile, expectedFile, "OML_O21");
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

        private void TestByTemplate(string inputFile, string expectedFile, string entryTemplate)
        {
            var hl7v2Processor = new Hl7v2Processor();
            var templateDirectory = Path.Join(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\..\data\Templates\Hl7v2");

            var inputContent = File.ReadAllText(inputFile);
            var expectedContent = File.ReadAllText(expectedFile);
            var actualContent = hl7v2Processor.Convert(inputContent, entryTemplate, new Hl7v2TemplateProvider(templateDirectory));

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
    }
}
