// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.TemplateManagement
{
    internal static class Constants
    {
        // Accepted media type for manifest. https://github.com/distribution/distribution/blob/main/docs/spec/manifest-v2-2.md
        internal const string V2MediaTypeManifest = "application/vnd.docker.distribution.manifest.v2+json";

        // Accepted media type for OCI manifest https://github.com/opencontainers/image-spec/blob/main/manifest.md#image-manifest
        internal const string OCIMediaTypeImageManifest = "application/vnd.oci.image.manifest.v1+json";

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
