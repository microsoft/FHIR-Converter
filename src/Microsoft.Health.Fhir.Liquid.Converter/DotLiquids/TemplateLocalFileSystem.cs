// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Utilities;

namespace Microsoft.Health.Fhir.Liquid.Converter.DotLiquids
{
    public class TemplateLocalFileSystem : IFhirConverterTemplateFileSystem
    {
        private readonly string _templateDirectory;

        private Dictionary<string, Template> _templateCache;

        public TemplateLocalFileSystem(string templateDirectory, DataType dataType)
        {
            if (!Directory.Exists(templateDirectory))
            {
                throw new TemplateLoadException(FhirConverterErrorCode.TemplateFolderNotFound, string.Format(Resources.TemplateFolderNotFound, templateDirectory));
            }

            _templateDirectory = templateDirectory;
            _templateCache = new Dictionary<string, Template>();
        }

        public string ReadTemplateFile(Context context, string templateName)
        {
            throw new NotImplementedException();
        }

        public Template GetTemplate(Context context, string templateName)
        {
            var templateKey = (string)context[templateName];
            if (templateKey == null)
            {
                throw new RenderException(FhirConverterErrorCode.TemplateNotFound, string.Format(Resources.TemplateNotFound, templateName));
            }

            return GetTemplate(templateKey) ?? throw new RenderException(FhirConverterErrorCode.TemplateNotFound, string.Format(Resources.TemplateNotFound, templateKey));
        }

        public Template GetTemplate(string templateKey)
        {
            if (string.IsNullOrEmpty(templateKey))
            {
                return null;
            }

            // Get template from cache first
            if (_templateCache.ContainsKey(templateKey))
            {
                return _templateCache[templateKey];
            }

            // If not cached, search local file system
            var templateContent = ReadTemplateFile(templateKey);

            Template template = TemplateUtility.ParseTemplate(templateKey, templateContent);
            _templateCache[templateKey] = template;

            return template;
        }

        private string ReadTemplateFile(string templateKey)
        {
            try
            {
                var templatePath = GetAbsoluteTemplatePath(templateKey);
                return File.Exists(templatePath) ? File.ReadAllText(templatePath) : null;
            }
            catch (Exception ex)
            {
                throw new TemplateLoadException(FhirConverterErrorCode.TemplateLoadingError, string.Format(Resources.TemplateLoadingError, ex.Message), ex);
            }
        }

        private string GetAbsoluteTemplatePath(string templateName)
        {
            // 1. Json schema template, keep as is. E.g. "schema/Patient.schema.json"
            // 2. Liquid template in root directory, append ".liquid" suffix. E.g. "CCD" -> "CCD.liquid"
            // 3. Liquid template in sub directory, append "_" prefix and ".liquid" suffix. E.g. "Resource/Encounter" -> "Resource/_Encounter.liquid"
            // 4. Code mapping template, append ".json" suffix. E.g. "ValueSet/ValueSet" -> "valueSet/ValueSet.json"

            var result = _templateDirectory;
            var pathSegments = templateName.Split(Path.AltDirectorySeparatorChar);

            if (!TemplateUtility.IsJsonSchemaTemplate(templateName))
            {
                if (pathSegments.Length == 1)
                {
                    // Root template
                    pathSegments[0] = $"{pathSegments[0]}.liquid";
                }
                else
                {
                    // Snippets
                    pathSegments[^1] = TemplateUtility.IsCodeMappingTemplate(templateName) ? $"{pathSegments[^1]}.json" : $"_{pathSegments[^1]}.liquid";
                }
            }

            return pathSegments.Aggregate(result, Path.Join);
        }
    }
}
