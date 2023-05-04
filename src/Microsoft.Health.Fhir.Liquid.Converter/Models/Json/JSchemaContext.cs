// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using DotLiquid;
using NJsonSchema;

namespace Microsoft.Health.Fhir.Liquid.Converter.Models.Json
{
    public class JSchemaContext : Context
    {
        public JSchemaContext(List<Hash> environments, Hash outerScope, Hash registers, ErrorsOutputMode errorsOutputMode, int maxIterations, IFormatProvider formatProvider, CancellationToken cancellationToken)
             : base(environments, outerScope, registers, errorsOutputMode, maxIterations, formatProvider, cancellationToken)
        {
            ValidateSchemas = new List<JsonSchema>();
        }

        public List<JsonSchema> ValidateSchemas { get; set; }
    }
}
