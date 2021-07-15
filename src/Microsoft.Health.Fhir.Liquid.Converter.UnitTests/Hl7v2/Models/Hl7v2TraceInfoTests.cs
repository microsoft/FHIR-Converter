﻿// -------------------------------------------------------------------------------------------------
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
            data = Hl7v2DataParser.Parse(content)[Constants.Hl7v2DataKey] as Hl7v2Data;
            traceInfo = Hl7v2TraceInfo.CreateTraceInfo(data);
            Assert.Equal(2, traceInfo.UnusedSegments.Count);
            Assert.Equal(27, traceInfo.UnusedSegments[1].Components.Count);

            // Specially test MSH unused segments
            Assert.Equal(9, traceInfo.UnusedSegments[0].Components[0].Start);
            Assert.Equal("AccMgr", traceInfo.UnusedSegments[0].Components[0].Value);
            Assert.Equal(15, traceInfo.UnusedSegments[0].Components[0].End);

            // Valid Hl7v2Data after render
            var processor = new Hl7v2Processor();
            var templateProvider = new Hl7v2TemplateProvider(TestConstants.Hl7v2TemplateDirectory);
            _ = processor.Convert(content, "ADT_A01", templateProvider, traceInfo);
            Assert.Single(traceInfo.UnusedSegments);

            var unusedPid = traceInfo.UnusedSegments[0];
            Assert.Equal("PID", unusedPid.Type);
            Assert.Equal(1, unusedPid.Line);
            Assert.Equal(4, unusedPid.Components.Count);
            Assert.Equal(129, unusedPid.Components[2].Start);
            Assert.Equal(135, unusedPid.Components[2].End);
            Assert.Equal("AccMgr", unusedPid.Components[2].Value);
        }
    }
}
