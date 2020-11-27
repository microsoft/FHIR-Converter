// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.Health.Fhir.Liquid.Converter.Tool.Models;
using Microsoft.Health.Fhir.TemplateManagement;

namespace Microsoft.Health.Fhir.Liquid.Converter.Tool
{
    internal static class TemplateLogicHandler
    {
        internal static async Task PullAsync(PullTemplateOptions options)
        {
            try
            {
                OCIFileManager fileManager = new OCIFileManager(options.ImageReference, options.OutputTemplateFolder);
                if (await fileManager.PullOCIImageAsync())
                {
                    fileManager.UnpackOCIImage();
                    Console.WriteLine("Succeed!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred when pull new templates: {ex}");
            }
        }

        internal static async Task PushAsync(PushTemplateOptions options)
        {
            try
            {
                OCIFileManager fileManager = new OCIFileManager(options.ImageReference, options.InputTemplateFolder);
                fileManager.PackOCIImage(options.BuildNewBaseLayer);
                if (await fileManager.PushOCIImageAsync())
                {
                    Console.WriteLine("Succeed!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred when push new templates: {ex}");
            }
        }
    }
}
