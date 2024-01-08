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
    public class Hl7v2Segment : Drop
    {
        public Hl7v2Segment(string value, IEnumerable<Hl7v2Field> fields)
        {
            Value = value;
            Fields = new SafeList<Hl7v2Field>(fields);
        }

        public string SegmentName { get; set; }

        public string Value { get; set; }

        public SafeList<Hl7v2Field> Fields { get; set; }

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

                if (string.Equals(indexString, "Fields", StringComparison.InvariantCultureIgnoreCase))
                {
                    return Fields;
                }

                if (int.TryParse(indexString, out int result))
                {
                    return (Hl7v2Field)Fields[result];
                }

                throw new RenderException(FhirConverterErrorCode.PropertyNotFound, string.Format(Resources.PropertyNotFound, indexString, this.GetType().Name));
            }
        }

        private void SetAccessForAllComponents()
        {
            foreach (var field in Fields)
            {
                if (field is Hl7v2Field hl7v2Field)
                {
                    foreach (var component in hl7v2Field.Components)
                    {
                        if (component is Hl7v2Component hl7V2Component)
                        {
                            hl7V2Component.IsAccessed = true;
                        }
                    }
                }
            }
        }
    }
}
