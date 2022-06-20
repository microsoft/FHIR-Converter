// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Newtonsoft.Json.Schema;

namespace Microsoft.Health.Fhir.Liquid.Converter.Models.Json
{
    public class JSchemaTraceInfo : TraceInfo
    {
        public JSchemaTraceInfo()
        {
        }

        public JSchemaTraceInfo(List<JSchema> validateSchemas)
        {
            ValidateSchemas = validateSchemas;
        }

        public List<JSchema> ValidateSchemas { get; set; }
    }
}
