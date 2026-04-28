// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.FilterTests
{
    public class IdentifierSelectionFiltersTests
    {
        private static Hash BuildCoding(string system, string code, string display = null)
        {
            var dict = new Dictionary<string, object>
            {
                { "system", system },
                { "code", code },
            };
            if (display != null)
            {
                dict["display"] = display;
            }

            return Hash.FromDictionary(dict);
        }

        private static Hash BuildIdentifier(string system, string value, string use, Hash type)
        {
            var dict = new Dictionary<string, object>
            {
                { "system", system },
                { "value", value },
                { "use", use },
            };
            if (type != null)
            {
                dict["type"] = type;
            }

            return Hash.FromDictionary(dict);
        }

        private static Hash BuildType(object[] codings, string text = null)
        {
            var dict = new Dictionary<string, object>
            {
                { "coding", codings },
            };
            if (text != null)
            {
                dict["text"] = text;
            }

            return Hash.FromDictionary(dict);
        }

        private static Hash BuildCriteria(object[] conditions, string outputSystem)
        {
            return Hash.FromDictionary(new Dictionary<string, object>
            {
                { "conditions", conditions },
                { "outputSystem", outputSystem },
            });
        }

        private static Hash BuildCondition(string path, string value)
        {
            return Hash.FromDictionary(new Dictionary<string, object>
            {
                { "path", path },
                { "value", value },
            });
        }

        private static object[] GetTestIdentifiers()
        {
            var mrCoding = BuildCoding("http://terminology.hl7.org/CodeSystem/v2-0203", "MR", "Medical Record Number");
            var ssCoding = BuildCoding("http://terminology.hl7.org/CodeSystem/v2-0203", "SS", "Social Security Number");
            var dlCoding = BuildCoding("http://terminology.hl7.org/CodeSystem/v2-0203", "DL", "Driver's License");

            var mrType = BuildType(new object[] { mrCoding }, "Medical Record Number");
            var ssType = BuildType(new object[] { ssCoding });
            var dlType = BuildType(new object[] { dlCoding });

            return new object[]
            {
                BuildIdentifier("http://hospital.example.org/mrn", "MRN-12345", "official", mrType),
                BuildIdentifier("http://hl7.org/fhir/sid/us-ssn", "999-99-9999", "official", ssType),
                BuildIdentifier("urn:oid:2.16.840.1.113883.4.3.25", "DL-98765", "usual", dlType),
            };
        }

        [Fact]
        public void GivenSingleCodingCodeCondition_WhenSelectIdentifier_ReturnsCorrectIdentifier()
        {
            var identifiers = GetTestIdentifiers();
            var criteria = BuildCriteria(
                new object[] { BuildCondition("type.coding.code", "MR") },
                "urn:oid:dragon-copilot");

            var result = Filters.SelectIdentifier(identifiers, criteria) as Hash;

            Assert.NotNull(result);
            Assert.Equal("MRN-12345", result["value"]?.ToString());
            Assert.Equal("http://hospital.example.org/mrn", result["system"]?.ToString());
        }

        [Fact]
        public void GivenCompoundCodingConditions_WhenSelectIdentifier_ReturnsCorrectIdentifier()
        {
            var identifiers = GetTestIdentifiers();
            var criteria = BuildCriteria(
                new object[]
                {
                    BuildCondition("type.coding.system", "http://terminology.hl7.org/CodeSystem/v2-0203"),
                    BuildCondition("type.coding.code", "MR"),
                },
                "urn:oid:dragon-copilot");

            var result = Filters.SelectIdentifier(identifiers, criteria) as Hash;

            Assert.NotNull(result);
            Assert.Equal("MRN-12345", result["value"]?.ToString());
        }

        [Fact]
        public void GivenCrossCodingConditions_WhenSelectIdentifier_DoesNotFalselyMatch()
        {
            // Identifier with two coding entries: one has system=X, the other has code=Y.
            // A compound condition requiring system=X AND code=Y should NOT match.
            var codingA = BuildCoding("http://system-X", "CODE_A");
            var codingB = BuildCoding("http://system-B", "CODE_Y");
            var type = BuildType(new object[] { codingA, codingB });
            var identifier = BuildIdentifier("http://example.org", "VAL-1", "official", type);

            var criteria = BuildCriteria(
                new object[]
                {
                    BuildCondition("type.coding.system", "http://system-X"),
                    BuildCondition("type.coding.code", "CODE_Y"),
                },
                "urn:oid:dragon-copilot");

            var exception = Assert.Throws<RenderException>(() =>
                Filters.SelectIdentifier(new object[] { identifier }, criteria));
            Assert.Equal(FhirConverterErrorCode.TemplateRenderingError, exception.FhirConverterErrorCode);
            Assert.Contains("No identifier matched", exception.Message);
        }

        [Fact]
        public void GivenIdentifierSystemCondition_WhenSelectIdentifier_ReturnsCorrectIdentifier()
        {
            var identifiers = GetTestIdentifiers();
            var criteria = BuildCriteria(
                new object[] { BuildCondition("system", "http://hospital.example.org/mrn") },
                "urn:oid:dragon-copilot");

            var result = Filters.SelectIdentifier(identifiers, criteria) as Hash;

            Assert.NotNull(result);
            Assert.Equal("MRN-12345", result["value"]?.ToString());
        }

        [Fact]
        public void GivenTypeTextCondition_WhenSelectIdentifier_ReturnsCorrectIdentifier()
        {
            var identifiers = GetTestIdentifiers();
            var criteria = BuildCriteria(
                new object[] { BuildCondition("type.text", "Medical Record Number") },
                "urn:oid:dragon-copilot");

            var result = Filters.SelectIdentifier(identifiers, criteria) as Hash;

            Assert.NotNull(result);
            Assert.Equal("MRN-12345", result["value"]?.ToString());
        }

        [Fact]
        public void GivenMixedLevelConditions_WhenSelectIdentifier_BothMustPass()
        {
            var identifiers = GetTestIdentifiers();
            var criteria = BuildCriteria(
                new object[]
                {
                    BuildCondition("system", "http://hospital.example.org/mrn"),
                    BuildCondition("type.coding.code", "MR"),
                },
                "urn:oid:dragon-copilot");

            var result = Filters.SelectIdentifier(identifiers, criteria) as Hash;

            Assert.NotNull(result);
            Assert.Equal("MRN-12345", result["value"]?.ToString());
        }

        [Fact]
        public void GivenNoMatchingIdentifier_WhenSelectIdentifier_ThrowsRenderException()
        {
            var identifiers = GetTestIdentifiers();
            var criteria = BuildCriteria(
                new object[] { BuildCondition("type.coding.code", "NONEXISTENT") },
                "urn:oid:dragon-copilot");

            var exception = Assert.Throws<RenderException>(() =>
                Filters.SelectIdentifier(identifiers, criteria));
            Assert.Equal(FhirConverterErrorCode.TemplateRenderingError, exception.FhirConverterErrorCode);
            Assert.Contains("No identifier matched", exception.Message);
        }

        [Fact]
        public void GivenMultipleMatchingIdentifiers_WhenSelectIdentifier_ThrowsRenderException()
        {
            // Create two identifiers with the same coding code "MR"
            var mrCoding = BuildCoding("http://terminology.hl7.org/CodeSystem/v2-0203", "MR");
            var mrType = BuildType(new object[] { mrCoding });
            var identifiers = new object[]
            {
                BuildIdentifier("http://hospital-A.org/mrn", "MRN-111", "official", mrType),
                BuildIdentifier("http://hospital-B.org/mrn", "MRN-222", "official", mrType),
            };

            var criteria = BuildCriteria(
                new object[] { BuildCondition("type.coding.code", "MR") },
                "urn:oid:dragon-copilot");

            var exception = Assert.Throws<RenderException>(() =>
                Filters.SelectIdentifier(identifiers, criteria));
            Assert.Equal(FhirConverterErrorCode.TemplateRenderingError, exception.FhirConverterErrorCode);
            Assert.Contains("Multiple identifiers (2)", exception.Message);
        }

        [Fact]
        public void GivenEmptyIdentifierArray_WhenSelectIdentifier_ThrowsRenderException()
        {
            var criteria = BuildCriteria(
                new object[] { BuildCondition("type.coding.code", "MR") },
                "urn:oid:dragon-copilot");

            var exception = Assert.Throws<RenderException>(() =>
                Filters.SelectIdentifier(new object[] { }, criteria));
            Assert.Contains("No identifier matched", exception.Message);
        }

        [Fact]
        public void GivenIdentifierWithNoType_WhenSelectIdentifier_SkipsGracefully()
        {
            // Identifier without type should not match coding-level conditions and not throw.
            var identifier = BuildIdentifier("http://example.org", "VAL-1", "official", null);
            var criteria = BuildCriteria(
                new object[] { BuildCondition("type.coding.code", "MR") },
                "urn:oid:dragon-copilot");

            var exception = Assert.Throws<RenderException>(() =>
                Filters.SelectIdentifier(new object[] { identifier }, criteria));
            Assert.Contains("No identifier matched", exception.Message);
        }

        [Fact]
        public void GivenIdentifierWithEmptyCodingArray_WhenSelectIdentifier_SkipsGracefully()
        {
            var type = BuildType(new object[] { });
            var identifier = BuildIdentifier("http://example.org", "VAL-1", "official", type);
            var criteria = BuildCriteria(
                new object[] { BuildCondition("type.coding.code", "MR") },
                "urn:oid:dragon-copilot");

            var exception = Assert.Throws<RenderException>(() =>
                Filters.SelectIdentifier(new object[] { identifier }, criteria));
            Assert.Contains("No identifier matched", exception.Message);
        }

        [Fact]
        public void GivenNullSelectionCriteria_WhenSelectIdentifier_ThrowsRenderException()
        {
            var identifiers = GetTestIdentifiers();

            var exception = Assert.Throws<RenderException>(() =>
                Filters.SelectIdentifier(identifiers, null));
            Assert.Contains("selectionCriteria is null", exception.Message);
        }

        [Fact]
        public void GivenNullIdentifiers_WhenSelectIdentifier_ThrowsRenderException()
        {
            var criteria = BuildCriteria(
                new object[] { BuildCondition("type.coding.code", "MR") },
                "urn:oid:dragon-copilot");

            var exception = Assert.Throws<RenderException>(() =>
                Filters.SelectIdentifier(null, criteria));
            Assert.Contains("array of identifiers", exception.Message);
        }

        [Fact]
        public void GivenCodingDisplayCondition_WhenSelectIdentifier_ReturnsCorrectIdentifier()
        {
            var identifiers = GetTestIdentifiers();
            var criteria = BuildCriteria(
                new object[] { BuildCondition("type.coding.display", "Medical Record Number") },
                "urn:oid:dragon-copilot");

            var result = Filters.SelectIdentifier(identifiers, criteria) as Hash;

            Assert.NotNull(result);
            Assert.Equal("MRN-12345", result["value"]?.ToString());
        }
    }
}
