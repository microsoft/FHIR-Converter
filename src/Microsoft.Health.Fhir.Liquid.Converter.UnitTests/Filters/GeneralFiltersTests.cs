// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Globalization;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Hl7v2.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.FilterTests
{
    public class GeneralFiltersTests
    {
        public static IEnumerable<object[]> GetValidDataForGenerateUuid()
        {
            yield return new object[] { "MRN12345", "e7ce584a-acf4-7cf0-5b4e-d4961c8123e2" };
            yield return new object[] { new Hl7v2Segment("PV1||E|||||12345^Johnson^Peter|||||||||||||||||||||||||||||||||||||201905020700", new List<Hl7v2Field>()), "5444ae8e-0aa2-a3bc-8da0-2af8cef0335d" };
        }

        public static IEnumerable<object[]> GetInvalidDataForGenerateUuid()
        {
            yield return new object[] { 123 };
            yield return new object[] { true };
            yield return new object[] { null };
            yield return new object[] { string.Empty };
            yield return new object[] { new Hl7v2Component(null, new List<string>()) };
        }

        [Fact]
        public void GetPropertyTest()
        {
            // empty context
            var context = new Context(CultureInfo.InvariantCulture);
            Assert.Null(Filters.GetProperty(context, null, null, null));

            // context with null CodeSystemMapping
            context = new Context(new List<Hash>(), new Hash(), new Hash(), ErrorsOutputMode.Rethrow, 0, 0, CultureInfo.InvariantCulture);
            context["CodeSystemMapping"] = null;
            Assert.Null(Filters.GetProperty(context, "M", "Gender", "code"));

            // context with valid CodeSystemMapping
            context["CodeSystemMapping"] = new CodeSystemMapping(new Dictionary<string, Dictionary<string, Dictionary<string, string>>>
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
        public void GivenValidData_WhenGenerateUuid_CorrectResultShouldBeReturned(object input, string expected)
        {
            Assert.Equal(expected, Filters.GenerateUUID(input));
        }

        [Theory]
        [MemberData(nameof(GetInvalidDataForGenerateUuid))]
        public void GivenInvalidData_WhenGenerateUuid_CorrectResultShouldBeReturned(object input)
        {
            var exception = Assert.Throws<DataFormatException>(() => Filters.GenerateUUID(input));
            Assert.Equal(FhirConverterErrorCode.InvalidIdGenerationInput, exception.FhirConverterErrorCode);
        }
    }
}
