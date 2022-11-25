// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Microsoft.Health.Fhir.TemplateManagement.Models
{
    public class Digest
    {
        // Format of digest is: <algorithm>:<hex>
        // e.g. sha256:d377125165eb6d770f344429a7a55379d4028774aebe267fe620cd1fcd2daab7
        private static readonly Regex _digestRegex = new Regex("(?<algorithm>[A-Za-z][A-Za-z0-9]*([+.-_][A-Za-z][A-Za-z0-9]*)*):(?<hex>[0-9a-fA-F]{32,})");

        public string Algorithm { get; set; }

        public string Hex { get; set; }

        public string Value
        {
            get { return string.Concat(Algorithm, ":", Hex); }
        }

        public static List<Digest> GetDigest(string input)
        {
            var digests = new List<Digest>();
            foreach (Match digest in _digestRegex.Matches(input))
            {
                digests.Add(new Digest() { Algorithm = digest.Groups["algorithm"].ToString(), Hex = digest.Groups["hex"].ToString() });
            }

            return digests;
        }

        public static bool IsDigest(string input)
        {
            return _digestRegex.IsMatch(input);
        }
    }
}
