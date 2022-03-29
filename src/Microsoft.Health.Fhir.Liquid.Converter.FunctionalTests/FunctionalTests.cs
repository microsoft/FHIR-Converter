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
                new[] { @"ADT_A01", @"ADT-A01-01.hl7", @"ADT-A01-01-expected.json" },
                new[] { @"ADT_A01", @"ADT-A01-02.hl7", @"ADT-A01-02-expected.json" },
                new[] { @"ADT_A02", @"ADT-A02-01.hl7", @"ADT-A02-01-expected.json" },
                new[] { @"ADT_A02", @"ADT-A02-02.hl7", @"ADT-A02-02-expected.json" },
                new[] { @"ADT_A03", @"ADT-A03-01.hl7", @"ADT-A03-01-expected.json" },
                new[] { @"ADT_A03", @"ADT-A03-02.hl7", @"ADT-A03-02-expected.json" },
                new[] { @"ADT_A04", @"ADT-A04-01.hl7", @"ADT-A04-01-expected.json" },
                new[] { @"ADT_A04", @"ADT-A04-02.hl7", @"ADT-A04-02-expected.json" },
                new[] { @"ADT_A05", @"ADT-A05-01.hl7", @"ADT-A05-01-expected.json" },
                new[] { @"ADT_A05", @"ADT-A05-02.hl7", @"ADT-A05-02-expected.json" },
                new[] { @"ADT_A06", @"ADT-A06-01.hl7", @"ADT-A06-01-expected.json" },
                new[] { @"ADT_A06", @"ADT-A06-02.hl7", @"ADT-A06-02-expected.json" },
                new[] { @"ADT_A07", @"ADT-A07-01.hl7", @"ADT-A07-01-expected.json" },
                new[] { @"ADT_A07", @"ADT-A07-02.hl7", @"ADT-A07-02-expected.json" },
                new[] { @"ADT_A08", @"ADT-A08-01.hl7", @"ADT-A08-01-expected.json" },
                new[] { @"ADT_A08", @"ADT-A08-02.hl7", @"ADT-A08-02-expected.json" },
                new[] { @"ADT_A09", @"ADT-A09-01.hl7", @"ADT-A09-01-expected.json" },
                new[] { @"ADT_A09", @"ADT-A09-02.hl7", @"ADT-A09-02-expected.json" },
                new[] { @"ADT_A10", @"ADT-A10-01.hl7", @"ADT-A10-01-expected.json" },
                new[] { @"ADT_A10", @"ADT-A10-02.hl7", @"ADT-A10-02-expected.json" },
                new[] { @"ADT_A11", @"ADT-A11-01.hl7", @"ADT-A11-01-expected.json" },
                new[] { @"ADT_A11", @"ADT-A11-02.hl7", @"ADT-A11-02-expected.json" },
                new[] { @"ADT_A13", @"ADT-A13-01.hl7", @"ADT-A13-01-expected.json" },
                new[] { @"ADT_A13", @"ADT-A13-02.hl7", @"ADT-A13-02-expected.json" },
                new[] { @"ADT_A14", @"ADT-A14-01.hl7", @"ADT-A14-01-expected.json" },
                new[] { @"ADT_A14", @"ADT-A14-02.hl7", @"ADT-A14-02-expected.json" },
                new[] { @"ADT_A15", @"ADT-A15-01.hl7", @"ADT-A15-01-expected.json" },
                new[] { @"ADT_A15", @"ADT-A15-02.hl7", @"ADT-A15-02-expected.json" },
                new[] { @"ADT_A16", @"ADT-A16-01.hl7", @"ADT-A16-01-expected.json" },
                new[] { @"ADT_A16", @"ADT-A16-02.hl7", @"ADT-A16-02-expected.json" },
                new[] { @"ADT_A25", @"ADT-A25-01.hl7", @"ADT-A25-01-expected.json" },
                new[] { @"ADT_A25", @"ADT-A25-02.hl7", @"ADT-A25-02-expected.json" },
                new[] { @"ADT_A26", @"ADT-A26-01.hl7", @"ADT-A26-01-expected.json" },
                new[] { @"ADT_A26", @"ADT-A26-02.hl7", @"ADT-A26-02-expected.json" },
                new[] { @"ADT_A27", @"ADT-A27-01.hl7", @"ADT-A27-01-expected.json" },
                new[] { @"ADT_A27", @"ADT-A27-02.hl7", @"ADT-A27-02-expected.json" },
                new[] { @"ADT_A28", @"ADT-A28-01.hl7", @"ADT-A28-01-expected.json" },
                new[] { @"ADT_A28", @"ADT-A28-02.hl7", @"ADT-A28-02-expected.json" },
                new[] { @"ADT_A29", @"ADT-A29-01.hl7", @"ADT-A29-01-expected.json" },
                new[] { @"ADT_A29", @"ADT-A29-02.hl7", @"ADT-A29-02-expected.json" },
                new[] { @"ADT_A31", @"ADT-A31-01.hl7", @"ADT-A31-01-expected.json" },
                new[] { @"ADT_A31", @"ADT-A31-02.hl7", @"ADT-A31-02-expected.json" },
                new[] { @"ADT_A40", @"ADT-A40-01.hl7", @"ADT-A40-01-expected.json" },
                new[] { @"ADT_A40", @"ADT-A40-02.hl7", @"ADT-A40-02-expected.json" },
                new[] { @"ADT_A41", @"ADT-A41-01.hl7", @"ADT-A41-01-expected.json" },
                new[] { @"ADT_A41", @"ADT-A41-02.hl7", @"ADT-A41-02-expected.json" },
                new[] { @"ADT_A45", @"ADT-A45-01.hl7", @"ADT-A45-01-expected.json" },
                new[] { @"ADT_A45", @"ADT-A45-02.hl7", @"ADT-A45-02-expected.json" },
                new[] { @"ADT_A47", @"ADT-A47-01.hl7", @"ADT-A47-01-expected.json" },
                new[] { @"ADT_A47", @"ADT-A47-02.hl7", @"ADT-A47-02-expected.json" },
                new[] { @"ADT_A60", @"ADT-A60-01.hl7", @"ADT-A60-01-expected.json" },
                new[] { @"ADT_A60", @"ADT-A60-02.hl7", @"ADT-A60-02-expected.json" },

                new[] { @"SIU_S12", @"SIU-S12-01.hl7", @"SIU-S12-01-expected.json" },
                new[] { @"SIU_S12", @"SIU-S12-02.hl7", @"SIU-S12-02-expected.json" },
                new[] { @"SIU_S13", @"SIU-S13-01.hl7", @"SIU-S13-01-expected.json" },
                new[] { @"SIU_S13", @"SIU-S13-02.hl7", @"SIU-S13-02-expected.json" },
                new[] { @"SIU_S14", @"SIU-S14-01.hl7", @"SIU-S14-01-expected.json" },
                new[] { @"SIU_S14", @"SIU-S14-02.hl7", @"SIU-S14-02-expected.json" },
                new[] { @"SIU_S15", @"SIU-S15-01.hl7", @"SIU-S15-01-expected.json" },
                new[] { @"SIU_S15", @"SIU-S15-02.hl7", @"SIU-S15-02-expected.json" },
                new[] { @"SIU_S16", @"SIU-S16-01.hl7", @"SIU-S16-01-expected.json" },
                new[] { @"SIU_S16", @"SIU-S16-02.hl7", @"SIU-S16-02-expected.json" },
                new[] { @"SIU_S17", @"SIU-S17-01.hl7", @"SIU-S17-01-expected.json" },
                new[] { @"SIU_S17", @"SIU-S17-02.hl7", @"SIU-S17-02-expected.json" },
                new[] { @"SIU_S26", @"SIU-S26-01.hl7", @"SIU-S26-01-expected.json" },
                new[] { @"SIU_S26", @"SIU-S26-02.hl7", @"SIU-S26-02-expected.json" },

                new[] { @"ORU_R01", @"ORU-R01-01.hl7",  @"ORU-R01-01-expected.json" },

                new[] { @"ORM_O01", @"ORM-O01-01.hl7", @"ORM-O01-01-expected.json" },
                new[] { @"ORM_O01", @"ORM-O01-02.hl7", @"ORM-O01-02-expected.json" },
                new[] { @"ORM_O01", @"ORM-O01-03.hl7", @"ORM-O01-03-expected.json" },
                new[] { @"ORM_O01", @"ORM-O01-04.hl7", @"ORM-O01-04-expected.json" },
                new[] { @"ORM_O01", @"ORM-O01-05.hl7", @"ORM-O01-05-expected.json" },
                new[] { @"ORM_O01", @"ORM-O01-06.hl7", @"ORM-O01-06-expected.json" },

                new[] { @"MDM_T01", @"MDM-T01-01.hl7",  @"MDM-T01-01-expected.json" },
                new[] { @"MDM_T01", @"MDM-T01-02.hl7",  @"MDM-T01-02-expected.json" },
                new[] { @"MDM_T02", @"MDM-T02-01.hl7",  @"MDM-T02-01-expected.json" },
                new[] { @"MDM_T02", @"MDM-T02-02.hl7",  @"MDM-T02-02-expected.json" },
                new[] { @"MDM_T02", @"MDM-T02-03.hl7",  @"MDM-T02-03-expected.json" },
                new[] { @"MDM_T05", @"MDM-T05-01.hl7",  @"MDM-T05-01-expected.json" },
                new[] { @"MDM_T05", @"MDM-T05-02.hl7",  @"MDM-T05-02-expected.json" },
                new[] { @"MDM_T06", @"MDM-T06-01.hl7",  @"MDM-T06-01-expected.json" },
                new[] { @"MDM_T06", @"MDM-T06-02.hl7",  @"MDM-T06-02-expected.json" },
                new[] { @"MDM_T09", @"MDM-T09-01.hl7",  @"MDM-T09-01-expected.json" },
                new[] { @"MDM_T09", @"MDM-T09-02.hl7",  @"MDM-T09-02-expected.json" },
                new[] { @"MDM_T10", @"MDM-T10-01.hl7",  @"MDM-T10-01-expected.json" },
                new[] { @"MDM_T10", @"MDM-T10-02.hl7",  @"MDM-T10-02-expected.json" },

                new[] { @"RDE_O11", @"RDE-O11-01.hl7", @"RDE-O11-01-expected.json" },
                new[] { @"RDE_O11", @"RDE-O11-02.hl7", @"RDE-O11-02-expected.json" },
                new[] { @"RDE_O25", @"RDE-O25-01.hl7", @"RDE-O25-01-expected.json" },
                new[] { @"RDE_O25", @"RDE-O25-02.hl7", @"RDE-O25-02-expected.json" },

                new[] { @"RDS_O13", @"RDS-O13-01.hl7", @"RDS-O13-01-expected.json" },
                new[] { @"RDS_O13", @"RDS-O13-02.hl7", @"RDS-O13-02-expected.json" },

                new[] { @"OML_O21", @"OML-O21-01.hl7",  @"OML-O21-01-expected.json" },
                new[] { @"OML_O21", @"OML-O21-02.hl7",  @"OML-O21-02-expected.json" },
                new[] { @"OML_O21", @"OML-O21-03.hl7",  @"OML-O21-03-expected.json" },

                new[] { @"OUL_R22", @"OUL-R22-01.hl7",  @"OUL-R22-01-expected.json" },
                new[] { @"OUL_R22", @"OUL-R22-02.hl7",  @"OUL-R22-02-expected.json" },
                new[] { @"OUL_R23", @"OUL-R23-01.hl7",  @"OUL-R23-01-expected.json" },
                new[] { @"OUL_R23", @"OUL-R23-02.hl7",  @"OUL-R23-02-expected.json" },
                new[] { @"OUL_R24", @"OUL-R24-01.hl7",  @"OUL-R24-01-expected.json" },
                new[] { @"OUL_R24", @"OUL-R24-02.hl7",  @"OUL-R24-02-expected.json" },

                new[] { @"VXU_V04", @"VXU-V04-01.hl7",  @"VXU-V04-01-expected.json" },
                new[] { @"VXU_V04", @"VXU-V04-02.hl7",  @"VXU-V04-02-expected.json" },

                new[] { @"BAR_P01", @"BAR-P01-01.hl7", @"BAR-P01-01-expected.json" },
                new[] { @"BAR_P01", @"BAR-P01-02.hl7", @"BAR-P01-02-expected.json" },
                new[] { @"BAR_P02", @"BAR-P02-01.hl7", @"BAR-P02-01-expected.json" },
                new[] { @"BAR_P02", @"BAR-P02-02.hl7", @"BAR-P02-02-expected.json" },
                new[] { @"BAR_P12", @"BAR-P12-01.hl7", @"BAR-P12-01-expected.json" },
                new[] { @"BAR_P12", @"BAR-P12-02.hl7", @"BAR-P12-02-expected.json" },

                new[] { @"DFT_P03", @"DFT-P03-01.hl7", @"DFT-P03-01-expected.json" },
                new[] { @"DFT_P03", @"DFT-P03-02.hl7", @"DFT-P03-02-expected.json" },
                new[] { @"DFT_P11", @"DFT-P11-01.hl7", @"DFT-P11-01-expected.json" },
                new[] { @"DFT_P11", @"DFT-P11-02.hl7", @"DFT-P11-02-expected.json" },

                new[] { @"OMG_O19", @"OMG-O19-01.hl7", @"OMG-O19-01-expected.json" },
                new[] { @"OMG_O19", @"OMG-O19-02.hl7", @"OMG-O19-02-expected.json" },

                new[] { @"REF_I12", @"REF-I12-01.hl7", @"REF-I12-01-expected.json" },
                new[] { @"REF_I12", @"REF-I12-02.hl7", @"REF-I12-02-expected.json" },
                new[] { @"REF_I14", @"REF-I14-01.hl7", @"REF-I14-01-expected.json" },
                new[] { @"REF_I14", @"REF-I14-02.hl7", @"REF-I14-02-expected.json" },

                new[] { @"ADT_A01", @"ADT01-23.hl7", @"ADT01-23-expected.json" },
                new[] { @"ADT_A01", @"ADT01-28.hl7", @"ADT01-28-expected.json" },
                new[] { @"ADT_A04", @"ADT04-23.hl7", @"ADT04-23-expected.json" },
                new[] { @"ADT_A04", @"ADT04-251.hl7", @"ADT04-251-expected.json" },
                new[] { @"ADT_A04", @"ADT04-28.hl7", @"ADT04-28-expected.json" },
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

        [Fact]
        public void GivenCcdaMessageForTimezoneTesting_WhenConvert_ExpectedResultShouldBeReturned()
        {
            var inputFile = Path.Combine("TestData", "TimezoneHandling", "Input", "CcdaTestTimezoneInput.ccda");
            var ccdaProcessor = new CcdaProcessor();
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
            var hl7v2Processor = new Hl7v2Processor();
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
