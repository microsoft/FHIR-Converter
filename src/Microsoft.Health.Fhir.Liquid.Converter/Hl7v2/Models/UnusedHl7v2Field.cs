// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Health.Fhir.Liquid.Converter.Hl7v2.Models
{
    public class UnusedHl7v2Field
    {
        public UnusedHl7v2Field(int index, List<UnusedHl7v2Component> components)
        {
            Index = index;
            Components = components;
        }

        public int Index { get; set; }

        public List<UnusedHl7v2Component> Components { get; set; }
    }
}
