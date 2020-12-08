// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Health.Fhir.Liquid.Converter.Hl7v2;
using Microsoft.Health.Fhir.Liquid.Converter.Hl7v2.Models;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.Hl7v2.Models
{
    public class Hl7v2TraceInfoTests
    {
        [Fact]
        public void GivenHl7v2Data_WhenCreate_CorrectHl7v2TraceInfoShouldBeReturned()
        {
            // Null Hl7v2Data
            var traceInfo = Hl7v2TraceInfo.CreateTraceInfo(null);
            Assert.Empty(traceInfo.UnusedSegments);

            // Empty Hl7v2Data
            var data = new Hl7v2Data();
            traceInfo = Hl7v2TraceInfo.CreateTraceInfo(data);
            Assert.Empty(traceInfo.UnusedSegments);

            // Null data
            data = new Hl7v2Data()
            {
                Meta = null,
                Data = null,
            };
            traceInfo = Hl7v2TraceInfo.CreateTraceInfo(data);
            Assert.Empty(traceInfo.UnusedSegments);

            // Null segment
            data = new Hl7v2Data()
            {
                Meta = new List<string>() { null },
                Data = new List<Hl7v2Segment>() { null },
            };
            traceInfo = Hl7v2TraceInfo.CreateTraceInfo(data);
            Assert.Empty(traceInfo.UnusedSegments);

            // Valid Hl7v2Data before render
            var content = @"MSH|^~\&|AccMgr|1|||20050110045504||ADT^A01|599102|P|2.3||| 
PID|1||10006579^^^1^MR^1||DUCK^DONALD^D||19241010|M||1|111 DUCK ST^^FOWL^CA^999990000^^M|1|8885551212|8885551212|1|2||40007716^^^AccMgr^VN^1|123121234|||||||||||NO ";
            var parser = new Hl7v2DataParser();
            data = parser.Parse(content);
            traceInfo = Hl7v2TraceInfo.CreateTraceInfo(data);
            Assert.Equal(2, traceInfo.UnusedSegments.Count);
            Assert.Equal(15, traceInfo.UnusedSegments[1].Field.Count);

            // Valid Hl7v2Data after render
            var processor = new Hl7v2Processor();
            var templateProvider = new Hl7v2TemplateProvider(@"..\..\..\..\..\data\Templates\Hl7v2");
            _ = processor.Convert(content, "ADT_A01", templateProvider, traceInfo);
            Assert.Equal(2, traceInfo.UnusedSegments.Count);

            var unusedPid = traceInfo.UnusedSegments[1];
            Assert.Equal("PID", unusedPid.Type);
            Assert.Equal(1, unusedPid.Line);
            Assert.Equal(3, unusedPid.Field.Count);
            Assert.Equal(1, unusedPid.Field[0].Index);
            Assert.Single(unusedPid.Field[0].Component);
            Assert.Equal(1, unusedPid.Field[0].Index);
            Assert.Equal("1", unusedPid.Field[0].Component[0].Value);
        }
    }
}
