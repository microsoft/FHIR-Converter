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
        internal static void Pull(object options)
        {
            try
            {
                OCIFileManager fileManager = new OCIFileManager(((PullTemplateOptions)options).ImageReference, ((PullTemplateOptions)options).OutputTemplateFolder);
                string output = fileManager.PullOCIImage();
                fileManager.UnpackOCIImage();
                Console.WriteLine("Succeed!");
                Console.WriteLine(output);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred when pull new templates: {ex}");
            }
        }

        internal static void Push(object options)
        {
            try
            {
                OCIFileManager fileManager = new OCIFileManager(((PushTemplateOptions)options).ImageReference, ((PushTemplateOptions)options).InputTemplateFolder);
                fileManager.PackOCIImage(((PushTemplateOptions)options).BuildNewBaseLayer);
                string output = fileManager.PushOCIImage();
                Console.WriteLine("Succeed!");
                Console.WriteLine(output);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred when push new templates: {ex}");
            }
        }
    }
}
