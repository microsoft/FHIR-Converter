// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DotLiquid;
using DotLiquid.Exceptions;
using DotLiquid.Util;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Newtonsoft.Json;
using RenderException = Microsoft.Health.Fhir.Liquid.Converter.Exceptions.RenderException;

namespace Microsoft.Health.Fhir.Liquid.Converter.DotLiquids
{
    public class MergeDiff : Block
    {
        private static readonly Regex Syntax = R.B(@"^({0}+)\s$", DotLiquid.Liquid.VariableSignature);
        private string _variableName;
        private Dictionary<string, string> _attributes;

        private List<object> DiffBlock { get; set; }

        /// <summary>
        /// Initializes the mergeDiff tag
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
                R.Scan(markup, DotLiquid.Liquid.TagAttributes, (key, value) => _attributes[key] = value);
            }
            else
            {
                throw new SyntaxException(Resources.EvaluateTagSyntaxError);
            }

            base.Initialize(tagName, markup, tokens);
        }

        public override void Render(Context context, TextWriter result)
        {

            context.Stack(() =>
            {
                using StringWriter writer = new StringWriter();

                var inputContent = context[_variableName];

                // If input message is null, it returns empty string to makesure the output is still a valid json object.
                // And the element with empty string value will be removed in post processor.
                if (inputContent == null)
                {
                    result.Write("\"\"");
                    return;
                }

                // Input message should be a valid json object.
                if (!(inputContent is Dictionary<string, object> || inputContent is Hash))
                {
                    throw new RenderException(FhirConverterErrorCode.InvalidInputOfMergeDiffBlock, "The input message of MergeDiff tag should be a json object.");
                }

                RenderAll(DiffBlock, context, writer);

                // Diff content should be a valid json object.
                // If diff content is empty, the input message will output directly.
                Dictionary<string, object> diffDic = null;
                try
                {
                    diffDic = JsonConvert.DeserializeObject<Dictionary<string, object>>(writer.ToString());
                }
                catch
                {
                    throw new RenderException(FhirConverterErrorCode.InvalidMergeDiffBlockContent, "MergeDiff block content should be a json object.");
                }

                var mergedResult = inputContent;
                if (diffDic != null)
                {
                    mergedResult = MergeDiffContent(inputContent, diffDic);
                }

                result.Write(mergedResult == null ? null : JsonConvert.SerializeObject(mergedResult));
            });
        }

        public object MergeDiffContent(object source, Dictionary<string, object> diffDic)
        {
            Dictionary<string, object> result;

            if (source is Hash)
            {
                result = new Dictionary<string, object>(source as Hash);
            }
            else if (source is Dictionary<string, object>)
            {
                result = new Dictionary<string, object>(source as Dictionary<string, object>);
            }
            else
            {
                throw new RenderException(FhirConverterErrorCode.InvalidInputOfMergeDiffBlock, "MergeDiff block is empty.");
            }

            foreach (var item in diffDic)
            {
                if (item.Key.EndsWith("[x]"))
                {
                    var choiceTypeName = item.Key.Replace("[x]", string.Empty);
                    var choiceElements = result.Where(x => x.Key.StartsWith(choiceTypeName)).Select(x => x.Key).ToList();
                    foreach (var ele in choiceElements)
                    {
                        result[ele] = item.Value;
                    }
                }
                else
                {
                    result[item.Key] = item.Value;
                }
            }

            return result;
        }
    }
}
