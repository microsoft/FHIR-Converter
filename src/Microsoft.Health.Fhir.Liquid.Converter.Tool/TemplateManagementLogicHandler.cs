// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Fhir.Liquid.Converter.Tool.Models;
using Microsoft.Health.Fhir.TemplateManagement;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Liquid.Converter.Tool
{
    internal static class TemplateManagementLogicHandler
    {
        private static readonly ILogger Logger;

        static TemplateManagementLogicHandler()
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddFilter("Microsoft.Health.Fhir.TemplateManagement", LogLevel.Trace)
                       .AddConsole();
            });
            FhirConverterLogging.LoggerFactory = loggerFactory;
            Logger = FhirConverterLogging.CreateLogger(typeof(TemplateManagementLogicHandler));
        }

        internal static async Task PullAsync(PullTemplateOptions options)
        {
            try
            {
                OCIFileManager fileManager = new OCIFileManager(options.ImageReference, options.OutputTemplateFolder);
                if (await fileManager.PullOCIImageAsync())
                {
                    fileManager.UnpackOCIImage();
                    Logger.LogInformation($"Succeed to pull templates to {options.OutputTemplateFolder} folder");
                }

                Logger.LogInformation("Pull process complete.");
            }
            catch (Exception ex)
            {
                Logger.LogError("Fail to pull templates.");
                var error = new ConverterError(ex);
                TextWriter errorWriter = Console.Error;
                errorWriter.WriteLine(JsonConvert.SerializeObject(error));
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
                    Logger.LogInformation($"Succeed to push new templates to {options.ImageReference}");
                }

                Logger.LogInformation("Push process complete.");
            }
            catch (Exception ex)
            {
                Logger.LogError("Fail to push templates.");
                var error = new ConverterError(ex);
                TextWriter errorWriter = Console.Error;
                errorWriter.WriteLine(JsonConvert.SerializeObject(error));
            }
        }
    }
}
