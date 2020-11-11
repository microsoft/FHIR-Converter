// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;

namespace Microsoft.Health.Fhir.Liquid.Converter.Hl7v2.Models
{
    public class Hl7v2Field : Drop
    {
        public Hl7v2Field(string value, IEnumerable<Hl7v2Component> components)
        {
            Value = value;
            Components = new SafeList<Hl7v2Component>(components);
            Repeats = new List<Hl7v2Field>();
        }

        public string Value { get; set; }

        public SafeList<Hl7v2Component> Components { get; set; }

        public List<Hl7v2Field> Repeats { get; set; }

        public override object this[object index]
        {
            get
            {
                if (!(index is string || index is int))
                {
                    throw new RenderException(FhirConverterErrorCode.PropertyNotFound, string.Format(Resources.PropertyNotFound, index, this.GetType().Name));
                }

                var indexString = index.ToString();
                if (string.Equals(indexString, "Value", StringComparison.InvariantCultureIgnoreCase))
                {
                    return Value;
                }
                else if (string.Equals(indexString, "Components", StringComparison.InvariantCultureIgnoreCase))
                {
                    return Components;
                }
                else if (string.Equals(indexString, "Repeats", StringComparison.InvariantCultureIgnoreCase))
                {
                    return Repeats;
                }
                else if (int.TryParse(indexString, out int result))
                {
                    return (Hl7v2Component)Components[result];
                }
                else
                {
                    throw new RenderException(FhirConverterErrorCode.PropertyNotFound, string.Format(Resources.PropertyNotFound, indexString, this.GetType().Name));
                }
            }
        }
    }
}
