// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Extensions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Liquid.Converter.Parsers
{
    public class FhirDataParser : IDataParser
    {
        public object Parse(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new DataParseException(FhirConverterErrorCode.NullOrWhiteSpaceInput, Resources.NullOrWhiteSpaceInput);
            }

            try
            {
                return JToken.Parse(message).ToObject();
            }
            catch (Exception ex)
            {
                throw new DataParseException(FhirConverterErrorCode.InputParsingError, string.Format(Resources.InputParsingError, ex.Message), ex);
            }
        }
    }
}