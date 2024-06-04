// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DotLiquid;
using EnsureThat;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.TemplateManagement.Configurations;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Microsoft.Health.Fhir.TemplateManagement.Utilities;

namespace Microsoft.Health.Fhir.TemplateManagement.ArtifactProviders
{
    /// <summary>
    /// Template collection provider that provides all the default templates (under data/Templates) packaged as embedded resources,
    /// as a template collection to be used by the converter.
    /// Templates are added to the collection under the respective 'DataType' path, i.e., each template under data/Templates/Hl7v2/ will be added as Hl7v2/*templateName*.liquid
    /// </summary>
    public class DefaultTemplateCollectionProvider : IConvertDataTemplateCollectionProvider
    {
        private readonly string _resourcePathFormat = "{0}.{1}";

        private readonly IMemoryCache _templateCollectionCache;
        private readonly TemplateCollectionConfiguration _templateCollectionConfiguration;
        private readonly string _defaultTemplateCacheKey = "cached-default-templates";

        public DefaultTemplateCollectionProvider(IMemoryCache templateCache, TemplateCollectionConfiguration templateConfiguration)
        {
            _templateCollectionCache = EnsureArg.IsNotNull(templateCache, nameof(templateCache));
            _templateCollectionConfiguration = EnsureArg.IsNotNull(templateConfiguration, nameof(templateConfiguration));
        }

        public async Task<List<Dictionary<string, Template>>> GetTemplateCollectionAsync(CancellationToken cancellationToken)
        {
            // Read templates from cache if available
            if (_templateCollectionCache.TryGetValue(_defaultTemplateCacheKey, out List<Dictionary<string, Template>> templateCollectionCache))
            {
                return templateCollectionCache;
            }

            var templates = new List<Dictionary<string, Template>>();

            // Extract default templates from embeeded resources
            var hl7v2DefaultTemplatesTask = Task.Run(() => templates.Add(ExtractTemplatesFromResource(DefaultTemplateInfo.Hl7v2DefaultTemplatesResource, DefaultRootTemplateParentPath.Hl7v2.ToString())), cancellationToken);
            var ccdaDefaultTemplatesTask = Task.Run(() => templates.Add(ExtractTemplatesFromResource(DefaultTemplateInfo.CcdaDefaultTemplatesResource, DefaultRootTemplateParentPath.Ccda.ToString())), cancellationToken);
            var jsonDefaultTemplatesTask = Task.Run(() => templates.Add(ExtractTemplatesFromResource(DefaultTemplateInfo.JsonDefaultTemplatesResource, DefaultRootTemplateParentPath.Json.ToString())), cancellationToken);
            var stu3ToR4DefaultTemplatesTask = Task.Run(() => templates.Add(ExtractTemplatesFromResource(DefaultTemplateInfo.Stu3ToR4DefaultTemplatesResource, DefaultRootTemplateParentPath.Fhir.ToString())), cancellationToken);
            var fhirToHl7v2DefaultTemplatesTask = Task.Run(() => templates.Add(ExtractTemplatesFromResource(DefaultTemplateInfo.FhirToHl7v2DefaultTemplatesResource, DefaultRootTemplateParentPath.FhirToHl7v2.ToString())), cancellationToken);

            await Task.WhenAll(hl7v2DefaultTemplatesTask, ccdaDefaultTemplatesTask, jsonDefaultTemplatesTask, stu3ToR4DefaultTemplatesTask, fhirToHl7v2DefaultTemplatesTask);

            var templatesDict = templates
                .SelectMany(dict => dict)
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            var templateCollection = new List<Dictionary<string, Template>>();
            if (templatesDict.Any())
            {
                templateCollection.Add(templatesDict);
            }

            _templateCollectionCache.Set(_defaultTemplateCacheKey, templateCollection, _templateCollectionConfiguration.ShortCacheTimeSpan);

            return templateCollection;
        }

        private Dictionary<string, Template> ExtractTemplatesFromResource(string resourceName, string folderName)
        {
            try
            {
                var templates = new Dictionary<string, Template>();

                // Get the assembly where the resources are embedded
                Assembly assembly = Assembly.GetExecutingAssembly();
                var resourcePath = string.Format(_resourcePathFormat, assembly.GetName().Name, resourceName);

                // Read the content of the embedded resource
                using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
                {
                    var templatesExtracted = StreamUtility.DecompressFromTarGz(stream, folderName);
                    templates = TemplateLayerParser.ParseToTemplates(templatesExtracted);
                }

                return templates;
            }
            catch (Exception ex)
            {
                throw new DefaultTemplatesInitializeException(TemplateManagementErrorCode.DecompressArtifactFailed, ex.Message);
            }
        }
    }
}
