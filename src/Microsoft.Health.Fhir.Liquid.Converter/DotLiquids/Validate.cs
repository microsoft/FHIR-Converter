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
using Microsoft.Health.Fhir.Liquid.Converter.Models.Json;
using Microsoft.Health.Fhir.Liquid.Converter.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using RenderException = Microsoft.Health.Fhir.Liquid.Converter.Exceptions.RenderException;

namespace Microsoft.Health.Fhir.Liquid.Converter.DotLiquids
{
    public class Validate : Block
    {
        private static readonly Regex Syntax = R.B(@"^({0}+)\s$", DotLiquid.Liquid.QuotedString);

        // Ignore comments in validate block content, like "// comments... " or "/* comments... */"
        private static readonly JsonLoadSettings ContentLoadSettings = new JsonLoadSettings()
        {
            CommentHandling = CommentHandling.Ignore,
        };

        private string _schemaFileName;
        private List<object> _validateBlock;

        public override void Initialize(string tagName, string markup, List<string> tokens)
        {
            Match syntaxMatch = Syntax.Match(markup);
            if (syntaxMatch.Success)
            {
                NodeList = _validateBlock = new List<object>();
                var fullSchemaFileName = syntaxMatch.Groups[1].Value;
                _schemaFileName = fullSchemaFileName.Substring(1, fullSchemaFileName.Length - 2);
            }
            else
            {
                throw new SyntaxException(Resources.ValidateTagSyntaxError);
            }

            base.Initialize(tagName, markup, tokens);
        }

        public override void Render(Context context, TextWriter result)
        {
            JsonSchema validateSchema = LoadValidateSchema(context);

            if (context is JSchemaContext jSchemaContext)
            {
                jSchemaContext.ValidateSchemas.Add(validateSchema);
            }

            using StringWriter writer = new StringWriter();

            context.Stack(() =>
            {
                RenderAll(_validateBlock, context, writer);
            });

            // Enclose content in validate tag must be a valid JSON format
            var validateContent = writer.ToString();
            JObject validateObject;
            try
            {
                validateObject = JObject.Parse(validateContent, ContentLoadSettings);
            }
            catch (JsonException ex)
            {
                throw new RenderException(FhirConverterErrorCode.InvalidValidateBlockContent, string.Format(Resources.InvalidValidateContentBlock, ex.Message));
            }

            var errors = validateSchema.Validate(validateObject);

            if (errors.Any())
            {
                throw new RenderException(FhirConverterErrorCode.UnmatchedValidateBlockContent, string.Format(Resources.UnMatchedValidateBlockContent, _schemaFileName, string.Join(";", errors)));
            }

            result.Write(validateContent);
        }

        private JsonSchema LoadValidateSchema(Context context)
        {
            if (!(context.Registers["file_system"] is IFhirConverterTemplateFileSystem fileSystem))
            {
                throw new RenderException(FhirConverterErrorCode.NullTemplateProvider, Resources.NullTemplateProvider);
            }

            if (!(fileSystem.GetTemplate(_schemaFileName, context[TemplateUtility.RootTemplateParentPathScope]?.ToString())?.Root is JSchemaDocument jSchemaDocument))
            {
                throw new RenderException(FhirConverterErrorCode.TemplateNotFound, string.Format(Resources.TemplateNotFound, _schemaFileName));
            }

            return jSchemaDocument.Schema;
        }
    }
}
