// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Health.Fhir.Liquid.Converter.Hl7v2.InputProcessor;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Liquid.Converter
{
    /// <summary>
    /// Filters for conversion
    /// </summary>
    public partial class Filters
    {
        public static char CharAt(string data, int index)
        {
            return data[index];
        }

        public static bool Contains(string parentString, string childString)
        {
            return parentString != null && parentString.Contains(childString);
        }

        public static string EscapeSpecialChars(string data)
        {
            return string.IsNullOrEmpty(data) ? data : SpecialCharProcessor.Escape(data);
        }

        public static string UnescapeSpecialChars(string data)
        {
            return string.IsNullOrEmpty(data) ? data : SpecialCharProcessor.Unescape(data);
        }

        public static List<string> Match(string data, string regexString)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(data))
            {
                return result;
            }

            var regex = new Regex(regexString);
            var matches = regex.Match(data);
            foreach (Match match in matches.Captures)
            {
                result.Add(match.Value);
            }

            return result;
        }

        public static string ToJsonString(object data)
        {
            return data == null ? null : JsonConvert.SerializeObject(data, Formatting.None);
        }

        public static string Gzip(string data)
        {
            using var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(data));
            using var outputStream = new MemoryStream();
            using (var gzipStream = new GZipStream(outputStream, CompressionMode.Compress))
            {
                inputStream.CopyTo(gzipStream);
            }

            return Convert.ToBase64String(outputStream.ToArray());
        }

        public static string Gunzip(string data)
        {
            using var inputStream = new MemoryStream(Convert.FromBase64String(data));
            using var outputStream = new MemoryStream();
            using (var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress))
            {
                gzipStream.CopyTo(outputStream);
            }

            return Encoding.UTF8.GetString(outputStream.ToArray());
        }

        public static string Sha1Hash(string data)
        {
            var hash = new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(data));
            return string.Concat(hash.Select(b => b.ToString("x2")));
        }

        public static string Base64Encode(string data)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(data));
        }

        public static string Base64Decode(string data)
        {
            var bytes = Convert.FromBase64String(data);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
