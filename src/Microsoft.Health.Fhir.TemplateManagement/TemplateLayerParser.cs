// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.Utilities;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Microsoft.Health.Fhir.TemplateManagement.Utilities;

namespace Microsoft.Health.Fhir.TemplateManagement
{
    public static class TemplateLayerParser
    {
        public static TemplateLayer ParseArtifactsLayerToTemplateLayer(ArtifactLayer artifactsLayer)
        {
            TemplateLayer oneTemplateLayer = new TemplateLayer();
            var artifacts = DecompressRawBytesContent((byte[])artifactsLayer.Content);
            var parsedTemplate = ParseToTemplates(artifacts);

            oneTemplateLayer.Content = parsedTemplate;
            oneTemplateLayer.Digest = artifactsLayer.Digest;
            oneTemplateLayer.Size = artifacts.Sum(x => x.Value == null ? 0 : x.Value.Length);
            return oneTemplateLayer;
        }

        public static Dictionary<string, string> DecompressRawBytesContent(byte[] rawBytes)
        {
            Dictionary<string, string> artifacts;
            try
            {
                artifacts = StreamUtility.DecompressTarGzStream(new MemoryStream(rawBytes));
                return artifacts;
            }
            catch (Exception ex)
            {
                throw new ImageDecompressException(TemplateManagementErrorCode.DecompressImageFailed, "Decompress image failed.", ex);
            }
        }

        public static Dictionary<string, Template> ParseToTemplates(Dictionary<string, string> content)
        {
            try
            {
                var parsedTemplate = TemplateUtility.ParseHl7v2Templates(content);
                return parsedTemplate;
            }
            catch (Exception ex)
            {
                throw new TemplateParseException(TemplateManagementErrorCode.ParseTemplatesFailed, "Parse templates from image failed.", ex);
            }
        }
    }
}
