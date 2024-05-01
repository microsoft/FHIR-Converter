// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.TemplateManagement.Models
{
    public enum TemplateManagementErrorCode
    {
        // DefaultTemplatesInitializeException
        InitializeDefaultTemplateFailed = 2101,

        // ImageReferenceException
        InvalidReference = 2201,

        // ContainerRegistryAuthenticationException
        RegistryUnauthorized = 2301,
        AccessForbidden = 2302,

        // ImageNotFoundException
        ImageNotFound = 2401,

        // ImageFetchException
        FetchManifestFailed = 2501,
        FetchLayerFailed = 2502,
        CacheError = 2503,

        // ImageValidationException
        InvalidManifestInfo = 2601,
        InvalidBlobContent = 2602,

        // TemplateCollectionTooLargeException
        ImageSizeTooLarge = 2701,
        BlobTemplateCollectionTooLarge = 2702,

        // ArtifactArchiveException
        DecompressArtifactFailed = 2801,
        CompressArtifactFailed = 2802,

        // TemplateParseException
        ParseTemplatesFailed = 2901,

        // OverlayException
        ImageLayersNotFound = 3101,
        OverlayMetaJsonInvalid = 3102,
        BaseLayerLoadFailed = 3103,

        // OrasException
        OrasTimeOut = 3201,
        OrasProcessFailed = 3202,
        OrasCacheManifestFailed = 3203,
        OrasCacheBlobFailed = 3204,
    }
}
