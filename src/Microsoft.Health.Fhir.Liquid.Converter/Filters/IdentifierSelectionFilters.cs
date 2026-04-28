// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;

namespace Microsoft.Health.Fhir.Liquid.Converter
{
    /// <summary>
    /// Filters for FHIR R4 identifier selection based on structured criteria.
    /// </summary>
    public partial class Filters
    {
        // Paths that resolve within a coding entry and must be evaluated together on a single coding.
        private static readonly HashSet<string> CodingLevelPaths = new HashSet<string>(StringComparer.Ordinal)
        {
            "type.coding.code",
            "type.coding.system",
            "type.coding.display",
        };

        public static object SelectIdentifier(object identifiers, object selectionCriteria)
        {
            if (selectionCriteria == null)
            {
                throw new RenderException(FhirConverterErrorCode.TemplateRenderingError, string.Format(Resources.IdentifierSelectionInvalidCriteria, "selectionCriteria is null"));
            }

            var identifierList = ToObjectList(identifiers);
            if (identifierList == null)
            {
                throw new RenderException(FhirConverterErrorCode.TemplateRenderingError, Resources.IdentifierSelectionInvalidInput);
            }

            var conditions = ExtractConditions(selectionCriteria);
            if (conditions.Count == 0)
            {
                throw new RenderException(FhirConverterErrorCode.TemplateRenderingError, string.Format(Resources.IdentifierSelectionInvalidCriteria, "conditions array is empty"));
            }

            // Partition conditions into identifier-level and coding-level.
            var identifierLevelConditions = conditions.Where(c => !CodingLevelPaths.Contains(c.Path)).ToList();
            var codingLevelConditions = conditions.Where(c => CodingLevelPaths.Contains(c.Path)).ToList();

            var matches = new List<object>();

            foreach (var identifier in identifierList)
            {
                if (MatchesIdentifier(identifier, identifierLevelConditions, codingLevelConditions))
                {
                    matches.Add(identifier);
                }
            }

            if (matches.Count == 0)
            {
                throw new RenderException(FhirConverterErrorCode.TemplateRenderingError, Resources.IdentifierSelectionNoMatch);
            }

            if (matches.Count > 1)
            {
                throw new RenderException(FhirConverterErrorCode.TemplateRenderingError, string.Format(Resources.IdentifierSelectionMultipleMatches, matches.Count));
            }

            return matches[0];
        }

        private static bool MatchesIdentifier(object identifier, List<SelectionCondition> identifierLevelConditions, List<SelectionCondition> codingLevelConditions)
        {
            // Check all identifier-level conditions.
            foreach (var condition in identifierLevelConditions)
            {
                var actualValue = ResolveIdentifierPath(identifier, condition.Path);
                if (!string.Equals(actualValue, condition.Value, StringComparison.Ordinal))
                {
                    return false;
                }
            }

            // Check coding-level conditions: a single coding entry must satisfy ALL of them.
            if (codingLevelConditions.Count > 0)
            {
                var codingArray = GetCodingArray(identifier);
                if (codingArray == null || codingArray.Length == 0)
                {
                    return false;
                }

                bool anyCodingSatisfiesAll = false;
                foreach (var coding in codingArray)
                {
                    bool thisCodingSatisfiesAll = true;
                    foreach (var condition in codingLevelConditions)
                    {
                        // Extract the property name after "type.coding." prefix.
                        var codingProperty = condition.Path.Substring("type.coding.".Length);
                        var actualValue = ResolveSingleProperty(coding, codingProperty);
                        if (!string.Equals(actualValue, condition.Value, StringComparison.Ordinal))
                        {
                            thisCodingSatisfiesAll = false;
                            break;
                        }
                    }

                    if (thisCodingSatisfiesAll)
                    {
                        anyCodingSatisfiesAll = true;
                        break;
                    }
                }

                if (!anyCodingSatisfiesAll)
                {
                    return false;
                }
            }

            return true;
        }

        private static string ResolveIdentifierPath(object node, string path)
        {
            // Supported identifier-level paths: "system", "type.text"
            var segments = path.Split('.');
            object current = node;
            foreach (var segment in segments)
            {
                current = ResolveSinglePropertyObject(current, segment);
                if (current == null)
                {
                    return null;
                }
            }

            return current?.ToString();
        }

        private static object[] GetCodingArray(object identifier)
        {
            // Navigate identifier -> type -> coding
            var type = ResolveSinglePropertyObject(identifier, "type");
            if (type == null)
            {
                return null;
            }

            var coding = ResolveSinglePropertyObject(type, "coding");
            return coding switch
            {
                object[] arr => arr,
                List<object> list => list.ToArray(),
                _ => null,
            };
        }

        private static string ResolveSingleProperty(object node, string propertyName)
        {
            var result = ResolveSinglePropertyObject(node, propertyName);
            return result?.ToString();
        }

        private static object ResolveSinglePropertyObject(object node, string propertyName)
        {
            return node switch
            {
                Hash hash => hash.ContainsKey(propertyName) ? hash[propertyName] : null,
                IDictionary<string, object> dict => dict.TryGetValue(propertyName, out var val) ? val : null,
                _ => null,
            };
        }

        private static List<object> ToObjectList(object input)
        {
            return input switch
            {
                object[] arr => arr.ToList(),
                List<object> list => list,
                IEnumerable<object> enumerable => enumerable.ToList(),
                _ => null,
            };
        }

        private static List<SelectionCondition> ExtractConditions(object selectionCriteria)
        {
            var conditionsObj = ResolveSinglePropertyObject(selectionCriteria, "conditions");
            var conditionList = ToObjectList(conditionsObj);
            if (conditionList == null)
            {
                return new List<SelectionCondition>();
            }

            var result = new List<SelectionCondition>();
            foreach (var item in conditionList)
            {
                var path = ResolveSingleProperty(item, "path");
                var value = ResolveSingleProperty(item, "value");
                if (!string.IsNullOrEmpty(path) && value != null)
                {
                    result.Add(new SelectionCondition { Path = path, Value = value });
                }
            }

            return result;
        }

        private class SelectionCondition
        {
            public string Path { get; set; }

            public string Value { get; set; }
        }
    }
}
