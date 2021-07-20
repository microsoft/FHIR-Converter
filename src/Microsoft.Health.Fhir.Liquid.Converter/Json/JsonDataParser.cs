// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Liquid.Converter.Json
{
    public class JsonDataParser
    {
        public IDictionary<string, object> Parse(string json)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(json))
                {
                    throw new DataParseException(FhirConverterErrorCode.NullOrWhiteSpaceInput, Resources.NullOrWhiteSpaceInput);
                }

                var data = JsonConvert.DeserializeObject<object>(json, new JsonSerializerSettings { ContractResolver = new DictionaryContractResolver() });

                return new Dictionary<string, object>
                {
                    { Constants.JsonDataKey, data },
                };
            }
            catch (Exception ex)
            {
                throw new DataParseException(FhirConverterErrorCode.InputParsingError, string.Format(Resources.InputParsingError, ex.Message), ex);
            }
        }
    }
}