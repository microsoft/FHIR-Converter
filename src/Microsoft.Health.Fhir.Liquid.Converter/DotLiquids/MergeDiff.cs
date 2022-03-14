// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
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

        private List<object> _diffBlock;

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
                NodeList = _diffBlock = new List<object>();
                _variableName = match.Groups[0].Value;
            }
            else
            {
                throw new SyntaxException(Resources.MergeDiffTagSyntaxError);
            }

            base.Initialize(tagName, markup, tokens);
        }

        public override void Render(Context context, TextWriter result)
        {
            context.Stack(() =>
            {
                using StringWriter writer = new StringWriter();

                var inputContent = context[_variableName];

                // If input message is null, it returns empty string to make sure the output is still a valid json object.
                // And the element with empty string value will be removed in post processor.
                if (inputContent == null)
                {
                    result.Write("\"\"");
                    return;
                }

                // Input message should be a valid json object.
                if (!(inputContent is Dictionary<string, object> || inputContent is Hash))
                {
                    throw new RenderException(FhirConverterErrorCode.InvalidInputOfMergeDiffBlock, "The input message of 'MergeDiff' tag should be a json object.");
                }

                RenderAll(_diffBlock, context, writer);

                // Diff content should be a valid json object.
                // If diff content is empty, the input message will output directly.
                Dictionary<string, object> diffDict = null;
                try
                {
                    diffDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(writer.ToString());
                }
                catch (Exception ex)
                {
                    throw new RenderException(FhirConverterErrorCode.InvalidMergeDiffBlockContent, "Content in 'MergeDiff' block should be a json object.", ex);
                }

                var mergedResult = inputContent;
                if (diffDict != null)
                {
                    mergedResult = MergeDiffContent(inputContent, diffDict);
                }

                result.Write(JsonConvert.SerializeObject(mergedResult));
            });
        }

        private object MergeDiffContent(object source, Dictionary<string, object> diffDict)
        {
            Dictionary<string, object> result;

            if (source is Hash hashSource)
            {
                result = new Dictionary<string, object>(hashSource);
            }
            else if (source is Dictionary<string, object> dictSource)
            {
                result = new Dictionary<string, object>(dictSource);
            }
            else
            {
                throw new RenderException(FhirConverterErrorCode.InvalidInputOfMergeDiffBlock, "'MergeDiff' block is empty.");
            }

            foreach (var item in diffDict)
            {
                // [x] represent choice type elements.
                if (item.Key.EndsWith("[x]"))
                {
                    var choiceTypeName = item.Key.Remove(item.Key.Length - 3);
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
