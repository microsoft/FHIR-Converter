// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Microsoft.Health.Fhir.TemplateManagement.Utilities;
using Xunit;

namespace Microsoft.Health.Fhir.TemplateManagement.UnitTests
{
    public class TemplateLayerParserTests
    {
        public static IEnumerable<object[]> GetFilePathForOciArifactWithFileCounts()
        {
            yield return new object[] { "TestData/TarGzFiles/userV1.tar.gz", 813 };
            yield return new object[] { "TestData/TarGzFiles/userV2.tar.gz", 767 };
            yield return new object[] { "TestData/TarGzFiles/baseLayer.tar.gz", 818 };
        }

        public static IEnumerable<object[]> GetFilePathForOciArifactWithTemplateCounts()
        {
            yield return new object[] { "TestData/TarGzFiles/userV1.tar.gz", 813 };
            yield return new object[] { "TestData/TarGzFiles/userV2.tar.gz", 767 };
            yield return new object[] { "TestData/TarGzFiles/baseLayer.tar.gz", 817 };
        }

        public static IEnumerable<object[]> GetDecompressFailedFilePath()
        {
            yield return new object[] { "TestData/TarGzFiles/invalid1.tar.gz" };
            yield return new object[] { "TestData/TarGzFiles/invalid2.tar.gz" };
        }

        [Theory]
        [MemberData(nameof(GetFilePathForOciArifactWithTemplateCounts))]
        public void GivenArtifactLayer_WhenParseArtifactLayerToTemplateLayer_ACorrectTemplateLayerShouldBeReturned(string filePath, int expectedTemplatesCounts)
        {
            var content = File.ReadAllBytes(filePath);
            var testArtifactLayer = new ArtifactBlob() { Content = content, Digest = StreamUtility.CalculateDigestFromSha256(File.ReadAllBytes(filePath)) };
            var templateLayer = TemplateLayerParser.ParseArtifactsLayerToTemplateLayer(testArtifactLayer);
            Assert.Equal(expectedTemplatesCounts, templateLayer.TemplateContent.Count());
            Assert.Equal(testArtifactLayer.Digest, templateLayer.Digest);
        }

        [Theory]
        [MemberData(nameof(GetFilePathForOciArifactWithFileCounts))]
        public void GivenRawBytes_WhenDecompressRawBytesToStringContent_CorrectArtifactsShouldBeReturned(string filePath, int expectedArtifactCounts)
        {
            var content = File.ReadAllBytes(filePath);
            var artifacts = StreamUtility.DecompressFromTarGz(new MemoryStream(content));
            Assert.Equal(expectedArtifactCounts + 1, artifacts.Count());
        }

        [Fact]
        public void GivenStringContent_WhenParseToTemplates_CorrectTemplatesShouldBeReturn()
        {
            var content = new Dictionary<string, byte[]>
            {
                { "ADT_A01.liquid", Encoding.UTF8.GetBytes("a") },
                { "Resource/_Patient.liquid", Encoding.UTF8.GetBytes("b") },
                { @"Resource\_Encounter.liquid", Encoding.UTF8.GetBytes("c") },
            };
            var parsedTemplates = TemplateLayerParser.ParseToTemplates(content);
            Assert.Equal("a", parsedTemplates["ADT_A01"].Render());
            Assert.Equal("b", parsedTemplates["Resource/Patient"].Render());
            Assert.Equal("c", parsedTemplates["Resource/Encounter"].Render());
        }

        [Theory]
        [MemberData(nameof(GetDecompressFailedFilePath))]
        public void GivenRawBytes_WhenParseRawBytesToTemplates_IfDecompressedFailed_ExceptionShouldBeThrown(string filePath)
        {
            var content = File.ReadAllBytes(filePath);
            Assert.Throws<ArtifactArchiveException>(() => StreamUtility.DecompressFromTarGz(new MemoryStream(content)));
        }

        [Fact]
        public void GivenRawBytes_WhenParseRawBytesToTemplates_IfTemplateParseFailed_ExceptionShouldBeThrown()
        {
            var content = new Dictionary<string, byte[]>
            {
                { "ADT_A01.liquid", Encoding.UTF8.GetBytes("{{") },
                { "Resource/_Patient.liquid", Encoding.UTF8.GetBytes("..") },
                { @"Resource\_Encounter.liquid", Encoding.UTF8.GetBytes("c_") },
            };
            Assert.Throws<TemplateParseException>(() => TemplateLayerParser.ParseToTemplates(content));
        }
    }
}
