// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using DotLiquid;
using DotLiquid.FileSystems;
using Microsoft.Health.Fhir.Liquid.Converter.DotLiquids;
using Microsoft.Health.Fhir.Liquid.Converter.Models;

namespace Microsoft.Health.Fhir.Liquid.Converter.Hl7v2
{
    public class Hl7v2TemplateProvider : ITemplateProvider
    {
        private readonly IFhirConverterTemplateFileSystem _fileSystem;

        public Hl7v2TemplateProvider(string templateDirectory)
        {
            _fileSystem = new TemplateLocalFileSystem(templateDirectory, DataType.Hl7v2);
        }

        public Hl7v2TemplateProvider(List<Dictionary<string, Template>> templateCollection)
        {
            _fileSystem = new MemoryFileSystem(templateCollection);
        }

        public Template GetTemplate(string templateName)
        {
            return _fileSystem.GetTemplate(templateName);
        }

        public ITemplateFileSystem GetTemplateFileSystem()
        {
            return _fileSystem;
        }
    }
}
