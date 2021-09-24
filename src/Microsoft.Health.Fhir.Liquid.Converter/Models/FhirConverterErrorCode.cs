// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.Liquid.Converter.Models
{
    /// <summary>
    /// First digit: 1 for FhirConverterException
    /// Second digit: different detailed exception types
    /// Third and forth digits: error code number
    /// </summary>
    public enum FhirConverterErrorCode
    {
        // TemplateLoadException
        TemplateFolderNotFound = 1101,
        TemplateLoadingError = 1102,
        InvalidCodeMapping = 1103,
        TemplateSyntaxError = 1104,

        // DataParseException
        InputParsingError = 1201,
        NullOrWhiteSpaceInput = 1202,
        InvalidHl7v2Message = 1203,
        MissingHl7v2Separators = 1204,
        DuplicateHl7v2Separators = 1205,
        InvalidHl7v2EscapeCharacter = 1206,

        // This could be the inner exception of RenderException because it is used in filters
        InvalidHexadecimalNumber = 1207,

        // RenderException
        TemplateRenderingError = 1301,
        PropertyNotFound = 1302,
        NullOrEmptyRootTemplate = 1303,
        NullTemplateProvider = 1304,
        TemplateNotFound = 1305,
        TimeoutError = 1306,
        InvalidDateTimeFormat = 1307,
        InvalidIdGenerationInput = 1308,
        InvalidTimeZoneHandling = 1309,

        // PostprocessException
        JsonParsingError = 1401,
        JsonMergingError = 1402,
        TraceInfoError = 1403,
    }
}
