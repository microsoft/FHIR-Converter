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
        // DataFormatException
        NullOrEmptyInput = 1101,
        InvalidHl7v2Message = 1102,
        MissingHl7v2Separators = 1103,
        DuplicateHl7v2Separators = 1104,
        InvalidHl7v2EscapeCharacter = 1105,
        InvalidHexadecimalNumber = 1106,
        InvalidDateTimeFormat = 1107,
        InvalidIdGenerationInput = 1108,

        // InitializeException
        TemplateFolderNotFound = 1201,
        TemplateLoadingError = 1202,
        InvalidCodeSystemMapping = 1203,
        TemplateSyntaxError = 1204,

        // DataParseException
        InputParsingError = 1301,

        // RenderException
        TemplateRenderingError = 1401,
        PropertyNotFound = 1402,
        NullOrEmptyEntryTemplate = 1403,
        NullTemplateProvider = 1404,
        TemplateNotFound = 1405,

        // PostprocessException
        JsonParsingError = 1501,
        JsonMergingError = 1502,
    }
}
