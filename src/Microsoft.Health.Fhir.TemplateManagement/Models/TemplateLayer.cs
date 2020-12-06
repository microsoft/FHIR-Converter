// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Utilities;

namespace Microsoft.Health.Fhir.TemplateManagement.Models
{
    public class TemplateLayer : ArtifactLayer
    {
        public static TemplateLayer ReadFromEmbeddedResource()
        {
            try
            {
                var defaultTemplateResourceName = $"{typeof(Constants).Namespace}.{Constants.DefaultTemplatePath}";
                using Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(defaultTemplateResourceName);
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
                throw new DefaultTemplatesInitializeException(TemplateManagementErrorCode.InitializeDefaultTemplateFailed, $"Load default template failed.", ex);
            }
        }
    }
}
