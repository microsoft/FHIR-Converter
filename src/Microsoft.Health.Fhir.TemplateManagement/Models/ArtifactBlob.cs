// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.IO;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Health.Fhir.TemplateManagement.Utilities;

namespace Microsoft.Health.Fhir.TemplateManagement.Models
{
    public class ArtifactBlob
    {
        public string FileName { get; set; }

        // Size of the layer content.
        public long Size { get; set; }

        // Sha256 digest of the layer.
        public string Digest { get; set; }

        // Content of the layer (tar.gz).
        public virtual byte[] Content { get; set; }

        public async Task WriteToFileAsync(string path)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            if (Content == null)
            {
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            await File.WriteAllBytesAsync(path, Content);
        }

        public async Task ReadFromFileAsync(string path)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            if (!File.Exists(path))
            {
                return;
            }

            Content = await File.ReadAllBytesAsync(path);
            Digest = StreamUtility.CalculateDigestFromSha256(Content);
            Size = Content.Length;
            FileName = Path.GetFileName(path);
        }
    }
}
