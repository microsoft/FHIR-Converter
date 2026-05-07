// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;

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
        internal const int MaxVariableSchemaLength = 1048576;
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

        internal static void ValidateTypedVariables(IList<VariableDefinition> variables)
        {
            if (variables == null)
            {
                return;
            }

            if (variables.Count > MaxVariableCount)
            {
                throw new RenderException(FhirConverterErrorCode.InvalidVariable, string.Format(Resources.TooManyVariables, MaxVariableCount, variables.Count));
            }

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var variable in variables)
            {
                if (variable == null)
                {
                    throw new RenderException(FhirConverterErrorCode.InvalidVariable, Resources.InvalidVariable);
                }

                ValidateVariableName(variable.Name);

                if (!seen.Add(variable.Name))
                {
                    throw new RenderException(FhirConverterErrorCode.InvalidVariable, string.Format(Resources.DuplicateVariableName, variable.Name));
                }

                if (variable.Value != null && variable.Value.Length > MaxVariableValueLength)
                {
                    throw new RenderException(FhirConverterErrorCode.InvalidVariable, string.Format(Resources.VariableValueTooLong, variable.Name, MaxVariableValueLength));
                }

                switch (variable.Type)
                {
                    case VariableType.String:
                        ValidateStringVariable(variable);
                        break;
                    case VariableType.Numeric:
                        ValidateNumericVariable(variable);
                        break;
                    case VariableType.Complex:
                        ValidateComplexVariable(variable);
                        break;
                    default:
                        throw new RenderException(FhirConverterErrorCode.InvalidVariable, string.Format(Resources.InvalidVariableType, variable.Name, variable.Type));
                }
            }
        }

        private static void ValidateStringVariable(VariableDefinition variable)
        {
            if (variable.Value == null)
            {
                throw new RenderException(FhirConverterErrorCode.InvalidVariable, string.Format(Resources.VariableValueNull, variable.Name));
            }

            if (variable.Schema != null)
            {
                throw new RenderException(FhirConverterErrorCode.InvalidVariable, string.Format(Resources.VariableSchemaNotAllowed, variable.Name, variable.Type));
            }
        }

        private static void ValidateNumericVariable(VariableDefinition variable)
        {
            if (variable.Value == null)
            {
                throw new RenderException(FhirConverterErrorCode.InvalidVariable, string.Format(Resources.VariableValueNull, variable.Name));
            }

            if (variable.Schema != null)
            {
                throw new RenderException(FhirConverterErrorCode.InvalidVariable, string.Format(Resources.VariableSchemaNotAllowed, variable.Name, variable.Type));
            }

            if (!long.TryParse(variable.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _) &&
                !double.TryParse(variable.Value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out _))
            {
                throw new RenderException(FhirConverterErrorCode.InvalidVariable, string.Format(Resources.VariableNumericParseError, variable.Name, variable.Value));
            }
        }

        private static void ValidateComplexVariable(VariableDefinition variable)
        {
            if (variable.Value == null)
            {
                throw new RenderException(FhirConverterErrorCode.InvalidVariable, string.Format(Resources.VariableValueNull, variable.Name));
            }

            if (string.IsNullOrWhiteSpace(variable.Schema))
            {
                throw new RenderException(FhirConverterErrorCode.InvalidVariable, string.Format(Resources.VariableComplexSchemaRequired, variable.Name));
            }

            if (variable.Schema.Length > MaxVariableSchemaLength)
            {
                throw new RenderException(FhirConverterErrorCode.InvalidVariable, string.Format(Resources.VariableSchemaTooLong, variable.Name, MaxVariableSchemaLength));
            }

            // Parse and validate the JSON Schema
            JsonSchema schema;
            try
            {
                schema = JsonSchema.FromJsonAsync(variable.Schema).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw new RenderException(FhirConverterErrorCode.InvalidVariable, string.Format(Resources.VariableComplexSchemaInvalid, variable.Name, ex.Message));
            }

            // Parse the value as JSON
            JToken valueToken;
            try
            {
                valueToken = JToken.Parse(variable.Value);
            }
            catch (JsonException ex)
            {
                throw new RenderException(FhirConverterErrorCode.InvalidVariable, string.Format(Resources.VariableComplexValueInvalidJson, variable.Name, ex.Message));
            }

            // Validate value against schema
            var errors = schema.Validate(variable.Value);
            if (errors.Count > 0)
            {
                var errorMessages = string.Join("; ", errors.Select(e => e.ToString()));
                throw new RenderException(FhirConverterErrorCode.InvalidVariable, string.Format(Resources.VariableComplexSchemaValidationFailed, variable.Name, errorMessages));
            }
        }
    }
}
