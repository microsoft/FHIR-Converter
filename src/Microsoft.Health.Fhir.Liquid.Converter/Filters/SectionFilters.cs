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

        public static IDictionary<string, object> GetFirstCcdaSections(Hash data, string sectionNameContent)
        {
            var sectionLists = Filters.GetCcdaSectionLists(data, sectionNameContent);
            var result = new Dictionary<string, object>();
            foreach (var (key, value) in sectionLists)
            {
                result[key] = (value as List<object>)?.First();
            }

            return result;
        }

        public static IDictionary<string, object> GetCcdaSectionLists(Hash data, string sectionNameContent)
        {
            var result = new Dictionary<string, object>();
            var sectionNames = sectionNameContent.Split("|", StringSplitOptions.RemoveEmptyEntries);
            var components = GetComponents(data);

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
                        sectionDict.GetValueOrDefault("title") is Dictionary<string, object> titleDict &&
                        titleDict.GetValueOrDefault("_") is string titleString &&
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

        public static IDictionary<string, object> GetFirstCcdaSectionsByTemplateId(Hash data, string templateIdContent)
        {
            var result = new Dictionary<string, object>();
            var templateIds = templateIdContent.Split("|", StringSplitOptions.RemoveEmptyEntries);
            var components = GetComponents(data);

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

        private static List<object> GetComponents(Hash data)
        {
            var dataComponents = (((data["ClinicalDocument"] as Hash)?
                ["component"] as Hash)?
                ["structuredBody"] as Hash)?
                ["component"];

            if (dataComponents == null)
            {
                return null;
            }

            return dataComponents is List<object> listComponents
                ? listComponents
                : new List<object> { dataComponents };
        }

        private static string NormalizeSectionName(string input)
        {
            return NormalizeSectionNameRegex.Replace(input, "_");
        }
    }
}
