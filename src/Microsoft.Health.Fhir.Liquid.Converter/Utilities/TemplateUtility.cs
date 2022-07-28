// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using DotLiquid;
using DotLiquid.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.DotLiquids;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Models.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using NJsonSchema.Validation;

namespace Microsoft.Health.Fhir.Liquid.Converter.Utilities
{
    public static class TemplateUtility
    {
        private static readonly Regex FormatRegex = new Regex(@"(\\|/)_?");
        private const string LiquidTemplateFileExtension = ".liquid";
        private const string JsonSchemaTemplateFileExtension = ".schema.json";
        private const string MetaJsonSchemaFileName = "meta-schema.json";
        private static readonly JsonSchema MetaJsonSchema;

        // Register "evaluate" tag in before Template.Parse
        static TemplateUtility()
        {
            Template.RegisterTag<Evaluate>("evaluate");
            Template.RegisterTag<MergeDiff>("mergeDiff");
            Template.RegisterTag<Validate>("validate");
            MetaJsonSchema = LoadEmbeddedMetaJsonSchema();
        }

        /// <summary>
        /// Parse templates from string, "CodeSystem/CodeSystem.json" and "ValueSet/ValueSet.json" are used for Hl7v2 and C-CDA data type code mapping respectively
        /// </summary>
        /// <param name="templates">A dictionary, key is the name, value is the template content in string format</param>
        /// <returns>A dictionary, key is the name, value is Template</returns>
        public static Dictionary<string, Template> ParseTemplates(Dictionary<string, string> templates)
        {
            var parsedTemplates = new Dictionary<string, Template>();
            foreach (var entry in templates)
            {
                var formattedEntryKey = FormatRegex.Replace(entry.Key, "/");

                string templateKey = GetTemplateKey(formattedEntryKey);

                if (templateKey != null)
                {
                    parsedTemplates[templateKey] = ParseTemplate(templateKey, entry.Value);
                }
            }

            return parsedTemplates;
        }

        /// <summary>
        /// Get template key from template file path.
        /// Liquid template keys and code mapping template keys have no suffix extension, like "CodeSystem/CodeSystem", "ValueSet/ValueSet".
        /// Json schema template keys have the suffix ".schema.json".
        /// Will return null if extension of given template file is not supported.
        /// </summary>
        /// <param name="templatePath">A template file path</param>
        /// <returns>A template key</returns>
        public static string GetTemplateKey(string templatePath)
        {
            if (string.Equals(templatePath, "CodeSystem/CodeSystem.json", StringComparison.InvariantCultureIgnoreCase)
                || string.Equals(templatePath, "ValueSet/ValueSet.json", StringComparison.InvariantCultureIgnoreCase)
                || string.Equals(Path.GetExtension(templatePath), LiquidTemplateFileExtension, StringComparison.InvariantCultureIgnoreCase))
            {
                return Path.ChangeExtension(templatePath, null);
            }
            else if (IsJsonSchemaTemplate(templatePath))
            {
                return templatePath;
            }
            else
            {
                return null;
            }
        }

        public static Template ParseTemplate(string templateKey, string content)
        {
            if (IsCodeMappingTemplate(templateKey))
            {
                return ParseCodeMapping(content);
            }
            else if (IsJsonSchemaTemplate(templateKey))
            {
                return ParseJsonSchemaTemplate(content);
            }
            else
            {
                return ParseLiquidTemplate(templateKey, content);
            }
        }

        public static Template ParseCodeMapping(string content)
        {
            if (content == null)
            {
                return null;
            }

            try
            {
                var mapping = JsonConvert.DeserializeObject<CodeMapping>(content);
                if (mapping?.Mapping == null)
                {
                    throw new TemplateLoadException(FhirConverterErrorCode.InvalidCodeMapping, Resources.InvalidCodeMapping);
                }

                var template = Template.Parse(string.Empty);
                template.Root = new CodeMappingDocument(new List<CodeMapping>() { mapping });
                return template;
            }
            catch (JsonException ex)
            {
                throw new TemplateLoadException(FhirConverterErrorCode.InvalidCodeMapping, Resources.InvalidCodeMapping, ex);
            }
        }

        public static Template ParseLiquidTemplate(string templateName, string content)
        {
            if (content == null)
            {
                return null;
            }

            try
            {
                return Template.Parse(content);
            }
            catch (SyntaxException ex)
            {
                throw new TemplateLoadException(FhirConverterErrorCode.TemplateSyntaxError, string.Format(Resources.TemplateSyntaxError, templateName, ex.Message), ex);
            }
        }

        public static Template ParseJsonSchemaTemplate(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new TemplateLoadException(FhirConverterErrorCode.InvalidJsonSchema, "Schema cannot be null or empty.");
            }

            // Validate input Json schema
            ICollection<ValidationError> errors;
            try
            {
                var schemaObject = JObject.Parse(content);
                errors = MetaJsonSchema.Validate(content);
            }
            catch (Exception ex)
            {
                throw new TemplateLoadException(FhirConverterErrorCode.InvalidJsonSchema, string.Format(Resources.InvalidJsonSchemaContent, ex.Message), ex);
            }

            if (errors.Any())
            {
                throw new TemplateLoadException(FhirConverterErrorCode.InvalidJsonSchema, string.Format(Resources.InvalidJsonSchemaContent, string.Join(";", errors)));
            }

            JsonSchema schema;
            try
            {
                schema = JsonSchema.FromJsonAsync(content).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw new TemplateLoadException(FhirConverterErrorCode.InvalidJsonSchema, string.Format(Resources.InvalidJsonSchemaContent, ex.Message), ex);
            }

            var template = Template.Parse(string.Empty);
            template.Root = new JSchemaDocument(schema);
            return template;
        }

        public static bool IsCodeMappingTemplate(string templateKey)
        {
            return string.Equals("CodeSystem/CodeSystem", templateKey, StringComparison.InvariantCultureIgnoreCase) ||
                   string.Equals("ValueSet/ValueSet", templateKey, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsJsonSchemaTemplate(string templateKey)
        {
            return templateKey.EndsWith(JsonSchemaTemplateFileExtension, StringComparison.InvariantCultureIgnoreCase);
        }

        private static JsonSchema LoadEmbeddedMetaJsonSchema()
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            var metaSchemaAssemblyName = string.Format("{0}.{1}", executingAssembly.GetName().Name, MetaJsonSchemaFileName);

            string metaSchemaContent;
            using (Stream stream = executingAssembly.GetManifestResourceStream(metaSchemaAssemblyName))
            using (StreamReader reader = new StreamReader(stream))
            {
                metaSchemaContent = reader.ReadToEnd();
            }

            return JsonSchema.FromJsonAsync(metaSchemaContent).GetAwaiter().GetResult();
        }
    }
}
