// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotLiquid;

namespace Microsoft.Health.Fhir.Liquid.Converter.DotLiquids
{
    public class SafeList<T> : IEnumerable, IIndexable, ILiquidizable
        where T : class
    {
        private IList<T> _internalList;

        public SafeList()
        {
            _internalList = new List<T>();
        }

        public SafeList(IEnumerable<T> internalList)
        {
            _internalList = internalList.ToList();
        }

        public int Count => _internalList.Count;

        public virtual object this[object key]
        {
            get
            {
                return key is int index && ContainsKey(key) ? _internalList[index] : default;
            }

            set
            {
                if (key is int index)
                {
                    if (index < _internalList.Count)
                    {
                        _internalList[index] = value as T;
                    }
                    else
                    {
                        _internalList.Add(value as T);
                    }
                }
            }
        }

        public virtual bool ContainsKey(object key)
        {
            return key is int index && index >= 0 && index < _internalList.Count;
        }

        public virtual void Add(T item)
        {
            _internalList.Add(item);
        }

        public virtual IEnumerator GetEnumerator()
        {
            return _internalList.GetEnumerator();
        }

        public virtual object ToLiquid()
        {
            return this;
        }
    }
}
