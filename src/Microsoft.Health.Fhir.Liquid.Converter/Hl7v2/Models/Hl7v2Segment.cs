// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.DotLiquids;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;

namespace Microsoft.Health.Fhir.Liquid.Converter.Hl7v2.Models
{
    public class Hl7v2Segment : Drop
    {
        public Hl7v2Segment(string value, IEnumerable<Hl7v2Field> fields)
        {
            Value = value;
            Fields = new SafeList<Hl7v2Field>(fields);
        }

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
                    return Value;
                }
                else if (string.Equals(indexString, "Fields", StringComparison.InvariantCultureIgnoreCase))
                {
                    return Fields;
                }
                else if (int.TryParse(indexString, out int result))
                {
                    return (Hl7v2Field)Fields[result];
                }
                else
                {
                    throw new RenderException(FhirConverterErrorCode.PropertyNotFound, string.Format(Resources.PropertyNotFound, indexString, this.GetType().Name));
                }
            }
        }
    }
}
