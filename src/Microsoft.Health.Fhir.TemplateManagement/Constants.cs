// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.TemplateManagement
{
    internal static class Constants
    {
        // Accept media type for manifest.
        internal const string MediatypeV2Manifest = "application/vnd.docker.distribution.manifest.v2+json";

        internal const string ImageReferenceFormat = "{0}/{1}:{2}";

        internal const string WhiteoutsLabel = ".wh.";

        internal const long ManifestObjectSizeInByte = 10000;

        internal const string HiddenImageFolder = ".image";

        internal const string HiddenLayersFolder = ".image/layers";

        internal const string HiddenBaseLayerFolder = ".image/base";

        internal const string ManifestFileName = "manifest";

        internal const string BaseLayerFileName = "layer1.tar.gz";

        internal const string UserLayerFileName = "layer2.tar.gz";

        internal const string OverlayMetaJsonFile = ".image/meta.info";

        internal const int TimeOutMilliseconds = 30000;

        internal const string OrasCacheEnvironmentVariableName = "ORAS_CACHE";

        internal const string DefaultOrasCacheEnvironmentVariable = ".oras/cache";

        internal const string OrasFileForWindows = "oras.exe";

        internal const string OrasFileForOSX = "oras-osx";
    }
}
