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
    public class FunctionalTests
    {
        public static IEnumerable<object[]> GetDataForHl7v2()
        {
            var data = new List<string[]>
            {
                new[] { @"ADT_A01", @"ADT01-23.hl7", @"ADT01-23-expected.json" },
                new[] { @"ADT_A01", @"ADT01-28.hl7", @"ADT01-28-expected.json" },
                new[] { @"ADT_A01", @"ADT04-23.hl7", @"ADT04-23-expected.json" },
                new[] { @"ADT_A01", @"ADT04-251.hl7", @"ADT04-251-expected.json" },
                new[] { @"ADT_A01", @"ADT04-28.hl7", @"ADT04-28-expected.json" },
                new[] { @"OML_O21", @"MDHHS-OML-O21-1.hl7", @"MDHHS-OML-O21-1-expected.json" },
                new[] { @"OML_O21", @"MDHHS-OML-O21-2.hl7", @"MDHHS-OML-O21-2-expected.json" },
                new[] { @"ORU_R01", @"LAB-ORU-1.hl7", @"LAB-ORU-1-expected.json" },
                new[] { @"ORU_R01", @"LAB-ORU-2.hl7", @"LAB-ORU-2-expected.json" },
                new[] { @"ORU_R01", @"LRI_2.0-NG_CBC_Typ_Message.hl7", @"LRI_2.0-NG_CBC_Typ_Message-expected.json" },
                new[] { @"ORU_R01", @"ORU-R01-RMGEAD.hl7", @"ORU-R01-RMGEAD-expected.json" },
                new[] { @"VXU_V04", @"IZ_1_1.1_Admin_Child_Max_Message.hl7", @"IZ_1_1.1_Admin_Child_Max_Message-expected.json" },
                new[] { @"VXU_V04", @"VXU.hl7", @"VXU-expected.json" },
            };
            return data.Select(item => new[]
            {
                item[0],
                Path.Join(Constants.SampleDataDirectory, "Hl7v2", item[1]),
                Path.Join(Constants.ExpectedDataFolder, "Hl7v2", item[0], item[2]),
            });
        }

        public static IEnumerable<object[]> GetDataForCcda()
        {
            var data = new List<string[]>
            {
                new[] { @"CCD", @"170.314B2_Amb_CCD.ccda", @"170.314B2_Amb_CCD-expected.json" },
                new[] { @"CCD", @"C-CDA_R2-1_CCD.xml.ccda", @"C-CDA_R2-1_CCD.xml-expected.json" },
                new[] { @"CCD", @"CCD.ccda", @"CCD-expected.json" },
                new[] { @"CCD", @"CCD-Parent-Document-Replace-C-CDAR2.1.ccda", @"CCD-Parent-Document-Replace-C-CDAR2.1-expected.json" },
                new[] { @"ConsultationNote", @"Care_Plan.ccda", @"Care_Plan-expected.json" },
                new[] { @"ConsultationNote", @"CDA_with_Embedded_PDF.ccda", @"CDA_with_Embedded_PDF-expected.json" },
                new[] { @"ConsultationNote", @"Consultation_Note.ccda", @"Consultation_Note-expected.json" },
                new[] { @"ConsultationNote", @"Unstructured_Document_embed.ccda", @"Unstructured_Document_embed-expected.json" },
                new[] { @"DischargeSummary", @"Discharge_Summary.ccda", @"Discharge_Summary-expected.json" },
                new[] { @"DischargeSummary", @"Consult-Document-Closing-Referral-C-CDAR2.1.ccda", @"Consult-Document-Closing-Referral-C-CDAR2.1-expected.json" },
                new[] { @"HistoryandPhysical", @"History_and_Physical.ccda", @"History_and_Physical-expected.json" },
                new[] { @"HistoryandPhysical", @"Diagnostic_Imaging_Report.ccda", @"Diagnostic_Imaging_Report-expected.json" },
                new[] { @"OperativeNote", @"Operative_Note.ccda", @"Operative_Note-expected.json" },
                new[] { @"OperativeNote", @"Patient-1.ccda", @"Patient-1-expected.json" },
                new[] { @"ProcedureNote", @"Procedure_Note.ccda", @"Procedure_Note-expected.json" },
                new[] { @"ProcedureNote", @"Patient-and-Provider-Organization-Direct-Address-C-CDAR2.1.ccda", @"Patient-and-Provider-Organization-Direct-Address-C-CDAR2.1-expected.json" },
                new[] { @"ProgressNote", @"Progress_Note.ccda", @"Progress_Note-expected.json" },
                new[] { @"ProgressNote", @"PROBLEMS_in_Empty_C-CDA_2.1-C-CDAR2.1.ccda", @"PROBLEMS_in_Empty_C-CDA_2.1-C-CDAR2.1-expected.json" },
                new[] { @"ReferralNote", @"Referral_Note.ccda", @"Referral_Note-expected.json" },
                new[] { @"ReferralNote", @"sample.ccda", @"sample-expected.json" },
                new[] { @"TransferSummary", @"Transfer_Summary.ccda", @"Transfer_Summary-expected.json" },
                new[] { @"TransferSummary", @"Unstructured_Document_reference.ccda", @"Unstructured_Document_reference-expected.json" },
            };
            return data.Select(item => new[]
            {
                item[0],
                Path.Join(Constants.SampleDataDirectory, "Ccda", item[1]),
                Path.Join(Constants.ExpectedDataFolder, "Ccda", item[0], item[2]),
            });
        }

        public static IEnumerable<object[]> GetDataForJson()
        {
            var data = new List<string[]>
            {
                new[] { @"ExamplePatient", @"ExamplePatient.json", @"ExamplePatient-expected.json" },
                new[] { @"Stu3ChargeItem", @"Stu3ChargeItem.json", @"Stu3ChargeItem-expected.json" },
            };
            return data.Select(item => new[]
            {
                item[0],
                Path.Join(Constants.SampleDataDirectory, "Json", item[1]),
                Path.Join(Constants.ExpectedDataFolder, "Json", item[0], item[2]),
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
            var actualContent = hl7v2Processor.Convert(inputContent, rootTemplate, new TemplateProvider(templateDirectory, DataType.Hl7v2), traceInfo);

            JsonSerializer serializer = new JsonSerializer();
            var expectedObject = serializer.Deserialize<JObject>(new JsonTextReader(new StringReader(expectedContent)));
            var actualObject = serializer.Deserialize<JObject>(new JsonTextReader(new StringReader(actualContent)));

            new List<string>
            {
                "$.entry[?(@.resource.resourceType=='Provenance')].resource.text.div",
            }.ForEach(path =>
            {
                expectedObject.SelectToken(path).Parent.Remove();
                actualObject.SelectToken(path).Parent.Remove();
            });

            Assert.True(JToken.DeepEquals(expectedObject, actualObject));
            Assert.True(traceInfo.UnusedSegments.Count > 0);
        }

        [Theory]
        [MemberData(nameof(GetDataForCcda))]
        public void GivenCcdaDocument_WhenConverting_ExpectedFhirResourceShouldBeReturned(string rootTemplate, string inputFile, string expectedFile)
        {
            var ccdaProcessor = new CcdaProcessor();
            var templateDirectory = Path.Join(AppDomain.CurrentDomain.BaseDirectory, Constants.TemplateDirectory, "Ccda");

            var inputContent = File.ReadAllText(inputFile);
            var expectedContent = File.ReadAllText(expectedFile);
            var actualContent = ccdaProcessor.Convert(inputContent, rootTemplate, new TemplateProvider(templateDirectory, DataType.Ccda));

            var expectedObject = JObject.Parse(expectedContent);
            var actualObject = JObject.Parse(actualContent);

            // Remove DocumentReference, where date is different every time conversion is run and gzip result is OS dependent
            expectedObject["entry"]?.Last()?.Remove();
            actualObject["entry"]?.Last()?.Remove();

            Assert.True(JToken.DeepEquals(expectedObject, actualObject));
        }

        [Theory]
        [MemberData(nameof(GetDataForJson))]
        public void GivenJsonData_WhenConverting_ExpectedFhirResourceShouldBeReturned(string rootTemplate, string inputFile, string expectedFile)
        {
            var jsonProcessor = new JsonProcessor();
            var templateDirectory = Path.Join(AppDomain.CurrentDomain.BaseDirectory, Constants.TemplateDirectory, "Json");

            var inputContent = File.ReadAllText(inputFile);
            var expectedContent = File.ReadAllText(expectedFile);
            var actualContent = jsonProcessor.Convert(inputContent, rootTemplate, new TemplateProvider(templateDirectory, DataType.Json));

            var expectedObject = JObject.Parse(expectedContent);
            var actualObject = JObject.Parse(actualContent);

            Assert.True(JToken.DeepEquals(expectedObject, actualObject));
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

            var exception = Assert.Throws<RenderException>(() => hl7v2Processor.Convert(@"MSH|^~\&|", "template", new TemplateProvider(templateCollection)));
            Assert.True(exception.InnerException is DotLiquid.Exceptions.StackLevelException);
        }
    }
}
