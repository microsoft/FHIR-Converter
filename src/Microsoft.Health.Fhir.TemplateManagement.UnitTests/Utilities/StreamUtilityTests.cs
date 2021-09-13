// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Utilities;
using Xunit;

namespace Microsoft.Health.Fhir.TemplateManagement.UnitTests.Utilities
{
    public class StreamUtilityTests
    {
        private readonly string _tarGzFilePath = "TestData/TarGzFiles/testdecompress.tar.gz";
        private readonly string _decompressedFileFolder = "TestData/DecompressedFiles";

        public static IEnumerable<object[]> GetFilePathWithDigest()
        {
            yield return new object[] { "TestData/DecompressedFiles/ADT_A01.liquid", "sha256:d377125165eb6d770f344429a7a55379d4028774aebe267fe620cd1fcd2daab7" };
            yield return new object[] { "TestData/DecompressedFiles/ORU_R01.liquid", "sha256:be0183c10057fc0a2a2f3bb28f65d281042dd4fa9b9bc4ff9c7df9db0cf2f08d" };
            yield return new object[] { "TestData/DecompressedFiles/.wh.VXU_V04.liquid", "sha256:9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08" };
        }

        public static IEnumerable<object[]> GetTarGzFilePathWithCountsOfFiles()
        {
            yield return new object[] { "TestData/TarGzFiles/userV1.tar.gz", 814 };
            yield return new object[] { "TestData/TarGzFiles/userV2.tar.gz", 768 };
        }

        public static IEnumerable<object[]> GetInvalidTarGzFilePath()
        {
            yield return new object[] { "TestData/TarGzFiles/invalid1.tar.gz" };
            yield return new object[] { "TestData/TarGzFiles/invalid2.tar.gz" };
        }

        [Fact]
        public void GivenACompressedTarGzFile_WhenDecompress_UnCompressedFilesShouldBeReturned()
        {
            var rawBytes = File.ReadAllBytes(_tarGzFilePath);
            var compressedStream = new MemoryStream(rawBytes);
            var artifacts = StreamUtility.DecompressFromTarGz(compressedStream);

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

        [Fact]
        public void GivenFileContent_WhenCompressWithFixedTimestamp_CompressedFilesWithFixedDigestShouldBeReturned()
        {
            var artifacts = new Dictionary<string, byte[]>();
            var files = Directory.EnumerateFiles(_decompressedFileFolder, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                artifacts.Add(file, File.ReadAllBytes(file));
            }

            var result = StreamUtility.CompressToTarGz(artifacts, true);

            Assert.Equal("sha256:a795f0737dddd33784564fe1c190c33a947b6f3614ccb9a6ff31afb19e3f78b3", StreamUtility.CalculateDigestFromSha256(result.ToArray()));
        }

        [Theory]
        [MemberData(nameof(GetTarGzFilePathWithCountsOfFiles))]
        public void GivenACompressedTarGzFile_WhenDecompress_TheCorrectCountsOfFilesShouldBeReturned(string tarGzPath, int counts)
        {
            var rawBytes = File.ReadAllBytes(tarGzPath);
            var compressedStream = new MemoryStream(rawBytes);
            var artifacts = StreamUtility.DecompressFromTarGz(compressedStream);
            Assert.Equal(counts, artifacts.Count());
        }

        [Theory]
        [MemberData(nameof(GetFilePathWithDigest))]
        public void GiveFileContent_WhenCalculateDigest_ACorrectDigestShouldBeReturned(string filePath, string expectedDigest)
        {
            var digest = StreamUtility.CalculateDigestFromSha256(Encoding.UTF8.GetBytes(File.ReadAllText(filePath).Replace("\r", string.Empty).Replace("\n", string.Empty)));
            Assert.Equal(expectedDigest, digest);
        }

        private void CompareTwoDictionary(Dictionary<string, byte[]> result, Dictionary<string, byte[]> expected)
        {
            foreach (var element in expected)
            {
                if (element.Value == null && result[element.Key] == null)
                {
                    Assert.True(true);
                }
                else
                {
                    Assert.Equal(
                        Encoding.UTF8.GetString(result[element.Key]).Replace("\r", string.Empty).Replace("\n", string.Empty),
                        Encoding.UTF8.GetString(element.Value).Replace("\r", string.Empty).Replace("\n", string.Empty));
                }
            }
        }

        [Theory]
        [MemberData(nameof(GetInvalidTarGzFilePath))]
        public void GiveAnInvalidTarGzFilePath_WhenDecompressArtifactsLayer_ExceptionShouldBeThrown(string tarGzPath)
        {
            var artifactsLayer = File.ReadAllBytes(tarGzPath);
            Assert.Throws<ArtifactArchiveException>(() => StreamUtility.DecompressFromTarGz(new MemoryStream(artifactsLayer)));
        }
    }
}
