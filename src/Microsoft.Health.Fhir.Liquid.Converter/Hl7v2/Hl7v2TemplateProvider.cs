// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

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
        public Hl7v2TemplateProvider(string templateDirectory)
        {
            if (!Directory.Exists(templateDirectory))
            {
                throw new ConverterInitializeException(FhirConverterErrorCode.TemplateFolderNotFound, string.Format(Resources.TemplateFolderNotFound, templateDirectory));
            }

            TemplateDirectory = templateDirectory;
            TemplateCollection = LoadCodeSystemMapping();
        }

        public Hl7v2TemplateProvider(List<Dictionary<string, Template>> templateCollection)
        {
            TemplateDirectory = null;
            TemplateCollection = new List<Dictionary<string, Template>>();
            foreach (var templates in templateCollection)
            {
                TemplateCollection.Add(new Dictionary<string, Template>(templates));
            }
        }

        public List<Dictionary<string, Template>> LoadCodeSystemMapping()
        {
            var templates = new Dictionary<string, Template>();
            var codeSystemMappingPath = Path.Join(TemplateDirectory, "CodeSystem", "CodeSystem.json");
            if (File.Exists(codeSystemMappingPath))
            {
                var content = LoadTemplate(codeSystemMappingPath);
                templates["CodeSystem/CodeSystem"] = TemplateUtility.ParseCodeSystemMapping(content);
            }

            return new List<Dictionary<string, Template>>() { templates };
        }
    }
}
