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
        private const string _choiceTypeSuffix = "[x]";

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

            // Initialize block content into variable 'Nodelist'
            // Variable '_diffBlock' will also be initialized since they refer to the same object.
            base.Initialize(tagName, markup, tokens);
        }

        public override void Render(Context context, TextWriter result)
        {
            using StringWriter writer = new StringWriter();

            context.Stack(() =>
            {
                RenderAll(_diffBlock, context, writer);
            });

            var inputContent = context[_variableName];

            // If input message is null, it returns empty string to make sure the output is still a valid json object.
            // And the element with empty string value will be removed in post processor.
            if (inputContent == null)
            {
                result.Write("\"\"");
                return;
            }

            // Diff content should be a valid json object.
            // If diff content is empty, the input message will output directly.
            Dictionary<string, object> diffDict = null;

            try
            {
                diffDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(writer.ToString());
            }
            catch (Exception ex)
            {
                throw new RenderException(FhirConverterErrorCode.InvalidMergeDiffBlockContent, Resources.InvalidMergeDiffBlockContent, ex);
            }

            var mergedResult = inputContent;

            // diffDict is null when diff content is empty.
            if (diffDict?.Count > 0)
            {
                mergedResult = MergeDiffContent(inputContent, diffDict);
            }

            result.Write(JsonConvert.SerializeObject(mergedResult));
        }

        private Dictionary<string, object> MergeDiffContent(object source, Dictionary<string, object> diffDict)
        {
            Dictionary<string, object> result;

            // Input message should be a valid json object.
            if (source is Hash hashSource)
            {
                result = new Dictionary<string, object>(hashSource);
            }
            else if (source is IDictionary<string, object> dictSource)
            {
                result = new Dictionary<string, object>(dictSource);
            }
            else
            {
                throw new RenderException(FhirConverterErrorCode.InvalidInputOfMergeDiffBlock, Resources.InvalidInputOfMergeDiffBlock);
            }

            foreach (var item in diffDict)
            {
                // [x] represent choice type elements.
                // Only handle choice type element for the first match.
                if (item.Key.EndsWith(_choiceTypeSuffix))
                {
                    var choiceTypeName = item.Key.Remove(item.Key.Length - _choiceTypeSuffix.Length);
                    if (string.IsNullOrEmpty(choiceTypeName))
                    {
                        throw new RenderException(FhirConverterErrorCode.InvalidMergeDiffBlockContent, Resources.InvalidMergeDiffBlockContentForChoiceType);
                    }

                    var choiceElement = result.Where(x => x.Key.StartsWith(choiceTypeName));
                    if (choiceElement.Any())
                    {
                        result[choiceElement.First().Key] = item.Value;
                    }
                }
                else
                {
                    result[item.Key] = item.Value ?? string.Empty;
                }
            }

            return result;
        }
    }
}
