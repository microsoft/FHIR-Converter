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
            yield return new object[] { Path.Join(new[] { "TestData", "DecompressedFiles", "ADT_A01.liquid" }), "sha256:056748739aef34bb6e4140cdbf87d9f728597bc40b19e1021085985dd9684b54" };
            yield return new object[] { Path.Join(new[] { "TestData", "DecompressedFiles", "ORU_R01.liquid" }), "sha256:46b9ca04c69e6a6bd7a1c73c3d070095a2c2551437de405558c1206bc720f473" };
            yield return new object[] { Path.Join(new[] { "TestData", "DecompressedFiles", ".wh.VXU_V04.liquid" }), "sha256:f2ca1bb6c7e907d06dafe4687e579fce76b37e4e93b7605022da52e6ccc26fd2" };
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
