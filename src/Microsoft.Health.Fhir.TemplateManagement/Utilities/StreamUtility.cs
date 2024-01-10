// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using EnsureThat;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers;
using SharpCompress.Writers.Tar;

namespace Microsoft.Health.Fhir.TemplateManagement.Utilities
{
    public class StreamUtility
    {
        public static Dictionary<string, byte[]> DecompressFromTarGz(Stream tarGzStream, string artifactFolder = "")
        {
            try
            {
                Dictionary<string, byte[]> artifacts = new Dictionary<string, byte[]> { };
                using var reader = ReaderFactory.Open(tarGzStream);
                while (reader.MoveToNextEntry())
                {
                    if (!reader.Entry.IsDirectory)
                    {
                        var stream = new MemoryStream();
                        reader.WriteEntryTo(stream);

                        var byteContent = stream.ToArray();
                        var fileName = reader.Entry.Key;
                        if (fileName.Contains(Constants.WhiteoutsLabel))
                        {
                            artifacts.Add(Path.GetRelativePath("./", fileName).Replace("\\", "/").Replace(Constants.WhiteoutsLabel, string.Empty), null);
                        }
                        else
                        {
                            var artifactName = Path.GetRelativePath("./", fileName).Replace("\\", "/");
                            artifactName = string.IsNullOrWhiteSpace(artifactFolder) ? artifactName : string.Format("{0}/{1}", artifactFolder, artifactName);

                            artifacts.Add(artifactName, byteContent);
                        }
                    }
                }

                return artifacts;
            }
            catch (Exception ex)
            {
                throw new ArtifactArchiveException(TemplateManagementErrorCode.DecompressArtifactFailed, "Decompress image failed.", ex);
            }
        }

        public static byte[] CompressToTarGz(Dictionary<string, byte[]> fileContents, bool resetTimestamp)
        {
            EnsureArg.IsNotNull(fileContents, nameof(fileContents));

            try
            {
                var resultStream = new MemoryStream();
                using (Stream stream = resultStream)
                {
                    using (var tarWriter = new TarWriter(stream, new TarWriterOptions(CompressionType.GZip, true)))
                    {
                        foreach (var eachFile in fileContents)
                        {
                            tarWriter.Write(eachFile.Key, new MemoryStream(eachFile.Value));
                        }
                    }

                    if (resetTimestamp)
                    {
                        ResetTimestampInGzHeader(resultStream);
                    }
                }

                return resultStream.ToArray();
            }
            catch (Exception ex)
            {
                throw new ArtifactArchiveException(TemplateManagementErrorCode.CompressArtifactFailed, "Compress content failed.", ex);
            }
        }

        public static string CalculateDigestFromSha256(byte[] content)
        {
            using SHA256 mySHA256 = SHA256.Create();
            string hashedValue = "sha256:";
            byte[] hashData = mySHA256.ComputeHash(content);
            string[] hashedStrings = hashData.Select(x => string.Format("{0,2:x2}", x)).ToArray();
            hashedValue += string.Join(string.Empty, hashedStrings);
            return hashedValue;
        }

        public static string CalculateDigestFromSha256(Stream stream)
        {
            using SHA256 mySHA256 = SHA256.Create();
            string hashedValue = "sha256:";
            byte[] hashData = mySHA256.ComputeHash(stream);
            string[] hashedStrings = hashData.Select(x => string.Format("{0,2:x2}", x)).ToArray();
            hashedValue += string.Join(string.Empty, hashedStrings);
            return hashedValue;
        }

        private static void ResetTimestampInGzHeader(MemoryStream inputStream)
        {
            byte[] array = { 0, 0, 0, 0 };
            var position = inputStream.Position;

            // Write header.
            inputStream.Seek(4, SeekOrigin.Begin);
            inputStream.Write(array, 0, array.Length);

            // Restore stream position
            inputStream.Seek(position, SeekOrigin.Begin);
        }
    }
}
