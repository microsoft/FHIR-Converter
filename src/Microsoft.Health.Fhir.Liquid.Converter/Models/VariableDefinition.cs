// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.Liquid.Converter.Models
{
    public enum VariableType
    {
        String,
        Numeric,
        Complex,
    }

    public class VariableDefinition
    {
        public string Name { get; set; }

        public VariableType Type { get; set; }

        public string Value { get; set; }

        /// <summary>
        /// JSON Schema text. Required for Complex type, must be null for String and Numeric types.
        /// </summary>
        public string Schema { get; set; }
    }
}
