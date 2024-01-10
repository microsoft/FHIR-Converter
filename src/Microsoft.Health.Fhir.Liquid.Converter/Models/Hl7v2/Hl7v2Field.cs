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
    public class Hl7v2Field : Drop
    {
        public Hl7v2Field(string value, IEnumerable<Hl7v2Component> components)
        {
            Value = value;
            Components = new SafeList<Hl7v2Component>(components);
            Repeats = new SafeList<Hl7v2Field>();
        }

        public Hl7v2Field()
        {
        }

        public string Value { get; set; }

        public SafeList<Hl7v2Component> Components { get; set; }

        public SafeList<Hl7v2Field> Repeats { get; set; }

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
                    SetAccessForAllComponents();
                    return Value;
                }

                if (string.Equals(indexString, "Components", StringComparison.InvariantCultureIgnoreCase))
                {
                    return Components;
                }

                if (string.Equals(indexString, "Repeats", StringComparison.InvariantCultureIgnoreCase))
                {
                    SetAccessForAllComponents();
                    return Repeats;
                }

                if (int.TryParse(indexString, out int result))
                {
                    return (Hl7v2Component)Components[result];
                }

                throw new RenderException(FhirConverterErrorCode.PropertyNotFound, string.Format(Resources.PropertyNotFound, indexString, this.GetType().Name));
            }
        }

        private void SetAccessForAllComponents()
        {
            foreach (var component in Components)
            {
                if (component is Hl7v2Component hl7V2Component)
                {
                    hl7V2Component.IsAccessed = true;
                }
            }
        }
    }
}
