// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;

namespace Microsoft.Health.Fhir.Liquid.Converter.Utilities
{
    internal static class VariableValidator
    {
        private static readonly Regex ValidVariableNameRegex = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]*$", RegexOptions.Compiled);

        // These are the keys used by the template context (e.g., "msg" for input data, "vars" for variables).
        // User-provided variable names must not collide with these.
        internal static readonly HashSet<string> ReservedTemplateContextVariableNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "msg", "vars" };

        internal const int MaxVariableNameLength = 128;
        internal const int MaxVariableValueLength = 1048576;
        internal const int MaxVariableCount = 100;

        internal static void ValidateVariables(IDictionary<string, string> variables)
        {
            if (variables == null)
            {
                return;
            }

            if (variables.Count > MaxVariableCount)
            {
                throw new RenderException(FhirConverterErrorCode.InvalidVariable, string.Format(Resources.TooManyVariables, MaxVariableCount, variables.Count));
            }

            // Check for case-insensitive duplicate keys
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in variables)
            {
                ValidateVariableName(kvp.Key);

                if (!seen.Add(kvp.Key))
                {
                    throw new RenderException(FhirConverterErrorCode.InvalidVariable, string.Format(Resources.DuplicateVariableName, kvp.Key));
                }

                if (kvp.Value != null && kvp.Value.Length > MaxVariableValueLength)
                {
                    throw new RenderException(FhirConverterErrorCode.InvalidVariable, string.Format(Resources.VariableValueTooLong, kvp.Key, MaxVariableValueLength));
                }
            }
        }

        internal static void ValidateVariableName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new RenderException(FhirConverterErrorCode.InvalidVariable, Resources.InvalidVariable);
            }

            if (name.Length > MaxVariableNameLength)
            {
                throw new RenderException(FhirConverterErrorCode.InvalidVariable, string.Format(Resources.VariableNameTooLong, MaxVariableNameLength));
            }

            if (!ValidVariableNameRegex.IsMatch(name))
            {
                throw new RenderException(FhirConverterErrorCode.InvalidVariable, string.Format(Resources.InvalidVariableNameFormat, name));
            }

            if (ReservedTemplateContextVariableNames.Contains(name))
            {
                throw new RenderException(FhirConverterErrorCode.InvalidVariable, string.Format(Resources.ReservedVariableName, name));
            }
        }
    }
}
