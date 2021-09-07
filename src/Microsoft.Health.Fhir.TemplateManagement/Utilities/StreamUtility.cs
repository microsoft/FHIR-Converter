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
        private static DateTime GzipTimestamp => new DateTime(2021, 1, 1);

        public static Dictionary<string, byte[]> DecompressFromTarGzStream(Stream tarGzStream)
        {
            try
            {
                Dictionary<string, byte[]> artifacts = new Dictionary<string, byte[]> { };
                var reader = ReaderFactory.Open(tarGzStream);
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
                            artifacts.Add(Path.GetRelativePath("./", fileName).Replace("\\", "/"), byteContent);
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

        public static MemoryStream CompressToTarGzStream(Dictionary<string, byte[]> fileContents, bool fixedTimestamp)
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

                    if (fixedTimestamp)
                    {
                        FixTimestampInGzHeader(resultStream);
                    }
                }

                return resultStream;
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

        private static void FixTimestampInGzHeader(MemoryStream inputStream)
        {
            int num = (int)((GzipTimestamp.Ticks - new DateTime(1970, 1, 1).Ticks) / 10000000);
            byte[] obj = new byte[4]
            {
                    0,
                    0,
                    0,
                    0,
            };
            obj[0] = (byte)num;
            obj[1] = (byte)(num >> 8);
            obj[2] = (byte)(num >> 16);
            obj[3] = (byte)(num >> 24);
            byte[] array = obj;
            inputStream.Seek(4, SeekOrigin.Begin);
            inputStream.Write(array, 0, array.Length);
        }
    }
}
