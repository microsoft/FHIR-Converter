// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Fhir.TemplateManagement.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        public int SequenceNumber { get; set; }

        // Content of the layer.
        public virtual byte[] Content { get; set; }

        public static void WriteOCIArtifactLayer(OCIArtifactLayer layer, string directory)
        {
            if (layer == null || layer.Content == null)
            {
                return;
            }

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(Path.Combine(directory, Path.GetFileName(layer.FileName)), layer.Content);
        }

        public static OCIArtifactLayer ReadOCIArtifactLayer(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            var artifactLayer = new OCIArtifactLayer() { Content = File.ReadAllBytes(path) };
            artifactLayer.Digest = StreamUtility.CalculateDigestFromSha256(artifactLayer.Content);
            artifactLayer.Size = artifactLayer.Content.Length;
            artifactLayer.FileName = Path.GetFileName(path);
            return artifactLayer;
        }
    }
}
