// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Fhir.Liquid.Converter.Models;

namespace Microsoft.Health.Fhir.Liquid.Converter.Exceptions
{
    /// <summary>
    /// FhirConverterException is thrown when exceptions happen during conversion.
    /// It could be caused by invalid template directory, invalid data format, invalid JSON generated for postprocessing, etc.
    /// </summary>
    public class FhirConverterException : Exception
    {
        public FhirConverterException(FhirConverterErrorCode fhirConverterErrorCode, string message)
            : base(message)
        {
            FhirConverterErrorCode = fhirConverterErrorCode;
        }

        public FhirConverterException(FhirConverterErrorCode fhirConverterErrorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            FhirConverterErrorCode = fhirConverterErrorCode;
        }

        public FhirConverterErrorCode FhirConverterErrorCode { get; }
    }
}
