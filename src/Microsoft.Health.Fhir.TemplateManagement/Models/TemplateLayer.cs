// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DotLiquid;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Utilities;

namespace Microsoft.Health.Fhir.TemplateManagement.Models
{
    public class TemplateLayer : ArtifactBlob
    {
        public Dictionary<string, Template> TemplateContent { get; set; }

        public static TemplateLayer ReadFromEmbeddedResource(string templatePath)
        {
            try
            {
                var defaultTemplateResourceName = $"{typeof(Constants).Namespace}.{templatePath}";
                using Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(defaultTemplateResourceName);
                return ReadFromStream(resourceStream);
            }
            catch (Exception ex)
            {
                throw new DefaultTemplatesInitializeException(TemplateManagementErrorCode.InitializeDefaultTemplateFailed, $"Load default template failed.", ex);
            }
        }

        public static TemplateLayer ReadFromFile(string filePath)
        {
            try
            {
                using Stream fileStream = File.OpenRead(filePath);
                return ReadFromStream(fileStream);
            }
            catch (Exception ex)
            {
                throw new DefaultTemplatesInitializeException(TemplateManagementErrorCode.InitializeDefaultTemplateFailed, $"Load default template failed.", ex);
            }
        }

        private static TemplateLayer ReadFromStream(Stream stream)
        {
            try
            {
                var digest = StreamUtility.CalculateDigestFromSha256(stream);
                stream.Position = 0;

                TemplateLayer templateLayer = new TemplateLayer();
                var artifacts = StreamUtility.DecompressFromTarGz(stream);
                templateLayer.TemplateContent = TemplateLayerParser.ParseToTemplates(artifacts);
                templateLayer.Digest = digest;
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
