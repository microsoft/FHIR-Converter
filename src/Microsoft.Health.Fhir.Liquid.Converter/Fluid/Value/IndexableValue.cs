// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using EnsureThat;
using Fluid.Values;

namespace Microsoft.Health.Fhir.Liquid.Converter.Fluid.Value
{
    public class IndexableValue<T> : IFluidIndexable
    {
        private readonly IDictionary<string, T> _collection;

        public IndexableValue(IDictionary<string, T> collection)
        {
            _collection = EnsureArg.IsNotNull(collection, nameof(collection));
        }

        public int Count => _collection.Count;

        public IEnumerable<string> Keys => _collection.Keys;

        public bool TryGetValue(string name, out FluidValue value)
        {
            bool found = _collection.TryGetValue(name, out var result);

            value = result switch
            {
                null => NilValue.Empty,
                FluidValue fluidValue => fluidValue,
                _ => result.ToObjectValue(),
            };
            return found;
        }

        public IDictionary<string, T> GetCollection()
        {
            return _collection;
        }
    }
}
