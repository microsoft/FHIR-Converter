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
            var templatePath = (string)context[templateName];
            if (templatePath == null)
            {
                throw new RenderException(FhirConverterErrorCode.TemplateNotFound, string.Format(Resources.TemplateNotFound, templateName));
            }

            return GetTemplate(templatePath) ?? throw new RenderException(FhirConverterErrorCode.TemplateNotFound, string.Format(Resources.TemplateNotFound, templatePath));
        }

        public Template GetTemplate(string templateName)
        {
            if (string.IsNullOrEmpty(templateName))
            {
                return null;
            }

            // Get template from cache first
            if (_templateCache.ContainsKey(templateName))
            {
                return _templateCache[templateName];
            }

            // If not cached, search local file system
            var templateContent = ReadTemplateFile(templateName);
            var template = IsCodeMappingTemplate(templateName) ? TemplateUtility.ParseCodeMapping(templateContent) : TemplateUtility.ParseTemplate(templateName, templateContent);
            var key = IsCodeMappingTemplate(templateName) ? $"{templateName}/{templateName}" : templateName;

            if (template != null)
            {
                _templateCache[key] = template;
            }

            return template;
        }

        private string ReadTemplateFile(string templateName)
        {
            try
            {
                var templatePath = GetAbsoluteTemplatePath(templateName);
                return File.Exists(templatePath) ? File.ReadAllText(templatePath) : null;
            }
            catch (Exception ex)
            {
                throw new TemplateLoadException(FhirConverterErrorCode.TemplateLoadingError, string.Format(Resources.TemplateLoadingError, ex.Message), ex);
            }
        }

        private string GetAbsoluteTemplatePath(string templateName)
        {
            var result = _templateDirectory;
            var pathSegments = templateName.Split(Path.AltDirectorySeparatorChar);

            // Root templates
            if (pathSegments.Length == 1)
            {
                return Path.Join(_templateDirectory, $"{pathSegments[0]}.liquid");
            }

            // Snippets
            pathSegments[^1] = IsCodeMappingTemplate(templateName) ? $"{pathSegments[^1]}.json" : $"_{pathSegments[^1]}.liquid";

            return pathSegments.Aggregate(result, Path.Join);
        }

        private static bool IsCodeMappingTemplate(string templateName)
        {
            return string.Equals("CodeSystem/CodeSystem", templateName, StringComparison.InvariantCultureIgnoreCase) ||
                   string.Equals("ValueSet/ValueSet", templateName, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
