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
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Models.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using RenderException = Microsoft.Health.Fhir.Liquid.Converter.Exceptions.RenderException;

namespace Microsoft.Health.Fhir.Liquid.Converter.DotLiquids
{
    public class Validate : Block
    {
        private static readonly Regex Syntax = R.B(@"({0}+)\s$", DotLiquid.Liquid.QuotedFragment);

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
            JSchema validateSchema = LoadValidateSchema(context);

            if (context is JsonContext jsonContext)
            {
                jsonContext.ValidateSchemas.Add(validateSchema);
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
                validateObject = JObject.Parse(validateContent);
            }
            catch (JsonException ex)
            {
                throw new RenderException(FhirConverterErrorCode.TemplateRenderingError, "Validate content should be JSON format: " + ex.Message);
            }

            if (!validateObject.IsValid(validateSchema, out IList<string> messages))
            {
                throw new RenderException(FhirConverterErrorCode.InvalidValidateBlockContent, string.Join(";", messages));
            }

            result.Write(validateContent);
        }

        private JSchema LoadValidateSchema(Context context)
        {
            IFhirConverterTemplateFileSystem fileSystem = context.Registers["file_system"] as IFhirConverterTemplateFileSystem;

            if (fileSystem == null)
            {
                throw new RenderException(FhirConverterErrorCode.NullTemplateProvider, Resources.NullTemplateProvider);
            }

            JsonContentDocument jSchemaDocument = fileSystem.GetTemplate(_schemaFileName)?.Root as JsonContentDocument;
            if (jSchemaDocument == null || jSchemaDocument.NodeList.Count == 0)
            {
                throw new RenderException(FhirConverterErrorCode.TemplateNotFound, string.Format(Resources.TemplateNotFound, _schemaFileName));
            }

            JObject schemaObject = jSchemaDocument.NodeList[0] as JObject;
            try
            {
                return JSchema.Parse(schemaObject.ToString());
            }
            catch (JsonException ex)
            {
                throw new RenderException(FhirConverterErrorCode.InvalidValidateSchema, Resources.TemplateRenderingError, ex);
            }
        }
    }
}
