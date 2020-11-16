// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Security.Cryptography;
using System.Text;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Hl7v2.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Liquid.Converter
{
    /// <summary>
    /// Filters for conversion
    /// </summary>
    public partial class Filters
    {
        public static string GetProperty(Context context, string originalCode, string mapping, string property)
        {
            if (string.IsNullOrEmpty(originalCode) || string.IsNullOrEmpty(mapping) || string.IsNullOrEmpty(property))
            {
                return null;
            }

            var map = ((CodeSystemMapping)context["CodeSystemMapping"])?.Mapping;
            return map != null &&
                map.ContainsKey(mapping) &&
                map[mapping].ContainsKey(originalCode) &&
                map[mapping][originalCode].ContainsKey(property) ?
                map[mapping][originalCode][property] :
                null;
        }

        public static string Evaluate(string input, string property)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(property))
            {
                return null;
            }

            var obj = JObject.Parse(input);
            var memberToken = obj.SelectToken(property);
            return memberToken?.Value<string>();
        }

        public static string GenerateIdInput(string segment, string resourceType, bool isBaseIdRequired, string baseId = null)
        {
            if (string.IsNullOrEmpty(segment) || string.IsNullOrEmpty(resourceType) || (isBaseIdRequired && string.IsNullOrEmpty(baseId)))
            {
                throw new DataFormatException(FhirConverterErrorCode.InvalidIdGenerationInput, Resources.InvalidIdGenerationInput);
            }

            return baseId != null ? $"{baseId}_{resourceType}_{segment}" : $"{resourceType}_{segment}";
        }

        public static string GenerateUUID(object input)
        {
            if (input is string)
            {
                return GenerateUuidFromString((string)input);
            }
            else if (input is Hl7v2Segment || input is Hl7v2Field || input is Hl7v2Component)
            {
                return GenerateUuidFromString(input.GetType().GetProperty("Value").GetValue(input, null)?.ToString());
            }
            else
            {
                throw new DataFormatException(FhirConverterErrorCode.InvalidIdGenerationInput, Resources.InvalidIdGenerationInput);
            }
        }

        private static string GenerateUuidFromString(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new DataFormatException(FhirConverterErrorCode.InvalidIdGenerationInput, Resources.InvalidIdGenerationInput);
            }

            var bytes = Encoding.UTF8.GetBytes(input);
            var algorithm = SHA256.Create();
            var hash = algorithm.ComputeHash(bytes);
            var guid = new byte[16];
            Array.Copy(hash, 0, guid, 0, 16);
            return new Guid(guid).ToString();
        }
    }
}
