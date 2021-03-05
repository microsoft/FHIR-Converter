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

        public static Dictionary<string, Template> ParseTemplates(Dictionary<string, string> templates)
        {
            var parsedTemplates = new Dictionary<string, Template>();
            foreach (var entry in templates)
            {
                var formattedEntryKey = FormatRegex.Replace(entry.Key, "/");
                if (string.Equals(formattedEntryKey, "CodeSystem/CodeSystem.json", StringComparison.InvariantCultureIgnoreCase))
                {
                    parsedTemplates["CodeSystem/CodeSystem"] = ParseCodeSystemMapping(entry.Value);
                }
                else if (string.Equals(formattedEntryKey, "ValueSet/ValueSet.json", StringComparison.InvariantCultureIgnoreCase))
                {
                    parsedTemplates["ValueSet/ValueSet"] = ParseCodeSystemMapping(entry.Value);
                }
                else if (string.Equals(Path.GetExtension(formattedEntryKey), TemplateFileExtension, StringComparison.InvariantCultureIgnoreCase))
                {
                    var templateName = formattedEntryKey.Substring(0, formattedEntryKey.LastIndexOf(TemplateFileExtension));
                    parsedTemplates[templateName] = ParseTemplate(templateName, entry.Value);
                }
            }

            return parsedTemplates;
        }

        public static Template ParseCodeSystemMapping(string content)
        {
            if (content == null)
            {
                return null;
            }

            try
            {
                var mapping = JsonConvert.DeserializeObject<CodeSystemMapping>(content);
                if (mapping == null || mapping.Mapping == null)
                {
                    throw new ConverterInitializeException(FhirConverterErrorCode.InvalidCodeSystemMapping, Resources.InvalidCodeSystemMapping);
                }

                var template = Template.Parse(string.Empty);
                template.Root = new CodeSystemMappingDocument(new List<CodeSystemMapping>() { mapping });
                return template;
            }
            catch (JsonException ex)
            {
                throw new ConverterInitializeException(FhirConverterErrorCode.InvalidCodeSystemMapping, Resources.InvalidCodeSystemMapping, ex);
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
                throw new ConverterInitializeException(FhirConverterErrorCode.TemplateSyntaxError, string.Format(Resources.TemplateSyntaxError, templateName, ex.Message), ex);
            }
        }
    }
}
