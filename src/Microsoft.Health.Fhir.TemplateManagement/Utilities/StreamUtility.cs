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

        public void TarCreateFromStream()
        {
            // Create an output stream. Does not have to be disk, could be MemoryStream etc.
            string tarOutFn = @"c:\temp\test.tar";
            Stream outStream = File.Create(tarOutFn);

            // If you wish to create a .Tar.GZ (.tgz):
            // - set the filename above to a ".tar.gz",
            // - create a GZipOutputStream here
            // - change "new TarOutputStream(outStream)" to "new TarOutputStream(gzoStream)"
            // Stream gzoStream = new GZipOutputStream(outStream);
            // gzoStream.SetLevel(3); // 1 - 9, 1 is best speed, 9 is best compression

            GZipOutputStream gzipOutputStream = new GZipOutputStream(outStream);
            TarOutputStream tarOutputStream = new TarOutputStream(gzipOutputStream);

            gzipOutputStream.SetLevel(3);

            CreateTarManually(tarOutputStream, @"c:\temp\debug");

            // Closing the archive also closes the underlying stream.
            // If you don't want this (e.g. writing to memorystream), set tarOutputStream.IsStreamOwner = false
            tarOutputStream.Close();
        }

        private void CreateTarManually(TarOutputStream tarOutputStream, string sourceDirectory)
        {
            // Optionally, write an entry for the directory itself.
            TarEntry tarEntry = TarEntry.CreateEntryFromFile(sourceDirectory);
            tarOutputStream.PutNextEntry(tarEntry);

            // Write each file to the tar.
            string[] filenames = Directory.GetFiles(sourceDirectory);

            foreach (string filename in filenames)
            {
                // You might replace these 3 lines with your own stream code

                using (Stream inputStream = File.OpenRead(filename))
                {
                    string tarName = filename.Substring(3); // strip off "C:\"

                    long fileSize = inputStream.Length;

                    // Create a tar entry named as appropriate. You can set the name to anything,
                    // but avoid names starting with drive or UNC.
                    TarEntry entry = TarEntry.CreateTarEntry(tarName);

                    // Must set size, otherwise TarOutputStream will fail when output exceeds.
                    entry.Size = fileSize;

                    // Add the entry to the tar stream, before writing the data.
                    tarOutputStream.PutNextEntry(entry);

                    // this is copied from TarArchive.WriteEntryCore
                    byte[] localBuffer = new byte[32 * 1024];
                    while (true)
                    {
                        int numRead = inputStream.Read(localBuffer, 0, localBuffer.Length);
                        if (numRead <= 0)
                            break;

                        tarOutputStream.Write(localBuffer, 0, numRead);
                    }
                }
                tarOutputStream.CloseEntry();
            }

            // Recurse. Delete this if unwanted.

            string[] directories = Directory.GetDirectories(sourceDirectory);
            foreach (string directory in directories)
                CreateTarManually(tarOutputStream, directory);
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
