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

        // ImageTooLargeException
        ImageSizeTooLarge = 2701,

        // ImageDecompressedException
        DecompressImageFailed = 2801,

        // TemplateParseException
        ParseTemplatesFailed = 2901,

        // OverlayException
        ImageLayersNotFound = 3101,
        OverlayMetaJsonNotFound = 3102,
        SequenceNumberReadFailed = 3103,
        SortLayersFailed = 3104,
        BaseLayerNotFound = 3105,

        // OrasException
        OrasTimeOut = 3201,
    }
}
