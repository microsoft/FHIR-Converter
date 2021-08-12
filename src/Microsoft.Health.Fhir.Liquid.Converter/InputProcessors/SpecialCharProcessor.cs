// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Text.RegularExpressions;

namespace Microsoft.Health.Fhir.Liquid.Converter.InputProcessors
{
    public static class SpecialCharProcessor
    {
        private static readonly Regex EscapeRegex = new Regex(@"\\|""");
        private static readonly Regex UnescapeRegex = new Regex(@"\\(\\|"")");

        public static string Escape(string input)
        {
            var evaluator = new MatchEvaluator(match => $"\\{match.Value}");
            return EscapeRegex.Replace(input, evaluator);
        }

        public static string Unescape(string input)
        {
            var evaluator = new MatchEvaluator(match => match.Groups[1].Value);
            return UnescapeRegex.Replace(input, evaluator);
        }
    }
}
