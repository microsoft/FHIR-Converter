// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using static jsonParser;

namespace Microsoft.Health.Fhir.Liquid.Converter.OutputProcessors
{
    public class JsonListener : jsonBaseListener
    {
        private Stack _jsonStack = new Stack();

        public override void ExitJson(JsonContext context)
        {
            var childText = _jsonStack.Pop();
            _jsonStack.Push(childText ?? "{}");
        }

        public override void ExitObj(ObjContext context)
        {
            List<object> pairArray = new List<object>();
            for (var i = 0; i < context.ChildCount; ++i)
            {
                if (context.GetChild(i).ChildCount == 3)
                {
                    var pairText = _jsonStack.Pop();
                    if (pairText != null)
                    {
                        pairArray.Add(pairText);
                    }
                }
            }

            pairArray.Reverse();
            var resultArray = string.Join(",", pairArray);
            _jsonStack.Push(!string.IsNullOrEmpty(resultArray) ? "{" + $"{resultArray}" + "}" : null);
        }

        public override void ExitArray(ArrayContext context)
        {
            List<object> valueArray = new List<object>();
            for (var i = 0; i < context.ChildCount; ++i)
            {
                if (context.GetChild(i).ChildCount > 0)
                {
                    var valText = _jsonStack.Pop();
                    if (valText != null)
                    {
                        valueArray.Add(valText);
                    }
                }
            }

            valueArray.Reverse();
            var resultArray = string.Join(",", valueArray);
            _jsonStack.Push(!string.IsNullOrEmpty(resultArray) ? $"[{resultArray}]" : null);
        }

        public override void ExitPair(PairContext context)
        {
            if (context.ChildCount == 3)
            {
                var valueText = _jsonStack.Pop();
                _jsonStack.Push(valueText != null ? $"{context.GetChild(0).GetText()}:{valueText}" : null);
            }
        }

        public override void ExitValue(ValueContext context)
        {
            if (context.ChildCount == 1)
            {
                var child = context.GetChild(0);
                if (child.ChildCount == 0)
                {
                    var text = child.GetText();
                    _jsonStack.Push((text.Length == 0 || text == "\"\"") ? null : text);
                }
            }
        }

        internal object GetResult()
        {
            return _jsonStack.Pop();
        }
    }
}