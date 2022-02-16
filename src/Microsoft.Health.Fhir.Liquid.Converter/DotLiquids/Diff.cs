// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using DotLiquid;
using DotLiquid.Exceptions;
using DotLiquid.Util;
using Microsoft.Health.Fhir.Liquid.Converter.OutputProcessors;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Liquid.Converter.DotLiquids
{
    public class Diff : Block
    {
        private static readonly Regex Syntax = R.B(@"({0}+)\s", DotLiquid.Liquid.VariableSignature);
        private string _variableName;
        private Dictionary<string, string> _attributes;

        private List<object> DiffBlock { get; set; }

        /// <summary>
        /// Initializes the diff tag
        /// </summary>
        /// <param name="tagName">Name of the parsed tag</param>
        /// <param name="markup">Markup of the parsed tag</param>
        /// <param name="tokens">Tokens of the parsed tag</param>
        public override void Initialize(string tagName, string markup, List<string> tokens)
        {
            Match match = Syntax.Match(markup);
            if (match.Success)
            {
                NodeList = DiffBlock = new List<object>();
                _variableName = match.Groups[0].Value;
                _attributes = new Dictionary<string, string>(Template.NamingConvention.StringComparer);
                R.Scan(markup, DotLiquid.Liquid.TagAttributes,
                    (key, value) => _attributes[key] = value);
            }
            else
            {
                throw new SyntaxException("Diff Tag syntax Error.");
            }

            base.Initialize(tagName, markup, tokens);
        }

        public override void Render(Context context, TextWriter result)
        {

            context.Stack(() =>
            {
                using StringWriter writer = new StringWriter();
                if (context[_variableName] == null)
                {
                    result.Write("\"\"");
                    return;
                }

                RenderAll(DiffBlock, context, writer);

                var mergedResult = MergeDiff(JObject.Parse(JsonConvert.SerializeObject(context[_variableName])), JObject.Parse(writer.ToString()));
                var resultObj = PostProcessor.Process(JsonConvert.SerializeObject(mergedResult));
                result.Write(string.IsNullOrEmpty(resultObj.ToString()) ? null : resultObj.ToString());
            });
        }

        public JToken MergeDiff(JToken source, JToken diff, string itemKey = null)
        {
            if ((source is JArray) && (diff is JArray))
            {
                if (itemKey == "extension")
                {
                    if (!string.Equals(source.ToString(), diff.ToString()))
                    {
                        foreach (var item in diff)
                        {
                            (source as JArray).Add(item);
                        }

                        return source;
                    }
                }
            }

            if ((source is JObject) && (diff is JObject))
            {
                foreach (var item in diff as JObject)
                {
                    if ((source as JObject).ContainsKey(item.Key))
                    {
                        (source as JObject)[item.Key] = MergeDiff((source as JObject).GetValue(item.Key), item.Value, item.Key);
                    }
                    else
                    {
                        (source as JObject).Add(item.Key, item.Value);
                    }
                }

                var result = source as JObject;
                foreach (var item in result)
                {
                    if (item.Value == null)
                    {
                        (source as JObject).Remove(item.Key);
                    }
                }

                return source;
             }
            else
            {
                return diff;
            }
        }
    }
}
