// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Health.Fhir.Liquid.Converter.Tool.Models;
using Microsoft.Health.Fhir.TemplateManagement;

namespace Microsoft.Health.Fhir.Liquid.Converter.Tool
{
    internal static class TemplateManagementLogicHandler
    {
        internal static async Task PullAsync(PullTemplateOptions options)
        {
            OCIFileManager fileManager = new OCIFileManager(options.ImageReference, options.OutputTemplateFolder);
            var imageInfo = await fileManager.PullOCIImageAsync(options.ForceOverride);
            Console.WriteLine($"Digest: {imageInfo.Digest}");
            Console.WriteLine($"Successfully pulled artifacts to {options.OutputTemplateFolder} folder");
        }

        internal static async Task PushAsync(PushTemplateOptions options)
        {
            if (!Directory.Exists(options.InputTemplateFolder))
            {
                throw new InputParameterException($"Input folder {options.InputTemplateFolder} not exist.");
            }

            if (Directory.GetFileSystemEntries(options.InputTemplateFolder).Length == 0)
            {
                throw new InputParameterException($"Input folder {options.InputTemplateFolder} is empty.");
            }

            OCIFileManager fileManager = new OCIFileManager(options.ImageReference, options.InputTemplateFolder);
            var imageInfo = await fileManager.PushOCIImageAsync(options.BuildNewBaseLayer);
            Console.WriteLine($"Digest: {imageInfo.Digest}");
            Console.WriteLine($"Successfully pushed artifacts to {options.ImageReference}");
        }
    }
}
