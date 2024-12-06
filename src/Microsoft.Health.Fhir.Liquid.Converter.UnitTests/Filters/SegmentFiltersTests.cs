// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models.Hl7v2;
using Microsoft.Health.Fhir.Liquid.Converter.Parsers;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.FilterTests
{
    public class SegmentFiltersTests
    {
        private const string TestDataContent = @"MSH|^~\&|NISTEHRAPP|NISTEHRFAC|NISTIISAPP|NISTIISFAC|20150624084727.655-0500||VXU^V04^VXU_V04|NIST-IZ-AD-2.1_Send_V04_Z22|P|2.5.1|||ER|AL|||||Z22^CDCPHINVS|NISTEHRFAC|NISTIISFAC
PID|1||90012^^^NIST-MPI-1^MR||Wong^Elise^^^^^L||19830615|F||2028-9^Asian^CDCREC|9200 Wellington Trail^^Bozeman^MT^59715^USA^P||^PRN^PH^^^406^5557896~^NET^^Elise.Wong@isp.com|||||||||2186-5^Not Hispanic or Latino^CDCREC||N|1|||||N
PD1|||||||||||02^Reminder/recall - any method^HL70215|N|20150624|||A|19830615|20150624
ORC|RE|4422^NIST-AA-IZ-2|13696^NIST-AA-IZ-2|||||||7824^Jackson^Lily^Suzanne^^^^^NIST-PI-1^L^^^PRN||654^Thomas^Wilma^Elizabeth^^^^^NIST-PI-1^L^^^MD|||||NISTEHRFAC^NISTEHRFacility^HL70362|
RXA|0|1|20150624||49281-0215-88^TENIVAC^NDC|0.5|mL^mL^UCUM||00^New Record^NIP001|7824^Jackson^Lily^Suzanne^^^^^NIST-PI-1^L^^^PRN|^^^NIST-Clinic-1||||315841|20151216|PMC^Sanofi Pasteur^MVX|||CP|A
RXR|C28161^Intramuscular^NCIT|RD^Right Deltoid^HL70163
OBX|1|CE|30963-3^Vaccine Funding Source^LN|1|PHC70^Private^CDCPHINVS||||||F|||20150624
OBX|2|CE|64994-7^Vaccine Funding Program Eligibility^LN|2|V01^Not VFC Eligible^HL70064||||||F|||20150624|||VXC40^per immunization^CDCPHINVS
OBX|3|CE|69764-9^Document Type^LN|3|253088698300028811170411^Tetanus/Diphtheria (Td) Vaccine VIS^cdcgs1vis||||||F|||20150624
OBX|4|DT|29769-7^Date Vis Presented^LN|3|20150624||||||F|||20150624
ORC|RE||38760^NIST-AA-IZ-2|||||||7824^Jackson^Lily^Suzanne^^^^^NIST-PI-1^L^^^PRN|||||||NISTEHRFAC^NISTEHRFacility^HL70362
RXA|0|1|20141012||88^influenza, unspecified formulation^CVX|999|||01^Historical Administration^NIP001|||||||||||CP|A
ORC|RE||35508^NIST-AA-IZ-2|||||||7824^Jackson^Lily^Suzanne^^^^^NIST-PI-1^L^^^PRN|||||||NISTEHRFAC^NISTEHRFacility^HL70362
RXA|0|1|20131112||88^influenza, unspecified formulation^CVX|999|||01^Historical Administration^NIP001|||||||||||CP|A";

        private static readonly Hl7v2Data TestData = LoadTestData();

        [Fact]
        public void GivenAnHl7v2Data_WhenGetFirstSegments_CorrectResultShouldBeReturned()
        {
            Assert.Empty(Filters.GetFirstSegments(new Hl7v2Data(), "PID"));

            Assert.Empty(Filters.GetFirstSegments(TestData, string.Empty));

            var segments = Filters.GetFirstSegments(TestData, "PID|PD1|PV1|ORC");
            Assert.Equal(@"PID|1||90012^^^NIST-MPI-1^MR||Wong^Elise^^^^^L||19830615|F||2028-9^Asian^CDCREC|9200 Wellington Trail^^Bozeman^MT^59715^USA^P||^PRN^PH^^^406^5557896~^NET^^Elise.Wong@isp.com|||||||||2186-5^Not Hispanic or Latino^CDCREC||N|1|||||N", segments["PID"].Value);
            Assert.Equal(@"PD1|||||||||||02^Reminder/recall - any method^HL70215|N|20150624|||A|19830615|20150624", segments["PD1"].Value);
            Assert.Equal(@"ORC|RE|4422^NIST-AA-IZ-2|13696^NIST-AA-IZ-2|||||||7824^Jackson^Lily^Suzanne^^^^^NIST-PI-1^L^^^PRN||654^Thomas^Wilma^Elizabeth^^^^^NIST-PI-1^L^^^MD|||||NISTEHRFAC^NISTEHRFacility^HL70362|", segments["ORC"].Value);
            Assert.True(!segments.ContainsKey("PV1"));

            // Hl7v2Data and segment id content could not be null
            Assert.Throws<TemplateLoadException>(() => Filters.GetFirstSegments(null, "PID"));
            Assert.Throws<NullReferenceException>(() => Filters.GetFirstSegments(new Hl7v2Data(), null));
        }

        [Fact]
        public void GivenAnHl7v2Data_WhenGetSegmentLists_CorrectResultShouldBeReturned()
        {
            Assert.Empty(Filters.GetSegmentLists(new Hl7v2Data(), "PID"));

            Assert.Empty(Filters.GetSegmentLists(TestData, string.Empty));

            var segments = Filters.GetSegmentLists(TestData, "PID|PV1|ORC|OBX");
            Assert.Single(segments["PID"]);
            Assert.Equal(3, segments["ORC"].Count);
            Assert.Equal(4, segments["OBX"].Count);
            Assert.True(!segments.ContainsKey("PV1"));

            // Hl7v2Data and segment id content could not be null
            Assert.Throws<NullReferenceException>(() => Filters.GetSegmentLists(null, "PID"));
            Assert.Throws<NullReferenceException>(() => Filters.GetSegmentLists(new Hl7v2Data(), null));
        }

        [Fact]
        public void GivenAnHl7v2Data_WhenGetRelatedSegmentList_CorrectResultShouldBeReturned()
        {
            Assert.Empty(Filters.GetRelatedSegmentList(new Hl7v2Data(), null, null));

            var firstSegments = Filters.GetFirstSegments(TestData, "ORC");
            var orcSegment = firstSegments["ORC"];

            var rxaSegments = Filters.GetRelatedSegmentList(TestData, orcSegment, "RXA")["RXA"];
            Assert.Single(rxaSegments);

            var obxSegments = Filters.GetRelatedSegmentList(TestData, rxaSegments.First(), "OBX")["OBX"];
            Assert.Equal(4, obxSegments.Count);

            var pidSegments = Filters.GetRelatedSegmentList(TestData, orcSegment, "FOO");
            Assert.True(!pidSegments.ContainsKey("FOO"));

            // Hl7v2Data could not be null
            Assert.Throws<NullReferenceException>(() => Filters.GetRelatedSegmentList(null, null, null));
        }

        [Fact]
        public void GivenAnHl7v2Data_WhenGetParentSegments_CorrectResultShouldBeReturned()
        {
            Assert.Empty(Filters.GetParentSegment(new Hl7v2Data(), "OBX", 3, "RXA"));

            var rxaSegment = Filters.GetParentSegment(TestData, "OBX", 3, "RXA")["RXA"];
            Assert.Equal(@"RXA|0|1|20150624||49281-0215-88^TENIVAC^NDC|0.5|mL^mL^UCUM||00^New Record^NIP001|7824^Jackson^Lily^Suzanne^^^^^NIST-PI-1^L^^^PRN|^^^NIST-Clinic-1||||315841|20151216|PMC^Sanofi Pasteur^MVX|||CP|A", rxaSegment.Value);

            Assert.Empty(Filters.GetParentSegment(TestData, "OBX", 4, "FOO"));

            // Hl7v2Data could not be null
            Assert.Throws<NullReferenceException>(() => Filters.GetParentSegment(null, "OBX", 3, "RXA"));
        }

        [Fact]
        public void GivenAnHl7v2Data_WhenHasSegments_CorrectResultShouldBeReturned()
        {
            Assert.False(Filters.HasSegments(new Hl7v2Data(), "PID"));

            Assert.False(Filters.HasSegments(TestData, string.Empty));
            Assert.True(Filters.HasSegments(TestData, "PID"));
            Assert.True(Filters.HasSegments(TestData, "PID|ORC|OBX"));
            Assert.False(Filters.HasSegments(TestData, "PID|ORC|OBX||"));

            // Hl7v2Data and segment id content could not be null
            Assert.Throws<NullReferenceException>(() => Filters.HasSegments(null, "PID"));
            Assert.Throws<NullReferenceException>(() => Filters.HasSegments(new Hl7v2Data(), null));
        }

        [Fact]
        public void GivenAnHl7v2Data_WhenSplitDataBySegments_CorrectResultShouldBeReturned()
        {
            List<Hl7v2Data> splitDataByOrc = Filters.SplitDataBySegments(TestData, "ORC");
            Assert.Equal(4, splitDataByOrc.Count);
            Assert.Equal(3, splitDataByOrc[0].Meta.Count);
            Assert.Equal(7, splitDataByOrc[1].Meta.Count);
            Assert.Equal(2, splitDataByOrc[2].Meta.Count);
            Assert.Equal(2, splitDataByOrc[3].Meta.Count);
            Assert.Equal(3, splitDataByOrc[0].Data.Count);
            Assert.Equal(7, splitDataByOrc[1].Data.Count);
            Assert.Equal(2, splitDataByOrc[2].Data.Count);
            Assert.Equal(2, splitDataByOrc[3].Data.Count);
            Assert.Equal("MSH", splitDataByOrc[0].Meta[0]);
            Assert.Equal("ORC", splitDataByOrc[1].Meta[0]);
            string expectedOrcValue = @"ORC|RE|4422^NIST-AA-IZ-2|13696^NIST-AA-IZ-2|||||||7824^Jackson^Lily^Suzanne^^^^^NIST-PI-1^L^^^PRN||654^Thomas^Wilma^Elizabeth^^^^^NIST-PI-1^L^^^MD|||||NISTEHRFAC^NISTEHRFacility^HL70362|";
            Assert.Equal(expectedOrcValue, splitDataByOrc[1].Data[0].Value);

            List<Hl7v2Data> splitDataByMsh = Filters.SplitDataBySegments(TestData, "MSH");
            Assert.Equal(2, splitDataByMsh.Count);
            Assert.Empty(splitDataByMsh[0].Meta);
            Assert.Empty(splitDataByMsh[0].Data);
            Assert.Equal(14, splitDataByMsh[1].Meta.Count);
            Assert.Equal(14, splitDataByMsh[1].Data.Count);

            List<Hl7v2Data> splitData = Filters.SplitDataBySegments(TestData, "PID|OBX");
            Assert.Equal(6, splitData.Count);
            Assert.Single(splitData[0].Meta);
            Assert.Equal(5, splitData[1].Meta.Count);
            Assert.Single(splitData[2].Meta);
            Assert.Single(splitData[3].Meta);
            Assert.Single(splitData[4].Meta);
            Assert.Equal(5, splitData[5].Meta.Count);
            Assert.Single(splitData[0].Data);
            Assert.Equal(5, splitData[1].Data.Count);
            Assert.Single(splitData[2].Data);
            Assert.Single(splitData[3].Data);
            Assert.Single(splitData[4].Data);
            Assert.Equal(5, splitData[5].Data.Count);
            Assert.Equal("MSH", splitData[0].Meta[0]);
            Assert.Equal("PID", splitData[1].Meta[0]);
            Assert.Equal("OBX", splitData[2].Meta[0]);
            Assert.Equal(@"OBX|1|CE|30963-3^Vaccine Funding Source^LN|1|PHC70^Private^CDCPHINVS||||||F|||20150624", splitData[2].Data[0].Value);

            splitData = Filters.SplitDataBySegments(TestData, "PID|||OBX|KKK|");
            Assert.Equal(6, splitData.Count);
            Assert.Equal("MSH", splitData[0].Meta[0]);
            Assert.Equal("PID", splitData[1].Meta[0]);
            Assert.Equal("OBX", splitData[2].Meta[0]);

            // If separators are not in the data or empty, a data list containing only the original data will be returned.
            Assert.StrictEqual(TestData, Filters.SplitDataBySegments(TestData, string.Empty)[0]);
            Assert.StrictEqual(TestData, Filters.SplitDataBySegments(TestData, "PV1")[0]);

            // Hl7v2Data and separators could not be null. If one of them is null, NullReferenceException will be thrown.
            Assert.Throws<NullReferenceException>(() => Filters.SplitDataBySegments(null, "ORC"));
            Assert.Throws<NullReferenceException>(() => Filters.SplitDataBySegments(TestData, null));
        }

        private static Hl7v2Data LoadTestData()
        {
            var parser = new Hl7v2DataParser();
            var data = parser.Parse(TestDataContent);
            return data as Hl7v2Data;
        }
    }
}
