﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.Utilities;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Microsoft.Health.Fhir.TemplateManagement.Utilities;

namespace Microsoft.Health.Fhir.TemplateManagement
{
    public static class TemplateLayerParser
    {
        public static TemplateLayer ParseArtifactsLayerToTemplateLayer(OCIArtifactLayer artifactsLayer)
        {
            TemplateLayer oneTemplateLayer = new TemplateLayer();
            var artifacts = StreamUtility.DecompressTarGzStream(new MemoryStream(artifactsLayer.Content));
            var parsedTemplate = ParseToTemplates(artifacts);

            oneTemplateLayer.TemplateContent = parsedTemplate;
            oneTemplateLayer.Digest = artifactsLayer.Digest;
            oneTemplateLayer.Size = artifacts.Sum(x => x.Value == null ? 0 : x.Value.Length);
            return oneTemplateLayer;
        }

        public static Dictionary<string, Template> ParseToTemplates(Dictionary<string, byte[]> content)
        {
            try
            {
                var fileContent = content.ToDictionary(
                    item => item.Key,
                    item => item.Value == null ? null : Encoding.UTF8.GetString(item.Value));
                var parsedTemplate = TemplateUtility.ParseHl7v2Templates(fileContent);
                return parsedTemplate;
            }
            catch (Exception ex)
            {
                throw new TemplateParseException(TemplateManagementErrorCode.ParseTemplatesFailed, "Parse templates from image failed.", ex);
            }
        }
    }
}
