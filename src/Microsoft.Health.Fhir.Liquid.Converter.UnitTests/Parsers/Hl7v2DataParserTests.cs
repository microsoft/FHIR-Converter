// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Health.Fhir.Liquid.Converter.DotLiquids;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Models.Hl7v2;
using Microsoft.Health.Fhir.Liquid.Converter.Parsers;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.Parsers
{
    public class Hl7v2DataParserTests
    {
        private readonly IDataParser _parser = new Hl7v2DataParser();

        public static IEnumerable<object[]> GetNullOrEmptyHl7v2Message()
        {
            yield return new object[] { null };
            yield return new object[] { string.Empty };
            yield return new object[] { " " };
            yield return new object[] { "\n" };
        }

        [Theory]
        [MemberData(nameof(GetNullOrEmptyHl7v2Message))]
        public void GivenNullOrEmptyHl7v2Message_WhenParse_ExceptionShouldBeThrown(string input)
        {
            var exception = Assert.Throws<DataParseException>(() => _parser.Parse(input));
            Assert.Equal(FhirConverterErrorCode.NullOrWhiteSpaceInput, exception.FhirConverterErrorCode);
        }

        [Fact]
        public void GivenValidHl7v2Message_WhenParse_CorrectHl7v2DataShouldBeReturned()
        {
            var input = @"MSH|^~\&|||||20190502121659.069-0700||ADT^A04^ADT_A01|401|T|2.5.1
NK1|1|JOHNSON^CONWAY^^^^^L|SPOUS||(130) 724-0433^PRN^PH^^^431^2780404~(330) 274-8214^ORN^PH^^^330^2748214||EMERGENCY
||E|||||12345^Johnson^Peter|||||||||||||||||||||||||||||||||||||201905020700";

            var hl7v2Data = _parser.Parse(input) as Hl7v2Data;
            Assert.Equal(3, hl7v2Data.Meta.Count);
            Assert.Equal("MSH", hl7v2Data.Meta[0]);
            Assert.Equal("NK1", hl7v2Data.Meta[1]);
            Assert.Equal(string.Empty, hl7v2Data.Meta[2]);
            Assert.Equal(3, hl7v2Data.Data.Count);

            // Hl7v2Segment tests
            var segment = hl7v2Data.Data[2];
            Assert.Equal("||E|||||12345^Johnson^Peter|||||||||||||||||||||||||||||||||||||201905020700", segment["Value"]);
            Assert.NotNull((SafeList<Hl7v2Field>)segment["Fields"]);
            Assert.Equal("E", ((Hl7v2Field)segment[2]).Value);

            segment = hl7v2Data.Data[1];
            Assert.Equal("NK1|1|JOHNSON^CONWAY^^^^^L|SPOUS||(130) 724-0433^PRN^PH^^^431^2780404~(330) 274-8214^ORN^PH^^^330^2748214||EMERGENCY", segment["Value"]);
            Assert.NotNull((SafeList<Hl7v2Field>)segment["Fields"]);
            Assert.Equal("NK1", ((Hl7v2Field)segment[0]).Value);
            Assert.Throws<RenderException>(() => (Hl7v2Field)segment[null]);
            Assert.Throws<RenderException>(() => (Hl7v2Field)segment[true]);
            Assert.Throws<RenderException>(() => (Hl7v2Field)segment["abc"]);

            // Hl7v2Field tests
            var field = (Hl7v2Field)segment[5];
            Assert.Equal("(130) 724-0433^PRN^PH^^^431^2780404~(330) 274-8214^ORN^PH^^^330^2748214", field["Value"]);
            Assert.Equal(2, ((SafeList<Hl7v2Field>)field["Repeats"]).Count);
            Assert.Equal("(130) 724-0433", ((Hl7v2Component)field[1]).Value);
            Assert.Throws<RenderException>(() => (Hl7v2Component)field[null]);
            Assert.Throws<RenderException>(() => (Hl7v2Component)field[1.2]);
            Assert.Throws<RenderException>(() => (Hl7v2Component)field["abc"]);

            // Hl7v2Component tests
            var component = (Hl7v2Component)field[1];
            Assert.Equal("(130) 724-0433", component["Value"]);
            Assert.NotNull((SafeList<string>)component["Subcomponents"]);
            Assert.Equal("(130) 724-0433", component[1]);
            Assert.Throws<RenderException>(() => (Hl7v2Component)field[null]);
            Assert.Throws<RenderException>(() => (Hl7v2Component)field["abc"]);
        }
    }
}
