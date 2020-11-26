// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.DotLiquids;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Utilities;

namespace Microsoft.Health.Fhir.Liquid.Converter.Hl7v2
{
    public class Hl7v2TemplateProvider : MemoryFileSystem, ITemplateProvider
    {
        private const string TemplateFileExtension = ".liquid";

        // Register "evaluate" tag in static constructor
        static Hl7v2TemplateProvider()
        {
            Template.RegisterTag<Evaluate>("evaluate");
        }

        public Hl7v2TemplateProvider(string templateDirectory)
        {
            TemplateCollection = LoadTemplates(templateDirectory);
        }

        public Hl7v2TemplateProvider(List<Dictionary<string, Template>> templateCollection)
        {
            TemplateCollection = templateCollection;
        }

        public List<Dictionary<string, Template>> LoadTemplates(string templateDirectory)
        {
            if (!Directory.Exists(templateDirectory))
            {
                throw new ConverterInitializeException(FhirConverterErrorCode.TemplateFolderNotFound, string.Format(Resources.TemplateFolderNotFound, templateDirectory));
            }

            try
            {
                // Load DotLiquid templates
                var templates = new Dictionary<string, string>();
                var templatePaths = Directory.EnumerateFiles(templateDirectory, "*" + TemplateFileExtension, SearchOption.AllDirectories);
                foreach (var templatePath in templatePaths)
                {
                    var name = Path.GetRelativePath(templateDirectory, templatePath);
                    templates[name] = File.ReadAllText(templatePath);
                }

                // Load code system mapping
                var codeSystemMappingPath = Path.Join(templateDirectory, "CodeSystem", "CodeSystem.json");
                if (File.Exists(codeSystemMappingPath))
                {
                    var name = Path.GetRelativePath(templateDirectory, codeSystemMappingPath);
                    templates[name] = File.ReadAllText(codeSystemMappingPath);
                }

                // Parse templates
                var parsedTemplates = TemplateUtility.ParseHl7v2Templates(templates);
                return new List<Dictionary<string, Template>>() { parsedTemplates };
            }
            catch (Exception ex)
            {
                throw new ConverterInitializeException(FhirConverterErrorCode.TemplateLoadingError, string.Format(Resources.TemplateLoadingError, ex.Message), ex);
            }
        }
    }
}
