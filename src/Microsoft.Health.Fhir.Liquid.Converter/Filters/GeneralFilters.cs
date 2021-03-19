// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Liquid.Converter
{
    /// <summary>
    /// Filters for conversion
    /// </summary>
    public partial class Filters
    {
        public static string GetProperty(Context context, string originalCode, string mapping, string property = "code")
        {
            if (string.IsNullOrEmpty(originalCode) || string.IsNullOrEmpty(mapping) || string.IsNullOrEmpty(property))
            {
                return null;
            }

            var map = (context["CodeMapping"] as CodeMapping)?.Mapping?.GetValueOrDefault(mapping, null);
            var codeMapping = map?.GetValueOrDefault(originalCode, null) ?? map?.GetValueOrDefault("__default__", null);
            return codeMapping?.GetValueOrDefault(property, null)
                ?? ((property.Equals("code") || property.Equals("display")) ? originalCode : null);
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
            if (string.IsNullOrWhiteSpace(segment))
            {
                return null;
            }

            if (string.IsNullOrEmpty(resourceType) || (isBaseIdRequired && string.IsNullOrEmpty(baseId)))
            {
                throw new RenderException(FhirConverterErrorCode.InvalidIdGenerationInput, Resources.InvalidIdGenerationInput);
            }

            segment = segment.Trim();
            return baseId != null ? $"{resourceType}_{segment}_{baseId}" : $"{resourceType}_{segment}";
        }

        public static string GenerateUUID(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
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
