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
        /// <summary>
        /// The default templates map, key is datatype, defulat templates information.
        /// </summary>
        private static IReadOnlyDictionary<DataType, DefaultTemplateInfo> _defaultTemplateMap = new Dictionary<DataType, DefaultTemplateInfo>
        {
            { DataType.Hl7v2, new DefaultTemplateInfo(DataType.Hl7v2, "microsofthealth/hl7v2templates:default", "Hl7v2DefaultTemplates.tar.gz") },
            { DataType.Cda, new DefaultTemplateInfo(DataType.Cda, "microsofthealth/ccdatemplates:default", "CdaDefaultTemplates.tar.gz") },
        };

        public DefaultTemplateInfo(DataType dataType, string imageReference, string templatePath)
        {
            DataType = dataType;
            ImageReference = imageReference;
            TemplatePath = templatePath;
        }

        public static IReadOnlyDictionary<DataType, DefaultTemplateInfo> DefaultTemplateMap => _defaultTemplateMap;

        public DataType DataType { get; set; }

        public string ImageReference { get; set; }

        public string TemplatePath { get; set; }
    }
}
