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

        public static string Hl7v2DefaultTemplatesResource => "Hl7v2DefaultTemplates.tar.gz";

        public static string CcdaDefaultTemplatesResource => "CcdaDefaultTemplates.tar.gz";

        public static string JsonDefaultTemplatesResource => "JsonDefaultTemplates.tar.gz";

        public static string Stu3ToR4DefaultTemplatesResource => "Stu3ToR4DefaultTemplates.tar.gz";

        public static string FhirToHl7v2DefaultTemplatesResource => "FhirToHl7v2DefaultTemplates.tar.gz";

        /// <summary>
        /// The default templates map, key is image reference, value is default templates information.
        /// </summary>
        public static IReadOnlyDictionary<string, DefaultTemplateInfo> DefaultTemplateMap { get; } = new Dictionary<string, DefaultTemplateInfo>(StringComparer.OrdinalIgnoreCase)
        {
            { "microsofthealth/fhirconverter:default", new DefaultTemplateInfo(DataType.Hl7v2, "microsofthealth/fhirconverter:default", Hl7v2DefaultTemplatesResource) },
            { "microsofthealth/hl7v2templates:default", new DefaultTemplateInfo(DataType.Hl7v2, "microsofthealth/hl7v2templates:default", Hl7v2DefaultTemplatesResource) },
            { "microsofthealth/ccdatemplates:default", new DefaultTemplateInfo(DataType.Ccda, "microsofthealth/ccdatemplates:default", CcdaDefaultTemplatesResource) },
            { "microsofthealth/jsontemplates:default", new DefaultTemplateInfo(DataType.Json, "microsofthealth/jsontemplates:default", JsonDefaultTemplatesResource) },
            { "microsofthealth/stu3tor4templates:default", new DefaultTemplateInfo(DataType.Fhir, "microsofthealth/stu3tor4templates:default", Stu3ToR4DefaultTemplatesResource) },
        };

        public DataType DataType { get; set; }

        public string ImageReference { get; set; }

        public string TemplatePath { get; set; }
    }
}
