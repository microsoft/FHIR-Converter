// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Text.RegularExpressions;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Models.Hl7v2;

namespace Microsoft.Health.Fhir.Liquid.Converter.InputProcessors
{
    public static class Hl7v2EscapeSequenceProcessor
    {
        private static readonly Regex UnescapeRegex = new Regex(@"\\(F|S|E|T|R|\.br|X[0-9A-F]+)\\");

        public static string Unescape(string input, Hl7v2EncodingCharacters encodingCharacters)
        {
            string Unescaper(Match match)
            {
                return match.Groups[1].Value switch
                {
                    "F" => encodingCharacters.FieldSeparator.ToString(),
                    "S" => encodingCharacters.ComponentSeparator.ToString(),
                    "T" => encodingCharacters.SubcomponentSeparator.ToString(),
                    "R" => encodingCharacters.RepetitionSeparator.ToString(),
                    "E" => "\\",
                    ".br" => "\\n",
                    _ => HandleHex(match.Groups[1].Value),
                };
            }

            return UnescapeRegex.Replace(input, new MatchEvaluator(Unescaper));
        }

        private static string HandleHex(string input)
        {
            if (input.Length % 2 != 1)
            {
                throw new DataParseException(FhirConverterErrorCode.InvalidHexadecimalNumber, string.Format(Resources.InvalidHexadecimalNumber, input.Substring(1)));
            }

            string result = string.Empty;
            for (var i = 1; i < input.Length - 1; i += 2)
            {
                var hex = input.Substring(i, 2);
                result += char.ConvertFromUtf32(Convert.ToInt32(hex, 16));
            }

            return result;
        }
    }
}
