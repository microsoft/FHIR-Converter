// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Health.Fhir.Liquid.Converter.Models;

namespace Microsoft.Health.Fhir.TemplateManagement.Models
{
    public class DefaultTemplateInfo
    {
        public DefaultTemplateInfo(DataType dataType, string imageReference, string templatePath)
        {
            DataType = dataType;
            ImageReference = imageReference;
            TemplatePath = templatePath;
        }

        /// <summary>
        /// The default templates map, key is image reference, value is default templates information.
        /// </summary>
        public static IReadOnlyDictionary<string, DefaultTemplateInfo> DefaultTemplateMap { get; } = new Dictionary<string, DefaultTemplateInfo>(StringComparer.OrdinalIgnoreCase)
        {
            { "microsofthealth/fhirconverter:default", new DefaultTemplateInfo(DataType.Hl7v2, "microsofthealth/fhirconverter:default", "Hl7v2DefaultTemplates.tar.gz") },
            { "microsofthealth/hl7v2templates:default", new DefaultTemplateInfo(DataType.Hl7v2, "microsofthealth/hl7v2templates:default", "Hl7v2DefaultTemplates.tar.gz") },
            { "microsofthealth/ccdatemplates:default", new DefaultTemplateInfo(DataType.Ccda, "microsofthealth/ccdatemplates:default", "CcdaDefaultTemplates.tar.gz") },
            { "microsofthealth/jsontemplates:default", new DefaultTemplateInfo(DataType.Json, "microsofthealth/jsontemplates:default", "JsonDefaultTemplates.tar.gz") },
        };

        public DataType DataType { get; set; }

        public string ImageReference { get; set; }

        public string TemplatePath { get; set; }
    }
}
