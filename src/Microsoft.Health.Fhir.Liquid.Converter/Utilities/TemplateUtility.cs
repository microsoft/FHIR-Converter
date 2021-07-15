// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using DotLiquid;
using DotLiquid.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.DotLiquids;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Liquid.Converter.Utilities
{
    public static class TemplateUtility
    {
        private static readonly Regex FormatRegex = new Regex(@"(\\|/)_?");
        private const string TemplateFileExtension = ".liquid";

        // Register "evaluate" tag in before Template.Parse
        static TemplateUtility()
        {
            Template.RegisterTag<Evaluate>("evaluate");
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
                if (string.Equals(formattedEntryKey, "CodeSystem/CodeSystem.json", StringComparison.InvariantCultureIgnoreCase))
                {
                    parsedTemplates["CodeSystem/CodeSystem"] = ParseCodeMapping(entry.Value);
                }
                else if (string.Equals(formattedEntryKey, "ValueSet/ValueSet.json", StringComparison.InvariantCultureIgnoreCase))
                {
                    parsedTemplates["ValueSet/ValueSet"] = ParseCodeMapping(entry.Value);
                }
                else if (string.Equals(Path.GetExtension(formattedEntryKey), TemplateFileExtension, StringComparison.InvariantCultureIgnoreCase))
                {
                    var templateName = formattedEntryKey.Substring(0, formattedEntryKey.LastIndexOf(TemplateFileExtension));
                    parsedTemplates[templateName] = ParseTemplate(templateName, entry.Value);
                }
            }

            return parsedTemplates;
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

        public static Template ParseTemplate(string templateName, string content)
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
    }
}
