// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Health.Fhir.Liquid.Converter.Tool.Models;
using Microsoft.Health.Fhir.TemplateManagement;
using Microsoft.Health.Fhir.TemplateManagement.Models;

namespace Microsoft.Health.Fhir.Liquid.Converter.Tool
{
    internal static class TemplateManagementLogicHandler
    {
        internal static async Task PullAsync(PullTemplateOptions options)
        {
            var imageInfo = ImageInfo.CreateFromImageReference(options.ImageReference);
            OciFileManager fileManager = new OciFileManager(imageInfo.Registry, options.OutputTemplateFolder);
            var artifactImage = await fileManager.PullOciImageAsync(imageInfo.ImageName, imageInfo.Label, options.ForceOverride);
            Console.WriteLine($"Digest: {artifactImage.ImageDigest}");
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

            var imageInfo = ImageInfo.CreateFromImageReference(options.ImageReference);
            OciFileManager fileManager = new OciFileManager(imageInfo.Registry, options.InputTemplateFolder);
            var digest = await fileManager.PushOciImageAsync(imageInfo.ImageName, imageInfo.Tag, options.BuildNewBaseLayer);
            Console.WriteLine($"Digest: {digest}");
            Console.WriteLine($"Successfully pushed artifacts to {options.ImageReference}");
        }
    }
}
