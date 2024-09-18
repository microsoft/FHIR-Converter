// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Values;

namespace Microsoft.Health.Fhir.Liquid.Converter.Fluid.Tags
{
    internal class Evaluate : Expression
    {
        public override ValueTask<FluidValue> EvaluateAsync(TemplateContext context)
        {
            throw new NotImplementedException();
        }
    }
}
