// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fluid.Values;

namespace Microsoft.Health.Fhir.Liquid.Converter.Fluid.Value
{
    internal static class ValueExtensions
    {
        public static ObjectValue ToObjectValue(this object obj)
        {
            return new ObjectValue(obj);
        }

        public static ArrayValue ToArrayValue<T>(this IEnumerable<T> collection)
        {
            return new ArrayValue(collection.Select(v => v.ToObjectValue()).ToList());
        }

        public static ArrayValue ToArrayValue(this IEnumerable collection)
        {
            var list = new List<FluidValue>();

            foreach (var item in collection)
            {
                list.Add(item.ToObjectValue());
            }

            return new ArrayValue(list);
        }

        public static IFluidIndexable ToIndexableValue<T>(this IDictionary<string, T> collection)
        {
            return new IndexableValue<T>(collection);
        }

        public static DictionaryValue ToDictionaryValue<T>(this IDictionary<string, T> collection)
        {
            return new DictionaryValue(collection.ToIndexableValue());
        }
    }
}
