// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.IO;
using EnsureThat;
using Microsoft.Health.Fhir.TemplateManagement.Utilities;

namespace Microsoft.Health.Fhir.TemplateManagement.Models
{
    public class OCIArtifactLayer
    {
        public string FileName { get; set; }

        // Size of the layer content.
        public long Size { get; set; }

        // Sha256 digest of the layer.
        public string Digest { get; set; }

        // Sequence number of the layer.
        public int SequenceNumber { get; set; } = -1;

        // Content of the layer.
        public virtual byte[] Content { get; set; }

        public void WriteToFolder(string directory)
        {
            EnsureArg.IsNotNullOrEmpty(directory, nameof(directory));

            if (Content == null)
            {
                return;
            }

            Directory.CreateDirectory(directory);
            File.WriteAllBytes(Path.Combine(directory, Path.GetFileName(FileName)), Content);
        }

        public void ReadFromFolder(string path)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            if (!File.Exists(path))
            {
                return;
            }

            Content = File.ReadAllBytes(path);
            Digest = StreamUtility.CalculateDigestFromSha256(Content);
            Size = Content.Length;
            FileName = Path.GetFileName(path);
        }
    }
}
