﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotLiquid;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Utilities;

namespace Microsoft.Health.Fhir.TemplateManagement.Models
{
    public class TemplateLayer : OCIArtifactLayer
    {
        public Dictionary<string, Template> TemplateContent { get; set; }

        public static TemplateLayer ReadFromFile(string filePath)
        {
            try
            {
                TemplateLayer templateLayer = new TemplateLayer();
                var rawBytes = File.ReadAllBytes(filePath);
                var artifacts = StreamUtility.DecompressTarGzStream(new MemoryStream(rawBytes));
                templateLayer.TemplateContent = TemplateLayerParser.ParseToTemplates(artifacts);
                templateLayer.Digest = StreamUtility.CalculateDigestFromSha256(File.ReadAllBytes(filePath));
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
