// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Health.Fhir.Liquid.Converter.Models;

namespace Microsoft.Health.Fhir.TemplateManagement
{
    internal static class Constants
    {
        /// <summary>
        /// The default templates information, key is datatype, value.item1 is template path, value.item2 is image reference.
        /// </summary>
        private static IReadOnlyDictionary<DataType, Tuple<string, string>> _defaultTemplateInfo = new Dictionary<DataType, Tuple<string, string>>
        {
            { DataType.Hl7v2, Tuple.Create("Hl7v2DefaultTemplates.tar.gz", "microsofthealth/hl7v2templates:default") },
            { DataType.Cda, Tuple.Create("CdaDefaultTemplates.tar.gz", "microsofthealth/ccdatemplates:default") },
        };

        internal const DataType DefaultDataType = DataType.Hl7v2;

        // Accept meidia type for manifest.
        internal const string MediatypeV2Manifest = "application/vnd.docker.distribution.manifest.v2+json";

        internal const string ImageReferenceFormat = "{0}/{1}:{2}";

        internal const string WhiteoutsLabel = ".wh.";

        internal const long ManifestObjectSizeInByte = 10000;

        internal const string HiddenImageFolder = ".image";

        internal const string HiddenLayersFolder = ".image/layers";

        internal const string HiddenBaseLayerFolder = ".image/base";

        internal const string OverlayMetaJsonFile = ".image/meta.info";

        internal const int TimeOutMilliseconds = 30000;

        internal const string OrasFile = "oras";

        internal static IReadOnlyDictionary<DataType, Tuple<string, string>> DefaultTemplateInfo => _defaultTemplateInfo;
    }
}
