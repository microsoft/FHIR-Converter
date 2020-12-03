// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Utilities;

namespace Microsoft.Health.Fhir.TemplateManagement.Models
{
    public class TemplateLayer : ArtifactLayer
    {
        private const string EmbeddedTemplatesResource = "DefaultTemplates.tar.gz";

        public static TemplateLayer ReadFromFile(string filePath)
        {
            try
            {
                TemplateLayer templateLayer = new TemplateLayer();
                var rawBytes = File.ReadAllBytes(filePath);
                var artifacts = TemplateLayerParser.DecompressRawBytesContent(rawBytes);
                templateLayer.Content = TemplateLayerParser.ParseToTemplates(artifacts);
                templateLayer.Digest = StreamUtility.CalculateDigestFromSha256(File.ReadAllBytes(filePath));
                templateLayer.Size = artifacts.Sum(x => x.Value.Length);
                return templateLayer;
            }
            catch (Exception ex)
            {
                throw new DefaultTemplatesInitializeException(TemplateManagementErrorCode.InitializeDefaultTemplateFailed, $"Load default template from {filePath} failed", ex);
            }
        }

        public static TemplateLayer ReadFromEmbeddedResource()
        {
            try
            {
                var templateAssembly = typeof(TemplateLayer).Assembly;
                using Stream resourceStream = templateAssembly.GetManifestResourceStream(EmbeddedTemplatesResource);
                using var stream = new MemoryStream();
                resourceStream.CopyTo(stream);
                var rawBytes = stream.ToArray();

                TemplateLayer templateLayer = new TemplateLayer();
                var artifacts = TemplateLayerParser.DecompressRawBytesContent(rawBytes);
                templateLayer.Content = TemplateLayerParser.ParseToTemplates(artifacts);
                templateLayer.Digest = StreamUtility.CalculateDigestFromSha256(rawBytes);
                templateLayer.Size = artifacts.Sum(x => x.Value.Length);
                return templateLayer;
            }
            catch (Exception ex)
            {
                throw new DefaultTemplatesInitializeException(TemplateManagementErrorCode.InitializeDefaultTemplateFailed, $"Load default template from {filePath} failed", ex);
            }
        }
    }
}
