// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using EnsureThat;
using Fluid;

namespace Microsoft.Health.Fhir.Liquid.Converter.Fluid
{
    public class SimpleFileSystemTemplateProvider : ITemplateProvider<IFluidTemplate>
    {
        private static readonly FluidParser Parser = FluidParserFactory.CreateParser();

        private readonly DirectoryInfo _templateRootDirectory;
        private readonly ReadOnlyDictionary<string, IFluidTemplate> _templates;

        public SimpleFileSystemTemplateProvider(DirectoryInfo templateRootDirectory, bool strictMode = true)
        {
            _templateRootDirectory = EnsureArg.IsNotNull(templateRootDirectory);
            _templates = LoadTemplates(_templateRootDirectory, strictMode);
        }

        public IFluidTemplate GetTemplate(string templateName)
        {
            return _templates.TryGetValue(templateName, out var template) ? template : throw new KeyNotFoundException($"Template {templateName} not found in {_templateRootDirectory.FullName}");
        }

        private static IFluidTemplate ParseTemplate(string templateContent)
        {
            return Parser.Parse(templateContent);
        }

        private static ReadOnlyDictionary<string, IFluidTemplate> LoadTemplates(DirectoryInfo rootDirectory, bool isStrictModeEnabled)
        {
            Dictionary<string, IFluidTemplate> templates = new ();
            int errors = 0;

            foreach (var file in rootDirectory.EnumerateFiles("*.liquid", SearchOption.AllDirectories))
            {
                var templateName = file.FullName.Substring(rootDirectory.FullName.Length + 1)
                    .Replace(@"\_", @"\")
                    .Replace(@".liquid", string.Empty);
                var templateContent = File.ReadAllText(file.FullName, Encoding.UTF8);

                try
                {
                    templates[templateName] = ParseTemplate(templateContent);
                }
                catch (Exception e) when (!isStrictModeEnabled)
                {
                    errors++;
                    Trace.WriteLine($"Failed to parse template {templateName}: {e.Message}");
                }
            }

            if (errors > 0)
            {
                Trace.WriteLine($"Failed to parse {errors} templates in {rootDirectory.FullName}");
            }

            return new ReadOnlyDictionary<string, IFluidTemplate>(templates);
        }
    }
}
