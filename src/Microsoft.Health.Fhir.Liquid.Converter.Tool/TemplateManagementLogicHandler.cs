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
        internal static async Task<int> PullAsync(PullTemplateOptions options)
        {
            try
            {
                if (!options.ForceOverride)
                {
                    if (Directory.Exists(options.OutputTemplateFolder) && Directory.GetFileSystemEntries(options.OutputTemplateFolder).Length != 0)
                    {
                        Console.Error.WriteLine($"Process Exits: The output folder is not empty. If force to override, please add -f in parameters");
                        return -1;
                    }
                }

                OCIFileManager fileManager = new OCIFileManager(options.ImageReference, options.OutputTemplateFolder);
                if (await fileManager.PullOCIImageAsync())
                {
                    fileManager.UnpackOCIImage();
                    Console.WriteLine($"Succeed to pull templates to {options.OutputTemplateFolder} folder");
                    return 0;
                }
                else
                {
                    Console.Error.WriteLine($"Fail to pull templates.");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Fail to pull templates. {ex.Message} ");
            }

            return -1;
        }

        internal static async Task<int> PushAsync(PushTemplateOptions options)
        {
            try
            {
                if (!Directory.Exists(options.InputTemplateFolder))
                {
                    Console.Error.WriteLine($"Process Exits: Input folder {options.InputTemplateFolder} not exist.");
                    return -1;
                }

                if (Directory.GetFileSystemEntries(options.InputTemplateFolder).Length == 0)
                {
                    Console.Error.WriteLine($"Process Exits: Input folder {options.InputTemplateFolder} is empty.");
                    return -1;
                }

                OCIFileManager fileManager = new OCIFileManager(options.ImageReference, options.InputTemplateFolder);
                fileManager.PackOCIImage(options.BuildNewBaseLayer);
                if (await fileManager.PushOCIImageAsync())
                {
                    Console.WriteLine($"Succeed to push new templates to {options.ImageReference}");
                    return 0;
                }
                else
                {
                    Console.Error.WriteLine("Fail to push templates.");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Fail to push templates. {ex.Message} ");
            }

            return -1;
        }
    }
}
