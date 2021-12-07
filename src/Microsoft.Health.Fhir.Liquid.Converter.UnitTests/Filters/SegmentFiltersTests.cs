// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
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
            Assert.Throws<NullReferenceException>(() => Filters.GetFirstSegments(null, "PID"));
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
        public void GivenAnHl7v2Data_WhenSliceDataBySegments_CorrectResultShouldBeReturned()
        {
            Dictionary<string, List<Hl7v2Segment>> segmentList = Filters.GetSegmentLists(TestData, "PID|ORC|OBX");
            Dictionary<string, Hl7v2Segment> firstSegment = Filters.GetFirstSegments(TestData, "RXR");

            // Slicing data using segmentList and endSegment
            Filters.SliceDataBySegments(TestData, segmentList["ORC"], firstSegment["RXR"]);
            List<Hl7v2Data> pidSlicedData = Filters.SliceDataBySegments(TestData, segmentList["PID"], firstSegment["RXR"]);
            Assert.Single(pidSlicedData);
            string expectedPidValue = @"PID|1||90012^^^NIST-MPI-1^MR||Wong^Elise^^^^^L||19830615|F||2028-9^Asian^CDCREC|9200 Wellington Trail^^Bozeman^MT^59715^USA^P||^PRN^PH^^^406^5557896~^NET^^Elise.Wong@isp.com|||||||||2186-5^Not Hispanic or Latino^CDCREC||N|1|||||N" + System.Environment.NewLine + "PD1|||||||||||02^Reminder/recall - any method^HL70215|N|20150624|||A|19830615|20150624" + System.Environment.NewLine + "ORC|RE|4422^NIST-AA-IZ-2|13696^NIST-AA-IZ-2|||||||7824^Jackson^Lily^Suzanne^^^^^NIST-PI-1^L^^^PRN||654^Thomas^Wilma^Elizabeth^^^^^NIST-PI-1^L^^^MD|||||NISTEHRFAC^NISTEHRFacility^HL70362|" + System.Environment.NewLine + "RXA|0|1|20150624||49281-0215-88^TENIVAC^NDC|0.5|mL^mL^UCUM||00^New Record^NIP001|7824^Jackson^Lily^Suzanne^^^^^NIST-PI-1^L^^^PRN|^^^NIST-Clinic-1||||315841|20151216|PMC^Sanofi Pasteur^MVX|||CP|A" + System.Environment.NewLine;
            Assert.Equal(expectedPidValue, pidSlicedData[0].Value);
            Assert.Equal(4, pidSlicedData[0].Meta.Count);
            Assert.Equal(4, pidSlicedData[0].Data.Count);
            Assert.Equal("PID", pidSlicedData[0].Meta[0]);
            Assert.Equal("PD1", pidSlicedData[0].Meta[1]);
            Assert.Equal(@"PD1|||||||||||02^Reminder/recall - any method^HL70215|N|20150624|||A|19830615|20150624", pidSlicedData[0].Data[1].Value);

            // Sliced data could only contain single segment and endSegment cound be null
            List<Hl7v2Data> obxSlicedData = Filters.SliceDataBySegments(TestData, segmentList["OBX"]);
            Assert.Equal(4, obxSlicedData.Count);
            Assert.Single(obxSlicedData[0].Meta);
            Assert.Single(obxSlicedData[1].Meta);
            Assert.Single(obxSlicedData[2].Meta);
            Assert.Equal(5, obxSlicedData[3].Meta.Count);
            Assert.Single(obxSlicedData[0].Data);
            Assert.Single(obxSlicedData[1].Data);
            Assert.Single(obxSlicedData[2].Data);
            Assert.Equal(5, obxSlicedData[3].Data.Count);
            Assert.Equal("OBX", obxSlicedData[0].Meta[0]);
            Assert.Equal(@"OBX|1|CE|30963-3^Vaccine Funding Source^LN|1|PHC70^Private^CDCPHINVS||||||F|||20150624", obxSlicedData[0].Data[0].Value);
            string expectedObxValue = @"OBX|1|CE|30963-3^Vaccine Funding Source^LN|1|PHC70^Private^CDCPHINVS||||||F|||20150624" + System.Environment.NewLine;
            Assert.Equal(expectedObxValue, obxSlicedData[0].Value);

            // Sliced data could contain multiple segments
            List<Hl7v2Data> orcSlicedData = Filters.SliceDataBySegments(TestData, segmentList["ORC"]);
            Assert.Equal(3, orcSlicedData.Count);
            Assert.Equal(7, orcSlicedData[0].Meta.Count);
            Assert.Equal(2, orcSlicedData[1].Meta.Count);
            Assert.Equal(2, orcSlicedData[2].Meta.Count);
            Assert.Equal(7, orcSlicedData[0].Data.Count);
            Assert.Equal(2, orcSlicedData[1].Data.Count);
            Assert.Equal(2, orcSlicedData[2].Data.Count);
            Assert.Equal("ORC", orcSlicedData[0].Meta[0]);
            string expectedOrcValue = @"ORC|RE|4422^NIST-AA-IZ-2|13696^NIST-AA-IZ-2|||||||7824^Jackson^Lily^Suzanne^^^^^NIST-PI-1^L^^^PRN||654^Thomas^Wilma^Elizabeth^^^^^NIST-PI-1^L^^^MD|||||NISTEHRFAC^NISTEHRFacility^HL70362|";
            Assert.Equal(expectedOrcValue, orcSlicedData[0].Data[0].Value);

            // If segmentList is nonexistent in data, an empty List<Hl7v2Data> will be returned.
            Assert.Empty(Filters.SliceDataBySegments(orcSlicedData[0], segmentList["PID"]));

            // If segmentList is an empty list, an empty List<Hl7v2Data> will be returned.
            Assert.Empty(Filters.SliceDataBySegments(TestData, new List<Hl7v2Segment>()));

            // Hl7v2Data and segmentList could not be null
            Assert.Throws<NullReferenceException>(() => Filters.SliceDataBySegments(null, segmentList["ORC"]));
            Assert.Throws<NullReferenceException>(() => Filters.SliceDataBySegments(TestData, null));
        }

        private static Hl7v2Data LoadTestData()
        {
            var parser = new Hl7v2DataParser();
            var data = parser.Parse(TestDataContent);
            return data as Hl7v2Data;
        }
    }
}
