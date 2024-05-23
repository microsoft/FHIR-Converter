// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.IO;
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
        private static Func<string, JsonReader> _defaultJsonReaderGenerator = (json) => new JsonTextReader(new StringReader(json))
        {
            DateParseHandling = DateParseHandling.None,
        };

        public JsonDataParser()
        : this(_defaultJsonReaderGenerator)
        {
        }

        public JsonDataParser(Func<string, JsonReader> jsonReaderGenerator)
        {
            this.JsonReaderGenerator = EnsureArg.IsNotNull(jsonReaderGenerator, nameof(jsonReaderGenerator));
        }

        protected Func<string, JsonReader> JsonReaderGenerator { get; }

        public object Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new DataParseException(FhirConverterErrorCode.NullOrWhiteSpaceInput, Resources.NullOrWhiteSpaceInput);
            }

            try
            {
                using var reader = JsonReaderGenerator.Invoke(json);
                return JToken.ReadFrom(reader).ToObject();
            }
            catch (Exception ex)
            {
                throw new DataParseException(FhirConverterErrorCode.InputParsingError, string.Format(Resources.InputParsingError, ex.Message), ex);
            }
        }
    }
}