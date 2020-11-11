// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Fhir.Liquid.Converter.Models;

namespace Microsoft.Health.Fhir.Liquid.Converter.Exceptions
{
    public class DataFormatException : FhirConverterException
    {
        public DataFormatException(FhirConverterErrorCode fhirConverterErrorCode, string message)
            : base(fhirConverterErrorCode, message)
        {
        }
    }
}
