// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotLiquid;
using DotLiquid.FileSystems;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Utilities;

namespace Microsoft.Health.Fhir.Liquid.Converter.DotLiquids
{
    public class MemoryFileSystem : ITemplateFileSystem
    {
        protected const string TemplateFileExtension = ".liquid";

        protected string TemplateDirectory { get; set; }

        protected List<Dictionary<string, Template>> TemplateCollection { get; set; }

        public string ReadTemplateFile(Context context, string templateName)
        {
            throw new NotImplementedException();
        }

        public Template GetTemplate(Context context, string templateName)
        {
            var templatePath = (string)context[templateName];
            if (templatePath == null)
            {
                throw new RenderException(FhirConverterErrorCode.TemplateNotFound, string.Format(Resources.TemplateNotFound, templatePath));
            }

            return GetTemplate(templatePath) ?? throw new RenderException(FhirConverterErrorCode.TemplateNotFound, string.Format(Resources.TemplateNotFound, templatePath));
        }

        public Template GetTemplate(string templateName)
        {
            if (string.IsNullOrEmpty(templateName))
            {
                return null;
            }

            // Get template from TemplateCollection first
            foreach (var templates in TemplateCollection)
            {
                if (templates != null && templates.ContainsKey(templateName))
                {
                    return templates[templateName];
                }
            }

            // If template not found but loaded from local file system, search local file system
            if (TemplateDirectory != null)
            {
                if (TemplateCollection == null)
                {
                    TemplateCollection = new List<Dictionary<string, Template>>();
                }

                if (TemplateCollection.FirstOrDefault() == null)
                {
                    TemplateCollection.Add(new Dictionary<string, Template>());
                }

                var templatePath = GetAbsoluteTemplatePath(templateName);
                var templateContent = LoadTemplate(templatePath);
                var template = TemplateUtility.ParseTemplate(templateName, templateContent);
                TemplateCollection[0][templateName] = template;
                return template;
            }

            return null;
        }

        protected string LoadTemplate(string templatePath)
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
            var result = TemplateDirectory;
            var pathSegments = templateName.Split(Path.AltDirectorySeparatorChar);

            // Root templates
            if (pathSegments.Length == 1)
            {
                return Path.Join(TemplateDirectory, $"{pathSegments[0]}{TemplateFileExtension}");
            }

            // Snippets
            pathSegments[^1] = $"_{pathSegments[^1]}{TemplateFileExtension}";
            foreach (var pathSegment in pathSegments)
            {
                result = Path.Join(result, pathSegment);
            }

            return result;
        }
    }
}
