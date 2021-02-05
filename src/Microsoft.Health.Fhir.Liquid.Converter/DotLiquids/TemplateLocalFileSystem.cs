﻿// -------------------------------------------------------------------------------------------------
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
                throw new ConverterInitializeException(FhirConverterErrorCode.TemplateFolderNotFound, string.Format(Resources.TemplateFolderNotFound, templateDirectory));
            }

            _templateDirectory = templateDirectory;
            _templateCache = new Dictionary<string, Template>();

            LoadMappingTemplate(dataType);
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
            var templatePath = GetAbsoluteTemplatePath(templateName);
            if (File.Exists(templatePath))
            {
                var templateContent = LoadTemplate(templatePath);
                var template = TemplateUtility.ParseTemplate(templateName, templateContent);
                _templateCache[templateName] = template;
                return template;
            }

            return null;
        }

        private void LoadMappingTemplate(DataType dataType)
        {
            var mappingFileType = dataType == DataType.Hl7v2 ? "CodeSystem" : "ValueSet";
            var codeSystemMappingPath = Path.Join(_templateDirectory, mappingFileType, $"{mappingFileType}.json");
            if (File.Exists(codeSystemMappingPath))
            {
                var content = LoadTemplate(codeSystemMappingPath);
                var template = TemplateUtility.ParseCodeSystemMapping(content);
                _templateCache[$"{mappingFileType}/{mappingFileType}"] = template;
            }
        }

        private string LoadTemplate(string templatePath)
        {
            try
            {
                return File.ReadAllText(templatePath);
            }
            catch (Exception ex)
            {
                throw new ConverterInitializeException(FhirConverterErrorCode.TemplateLoadingError, string.Format(Resources.TemplateLoadingError, ex.Message), ex);
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
            pathSegments[^1] = $"_{pathSegments[^1]}.liquid";

            return pathSegments.Aggregate(result, Path.Join);
        }
    }
}
