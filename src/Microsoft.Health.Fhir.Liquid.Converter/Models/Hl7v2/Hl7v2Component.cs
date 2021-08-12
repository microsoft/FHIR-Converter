// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.DotLiquids;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;

namespace Microsoft.Health.Fhir.Liquid.Converter.Models.Hl7v2
{
    public class Hl7v2Component : Drop
    {
        public Hl7v2Component(string value, IEnumerable<string> subcomponents)
        {
            IsAccessed = false;
            Value = value;
            Subcomponents = new SafeList<string>(subcomponents);
        }

        public bool IsAccessed { get; set; }

        public string Value { get; set; }

        public SafeList<string> Subcomponents { get; set; }

        public override object this[object index]
        {
            get
            {
                if (!(index is string || index is int))
                {
                    throw new RenderException(FhirConverterErrorCode.PropertyNotFound, string.Format(Resources.PropertyNotFound, index, this.GetType().Name));
                }

                IsAccessed = true;
                var indexString = index.ToString();

                if (string.Equals(indexString, "Value", StringComparison.InvariantCultureIgnoreCase))
                {
                    return Value;
                }

                if (string.Equals(indexString, "Subcomponents", StringComparison.InvariantCultureIgnoreCase))
                {
                    return Subcomponents;
                }

                if (int.TryParse(indexString, out int result))
                {
                    return (string)Subcomponents[result];
                }

                throw new RenderException(FhirConverterErrorCode.PropertyNotFound, string.Format(Resources.PropertyNotFound, indexString, this.GetType().Name));
            }
        }
    }
}
