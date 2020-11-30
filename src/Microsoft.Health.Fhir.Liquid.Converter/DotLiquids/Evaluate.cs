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
using DotLiquid.FileSystems;
using DotLiquid.Util;

namespace Microsoft.Health.Fhir.Liquid.Converter.DotLiquids
{
    public class Evaluate : Tag
    {
        private static readonly Regex Syntax = R.B(@"({0}+)\s+(?:using)\s+({1}+)", DotLiquid.Liquid.VariableSignature, DotLiquid.Liquid.QuotedString);

        private string _to;
        private string _templateName;
        private Dictionary<string, string> _attributes;

        public override void Initialize(string tagName, string markup, List<string> tokens)
        {
            Match syntaxMatch = Syntax.Match(markup);
            if (syntaxMatch.Success)
            {
                _to = syntaxMatch.Groups[1].Value;
                _templateName = syntaxMatch.Groups[2].Value;
                _attributes = new Dictionary<string, string>(Template.NamingConvention.StringComparer);
                R.Scan(markup, DotLiquid.Liquid.TagAttributes, (key, value) => _attributes[key] = value);
            }
            else
            {
                throw new SyntaxException(Resources.EvaluateTagSyntaxError);
            }

            base.Initialize(tagName, markup, tokens);
        }

        protected override void Parse(List<string> tokens)
        {
        }

        public override void Render(Context context, TextWriter result)
        {
            var templateFileSystem = context.Registers["file_system"] as ITemplateFileSystem;
            if (templateFileSystem == null)
            {
                throw new FileSystemException(Resources.TemplateFileSystemNotFound);
            }

            var partial = templateFileSystem.GetTemplate(context, _templateName);

            context.Stack(() =>
            {
                foreach (var keyValue in _attributes)
                {
                    context[keyValue.Key] = context[keyValue.Value];
                }

                var content = partial.Render(RenderParameters.FromContext(context, result.FormatProvider)).Trim();
                context.Scopes.Last()[_to] = string.IsNullOrEmpty(content) ? null : content;
            });
        }
    }
}
