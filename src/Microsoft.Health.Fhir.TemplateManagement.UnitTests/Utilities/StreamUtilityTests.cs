// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.GZip;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Utilities;
using Xunit;

namespace Microsoft.Health.Fhir.TemplateManagement.UnitTests.Utilities
{
    public class StreamUtilityTests
    {
        private readonly string _tarGzFilePath = Path.Join(new[] { "TestData", "TarGzFiles", "testdecompress.tar.gz" });
        private readonly string _decompressedFileFolder = Path.Join(new[] { "TestData", "DecompressedFiles" });

        public static IEnumerable<object[]> GetFilePathWithDigest()
        {
            yield return new object[] { Path.Join(new[] { "TestData", "DecompressedFiles", "ADT_A01.liquid" }), "sha256:74a970505314dee5b9827af6c12145a4992573f490b9d2ee3a3126f0b352425f" };
            yield return new object[] { Path.Join(new[] { "TestData", "DecompressedFiles", "ORU_R01.liquid" }), "sha256:4100086beb8df1e414a301c33066c1689e4156a2d9b718e3bc8146096d197032" };
            yield return new object[] { Path.Join(new[] { "TestData", "DecompressedFiles", ".wh.VXU_V04.liquid" }), "sha256:837ccb607e312b170fac7383d7ccfd61fa5072793f19a25e75fbacb56539b86b" };
        }

        public static IEnumerable<object[]> GetTarGzFilePathWithCountsOfFiles()
        {
            yield return new object[] { Path.Join(new[] { "TestData", "TarGzFiles", "userV1.tar.gz" }), 814 };
            yield return new object[] { Path.Join(new[] { "TestData", "TarGzFiles", "userV2.tar.gz" }), 768 };
        }

        public static IEnumerable<object[]> GetInvalidTarGzFilePath()
        {
            yield return new object[] { Path.Join(new[] { "TestData", "TarGzFiles", "invalid1.tar.gz" }) };
            yield return new object[] { Path.Join(new[] { "TestData", "TarGzFiles", "invalid2.tar.gz" }) };
        }

        [Fact]
        public void GivenACompressedTarGzFile_WhenDecompress_UnCompressedFilesShouldBeReturned()
        {
            var rawBytes = File.ReadAllBytes(_tarGzFilePath);
            var compressedStream = new MemoryStream(rawBytes);
            var artifacts = StreamUtility.DecompressTarGzStream(compressedStream);

            Dictionary<string, byte[]> expectedFile = new Dictionary<string, byte[]> { };
            var expectedFiles = Directory.EnumerateFiles(_decompressedFileFolder, "*.*", SearchOption.AllDirectories);
            foreach (var oneExpectedFile in expectedFiles)
            {
                if (oneExpectedFile.Contains(".wh."))
                {
                    expectedFile.Add(Path.GetRelativePath(_decompressedFileFolder, oneExpectedFile.Replace(".wh.", string.Empty)), null);
                }
                else
                {
                    expectedFile.Add(Path.GetRelativePath(_decompressedFileFolder, oneExpectedFile), File.ReadAllBytes(oneExpectedFile));
                }
            }

            CompareTwoDictionary(artifacts, expectedFile);
        }

        [Theory]
        [MemberData(nameof(GetTarGzFilePathWithCountsOfFiles))]
        public void GivenACompressedTarGzFile_WhenDecompress_TheCorrectCountsOfFilesShouldBeReturned(string tarGzPath, int counts)
        {
            var rawBytes = File.ReadAllBytes(tarGzPath);
            var compressedStream = new MemoryStream(rawBytes);
            var artifacts = StreamUtility.DecompressTarGzStream(compressedStream);
            Assert.Equal(counts, artifacts.Count());
        }

        [Theory]
        [MemberData(nameof(GetFilePathWithDigest))]
        public void GiveAFile_WhenCalculateDigest_ACorrectDigestShouldBeReturned(string filePath, string expectedDigest)
        {
            var digest = StreamUtility.CalculateDigestFromSha256(File.ReadAllBytes(filePath));
            Assert.Equal(expectedDigest, digest);
        }

        private void CompareTwoDictionary(Dictionary<string, byte[]> result, Dictionary<string, byte[]> expected)
        {
            foreach (var element in expected)
            {
                Assert.Equal(result[element.Key], element.Value);
            }
        }

        [Theory]
        [MemberData(nameof(GetInvalidTarGzFilePath))]
        public void GiveAnInvalidTarGzFilePath_WhenDecompressArtifactsLayer_ExceptionShouldBeThrown(string tarGzPath)
        {
            var artifactsLayer = File.ReadAllBytes(tarGzPath);
            Assert.Throws<ArtifactDecompressException>(() => StreamUtility.DecompressTarGzStream(new MemoryStream(artifactsLayer)));
        }
    }
}
