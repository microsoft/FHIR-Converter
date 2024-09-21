// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Health.Fhir.Liquid.Converter.Fluid.Tags
{
    internal struct EvaluateTag
    {
        public string OutputVariable { get; set; }

        public string TargetTemplate { get; set; }

        public List<(string, string)> Attributes { get; set; }
    }
}
