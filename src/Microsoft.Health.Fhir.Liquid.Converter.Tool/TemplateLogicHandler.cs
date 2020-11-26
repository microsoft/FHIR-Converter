// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Fhir.Liquid.Converter.Tool.Models;
using Microsoft.Health.Fhir.TemplateManagement;

namespace Microsoft.Health.Fhir.Liquid.Converter.Tool
{
    internal static class TemplateLogicHandler
    {
        internal static void Pull(PullTemplateOptions options)
        {
            try
            {
                OCIFileManager fileManager = new OCIFileManager(options.ImageReference, options.OutputTemplateFolder);
                fileManager.PullOCIImage();
                fileManager.UnpackOCIImage();
                Console.WriteLine("Succeed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred when pull new templates: {ex}");
            }
        }

        internal static void Push(PushTemplateOptions options)
        {
            try
            {
                OCIFileManager fileManager = new OCIFileManager(options.ImageReference, options.InputTemplateFolder);
                fileManager.PackOCIImage(options.BuildNewBaseLayer);
                fileManager.PushOCIImage();
                Console.WriteLine("Succeed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred when push new templates: {ex}");
            }
        }
    }
}
