// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using EnsureThat;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Extensions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Liquid.Converter.Parsers
{
    public class JsonDataParser : IDataParser
    {
        private static readonly JsonSerializerSettings DefaultSerializerSettings = new JsonSerializerSettings()
        {
            DateParseHandling = DateParseHandling.None,
        };

        public JsonDataParser()
        : this(DefaultSerializerSettings)
        {
        }

        public JsonDataParser(JsonSerializerSettings jsonSerializerSettings)
        {
            JsonSerializerSettings = EnsureArg.IsNotNull(jsonSerializerSettings, nameof(jsonSerializerSettings));
        }

        protected JsonSerializerSettings JsonSerializerSettings { get; private set; }

        public object Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new DataParseException(FhirConverterErrorCode.NullOrWhiteSpaceInput, Resources.NullOrWhiteSpaceInput);
            }

            try
            {
                return JsonConvert.DeserializeObject<JToken>(json, JsonSerializerSettings).ToObject();
            }
            catch (Exception ex)
            {
                throw new DataParseException(FhirConverterErrorCode.InputParsingError, string.Format(Resources.InputParsingError, ex.Message), ex);
            }
        }
    }
}