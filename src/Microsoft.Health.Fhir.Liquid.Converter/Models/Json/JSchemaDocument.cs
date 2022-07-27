// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using DotLiquid;
using NJsonSchema;

namespace Microsoft.Health.Fhir.Liquid.Converter.Models.Json
{
    public class JSchemaDocument : Document
    {
        public JSchemaDocument(JsonSchema schema)
        {
            if (schema == null)
            {
                throw new ArgumentException(string.Format(Resources.InvalidJsonSchemaContent, "Schema cannot be null"));
            }

            Schema = schema;
        }

        public JsonSchema Schema { get; set; }
    }
}
