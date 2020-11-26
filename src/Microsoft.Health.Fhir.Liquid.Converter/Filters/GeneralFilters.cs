// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Hl7v2.InputProcessor;
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
        private static readonly Regex SegmentSeparatorsRegex = new Regex(@"\s+\r\n|\r|\n");

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

        public static string GenerateIdInput(Context context, string segment, string resourceType, bool isBaseIdRequired, string baseId = null)
        {
            if (string.IsNullOrWhiteSpace(segment))
            {
                return null;
            }

            if (string.IsNullOrEmpty(resourceType) || (isBaseIdRequired && string.IsNullOrEmpty(baseId)))
            {
                throw new RenderException(FhirConverterErrorCode.InvalidIdGenerationInput, Resources.InvalidIdGenerationInput);
            }

            // Normalize encoding characters
            var encodingCharacters = (Hl7v2EncodingCharacters)context["EncodingCharacters"];
            var separators = $"{encodingCharacters.ComponentSeparator}{encodingCharacters.RepetitionSeparator}{encodingCharacters.EscapeCharacter}{encodingCharacters.SubcomponentSeparator}";
            var formattedSegment = segment.Trim().Replace(separators, ",,,,");

            formattedSegment = SegmentSeparatorsRegex.Replace(formattedSegment, ",");
            formattedSegment = formattedSegment.Replace(encodingCharacters.FieldSeparator, ',')
                .Replace(encodingCharacters.ComponentSeparator, ',')
                .Replace(encodingCharacters.SubcomponentSeparator, ',')
                .Replace(encodingCharacters.RepetitionSeparator, ',');

            formattedSegment = SpecialCharProcessor.Escape(Hl7v2EscapeSequenceProcessor.Unescape(formattedSegment, encodingCharacters));

            return baseId != null ? $"{baseId}_{resourceType}_{formattedSegment}" : $"{resourceType}_{formattedSegment}";
        }

        public static string GenerateUUID(string input)
        {
            if (string.IsNullOrEmpty(input))
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
