// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Globalization;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Models.Hl7v2;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.FilterTests
{
    public class GeneralFiltersTests
    {
        public static IEnumerable<object[]> GetValidDataForGenerateUuid()
        {
            yield return new object[] { null, null };
            yield return new object[] { string.Empty, null };
            yield return new object[] { "  \n", null };
            yield return new object[] { "MRN12345", "e7ce584a-acf4-7cf0-5b4e-d4961c8123e2" };
        }

        [Fact]
        public void GetPropertyTest()
        {
            // empty context
            var context = new Context(CultureInfo.InvariantCulture);
            Assert.Null(Filters.GetProperty(context, null, null, null));

            // context with null CodeMapping
            context = new Context(new List<Hash>(), new Hash(), new Hash(), ErrorsOutputMode.Rethrow, 0, 0, CultureInfo.InvariantCulture);
            context["CodeMapping"] = null;
            Assert.Equal("M", Filters.GetProperty(context, "M", "Gender", "code"));

            // context with valid CodeMapping
            context["CodeMapping"] = new CodeMapping(new Dictionary<string, Dictionary<string, Dictionary<string, string>>>
            {
                {
                    "CodeSystem/Gender", new Dictionary<string, Dictionary<string, string>>
                    {
                        { "M", new Dictionary<string, string> { { "code", "male" } } },
                    }
                },
            });
            Assert.Equal("male", Filters.GetProperty(context, "M", "CodeSystem/Gender", "code"));
            Assert.Null(Filters.GetProperty(context, "M", null, "code"));
            Assert.Null(Filters.GetProperty(context, "M", string.Empty, "code"));
        }

        [Fact]
        public void EvaluateTest()
        {
            Assert.Null(Filters.Evaluate(null, null));
            Assert.Null(Filters.Evaluate(string.Empty, string.Empty));

            var input = @"{""code"": ""male"", ""display"": ""Male"", ""system"": ""http://hl7.org/fhir/administrative-gender"",}";
            Assert.Equal("male", Filters.Evaluate(input, "code"));
            Assert.Equal("Male", Filters.Evaluate(input, "display"));
            Assert.Null(Filters.Evaluate(input, "abc"));
            Assert.Null(Filters.Evaluate(input, null));
            Assert.Null(Filters.Evaluate(input, string.Empty));
        }

        [Theory]
        [MemberData(nameof(GetValidDataForGenerateUuid))]
        public void GivenValidData_WhenGenerateUuid_CorrectResultShouldBeReturned(string input, string expected)
        {
            Assert.Equal(expected, Filters.GenerateUUID(input));
        }

        [Fact]
        public void GenerateIdInputTest()
        {
            var context = new Context(CultureInfo.InvariantCulture);
            context["EncodingCharacters"] = new Hl7v2EncodingCharacters();

            // Base resources
            Assert.Equal(
                "Patient_PATID1234,ADT1",
                Filters.GenerateIdInput("PATID1234,ADT1", "Patient", false));

            // Base optional resources
            Assert.Equal(
                "Encounter_0123456789_5002eb07-c460-7112-6574-50303ae3b4a6",
                Filters.GenerateIdInput("0123456789", "Encounter", false, "5002eb07-c460-7112-6574-50303ae3b4a6"));

            // Base required resources
            Assert.Equal(
                "RelatedPerson_NK1|1|DUCK^HUEY|SO|3583 DUCK RD^^FOWL^CA^999990000|8885552222||Y||||||||||||||_bab5ca58-f272-4c06-4b3f-f9661e45a22b",
                Filters.GenerateIdInput("NK1|1|DUCK^HUEY|SO|3583 DUCK RD^^FOWL^CA^999990000|8885552222||Y|||||||||||||| ", "RelatedPerson", true, "bab5ca58-f272-4c06-4b3f-f9661e45a22b"));

            // Bundle
            var message = @"MSH|^~\&|AccMgr|1|||20050110045504||ADT^A01|599102|P|2.3||| 
EVN|A01|20050110045502||||| ";
            Assert.Equal(
                @"Bundle_MSH|^~\&|AccMgr|1|||20050110045504||ADT^A01|599102|P|2.3||| 
EVN|A01|20050110045502|||||",
                Filters.GenerateIdInput(message, "Bundle", false));

            // Null, empty or whitespace segment
            Assert.Null(Filters.GenerateIdInput(null, "Location", false));
            Assert.Null(Filters.GenerateIdInput(string.Empty, "Location", false));
            Assert.Null(Filters.GenerateIdInput(" \n", "Location", false));

            // Base ID required but not provided
            var exception = Assert.Throws<RenderException>(() => Filters.GenerateIdInput("NK1|1|DUCK^HUEY|SO|3583 DUCK RD^^FOWL^CA^999990000|8885552222||Y|||||||||||||| ", "RelatedPerson", true, null));
            Assert.Equal(FhirConverterErrorCode.InvalidIdGenerationInput, exception.FhirConverterErrorCode);
        }
    }
}
