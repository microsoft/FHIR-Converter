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
            if (!options.ForceOverride)
            {
                if (Directory.Exists(options.OutputTemplateFolder) && Directory.GetFileSystemEntries(options.OutputTemplateFolder).Length != 0)
                {
                    throw new InputParameterException($"The output folder is not empty. If force to override, please add -f in parameters");
                }
            }

            OCIFileManager fileManager = new OCIFileManager(options.ImageReference, options.OutputTemplateFolder);
            await fileManager.PullOCIImageAsync();
            fileManager.UnpackOCIImage();
            Console.WriteLine($"Successfully pulled templates to {options.OutputTemplateFolder} folder");
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
            fileManager.PackOCIImage(options.BuildNewBaseLayer);
            await fileManager.PushOCIImageAsync();
            Console.WriteLine($"Successfully pushed new templates to {options.ImageReference}");
        }
    }
}
