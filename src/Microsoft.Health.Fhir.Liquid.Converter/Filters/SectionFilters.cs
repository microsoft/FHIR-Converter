// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DotLiquid;

namespace Microsoft.Health.Fhir.Liquid.Converter
{
    /// <summary>
    /// Filters for conversion
    /// </summary>
    public partial class Filters
    {
        private static readonly Regex NormalizeSectionNameRegex = new Regex("[^A-Za-z0-9]");

        // TODO: Remove this after testing
        public static object Test(object input)
        {
            return input;
        }

        public static List<object> ToArray(object input)
        {
            if (input is IEnumerable<object> enumerableObject)
            {
                return enumerableObject.ToList();
            }
            else
            {
                return new List<object> { input };
            }
        }

        public static List<object> Concat(List<object> l1, List<object> l2)
        {
            var result = new List<object>();
            result.AddRange(l1.Where(x => x != null));
            result.AddRange(l2.Where(x => x != null));
            return result;
        }

        public static IDictionary<string, object> GetFirstCdaSections(Hash data, string sectionNameContent)
        {
            var result = new Dictionary<string, object>();
            var sectionNames = sectionNameContent.Split("|");
            var components = ((((data["ClinicalDocument"] as Hash)?
                ["component"] as Hash)?
                ["structuredBody"] as Hash)?
                ["component"] as List<object>)?
                .ToList();

            if (components == null)
            {
                return result;
            }

            foreach (var sectionName in sectionNames)
            {
                foreach (var component in components)
                {
                    if (component is Dictionary<string, object> componentDict &&
                        componentDict.GetValueOrDefault("section") is Dictionary<string, object> sectionDict &&
                        sectionDict.GetValueOrDefault("title") is string titleString &&
                        titleString.Contains(sectionName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        result[NormalizeSectionName(sectionName)] = sectionDict;
                        break;
                    }
                }
            }

            return result;
        }

        public static IDictionary<string, object> GetCdaSectionLists(IDictionary<string, object> data, string sectionNameContent)
        {
            var result = new Dictionary<string, object>();
            var sectionNames = sectionNameContent.Split("|");
            var components = ((((data["ClinicalDocument"] as Hash)?
                ["component"] as Hash)?
                ["structuredBody"] as Hash)?
                ["component"] as List<object>)?
                .ToList();

            if (components == null)
            {
                return result;
            }

            foreach (var sectionName in sectionNames)
            {
                foreach (var component in components)
                {
                    if (component is Dictionary<string, object> componentDict &&
                        componentDict.GetValueOrDefault("section") is Dictionary<string, object> sectionDict &&
                        sectionDict.GetValueOrDefault("title") is string titleString &&
                        titleString.Contains(sectionName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var normalizedSectionName = NormalizeSectionName(sectionName);
                        if (result.GetValueOrDefault(normalizedSectionName) is List<object> list)
                        {
                            list.Add(sectionDict);
                        }
                        else
                        {
                            result[NormalizeSectionName(sectionName)] = new List<object> { sectionDict };
                        }
                    }
                }
            }

            return result;
        }

        public static IDictionary<string, object> GetFirstCdaSectionsByTemplateId(Hash data, string templateIdContent)
        {
            var result = new Dictionary<string, object>();
            var templateIds = templateIdContent.Split("|");
            var components = ((((data["ClinicalDocument"] as Hash)?
                ["component"] as Hash)?
                ["structuredBody"] as Hash)?
                ["component"] as List<object>)?
                .ToList();

            if (components == null)
            {
                return result;
            }

            foreach (var templateId in templateIds)
            {
                foreach (var component in components)
                {
                    if (component is Dictionary<string, object> componentDict &&
                        componentDict.GetValueOrDefault("section") is Dictionary<string, object> sectionDict &&
                        sectionDict.GetValueOrDefault("templateId") != null &&
                        ToJsonString(sectionDict["templateId"]).Contains(templateId, StringComparison.InvariantCultureIgnoreCase))
                    {
                        result[NormalizeSectionName(templateId)] = sectionDict;
                        break;
                    }
                }
            }

            return result;
        }

        private static string NormalizeSectionName(string input)
        {
            return NormalizeSectionNameRegex.Replace(input, "_");
        }
    }
}
