// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Models;

namespace Microsoft.Health.Fhir.TemplateManagement.Utilities
{
    public class StreamUtility
    {
        public static Dictionary<string, byte[]> DecompressTarGzStream(Stream sourceStream)
        {
            try
            {
                using var gzipStream = new GZipInputStream(sourceStream);
                using var tarIn = new TarInputStream(gzipStream, Encoding.UTF8);
                return ExtractContents(tarIn);
            }
            catch (Exception ex)
            {
                throw new ArtifactDecompressException(TemplateManagementErrorCode.DecompressImageFailed, "Decompress image failed.", ex);
            }
        }

        private static Dictionary<string, byte[]> ExtractContents(TarInputStream tarIn)
        {
            Dictionary<string, byte[]> artifacts = new Dictionary<string, byte[]> { };
            TarEntry entry = tarIn.GetNextEntry();
            while (entry != null)
            {
                if (entry.TarHeader.TypeFlag != TarHeader.LF_LINK || entry.TarHeader.TypeFlag != TarHeader.LF_SYMLINK)
                {
                    byte[] content = ExtractEntry(tarIn);
                    if (entry.Name.Contains(Constants.WhiteoutsLabel))
                    {
                        artifacts.Add(Path.GetRelativePath("./", entry.Name).Replace("\\", "/").Replace(Constants.WhiteoutsLabel, string.Empty), null);
                    }
                    else
                    {
                        artifacts.Add(Path.GetRelativePath("./", entry.Name).Replace("\\", "/"), content);
                    }
                }

                entry = tarIn.GetNextEntry();
            }

            return artifacts;
        }

        private static byte[] ExtractEntry(TarInputStream tarIn)
        {
            using var outputStream = new MemoryStream();
            tarIn.CopyEntryContents(outputStream);
            outputStream.Position = 0;
            return outputStream.ToArray();
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

    }
}
