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
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Liquid.Converter.Tool
{
    internal static class TemplateManagementLogicHandler
    {
        private static readonly ILogger Logger;
        private static readonly TextWriter ErrorWriter;

        static TemplateManagementLogicHandler()
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddFilter("Microsoft.Health.Fhir.TemplateManagement", LogLevel.Trace)
                       .AddConsole();
            });
            FhirConverterLogging.LoggerFactory = loggerFactory;
            Logger = FhirConverterLogging.CreateLogger(typeof(TemplateManagementLogicHandler));

            ErrorWriter = Console.Error;
        }

        internal static async Task PullAsync(PullTemplateOptions options)
        {
            try
            {
                if (!options.ForceOverride)
                {
                    if (Directory.Exists(options.OutputTemplateFolder) && Directory.GetFileSystemEntries(options.OutputTemplateFolder).Length != 0)
                    {
                        ExceptionHandling(options.ErrorJsonFile, new Exception("The output folder is not empty. If force to override, please add -f in parameters"), "Process Exits: ");
                        return;
                    }
                }

                OCIFileManager fileManager = new OCIFileManager(options.ImageReference, options.OutputTemplateFolder);
                if (await fileManager.PullOCIImageAsync())
                {
                    fileManager.UnpackOCIImage();
                    Logger.LogInformation($"Succeed to pull templates to {options.OutputTemplateFolder} folder");
                }
                else
                {
                    ExceptionHandling(options.ErrorJsonFile, new TemplateManagementException("Fail to pull templates."), "Process Exits: ");
                }
            }
            catch (Exception ex)
            {
                ExceptionHandling(options.ErrorJsonFile, ex, "Process Exits: Fail to pull templates. ");
            }
        }

        internal static async Task PushAsync(PushTemplateOptions options)
        {
            try
            {
                if (!Directory.Exists(options.InputTemplateFolder))
                {
                    ExceptionHandling(options.ErrorJsonFile, new Exception($"Input folder {options.InputTemplateFolder} not exist."), "Process Exits: ");
                    return;
                }

                if (Directory.GetFileSystemEntries(options.InputTemplateFolder).Length == 0)
                {
                    ExceptionHandling(options.ErrorJsonFile, new Exception($"Input folder {options.InputTemplateFolder} is empty."), "Process Exits: ");
                    return;
                }

                OCIFileManager fileManager = new OCIFileManager(options.ImageReference, options.InputTemplateFolder);
                fileManager.PackOCIImage(options.BuildNewBaseLayer);
                if (await fileManager.PushOCIImageAsync())
                {
                    Logger.LogInformation($"Succeed to push new templates to {options.ImageReference}");
                }
                else
                {
                    ExceptionHandling(options.ErrorJsonFile, new Exception("Fail to push templates."), "Process Exits: ");
                }
            }
            catch (Exception ex)
            {
                ExceptionHandling(options.ErrorJsonFile, ex, "Process Exits: Fail to push templates. ");
            }
        }

        private static void ExceptionHandling(string errorfolder, Exception ex, string helpMassage = "")
        {
            if (!string.IsNullOrEmpty(errorfolder))
            {
                var error = new ConverterError(ex);
                WriteOutputFile(errorfolder, JsonConvert.SerializeObject(error));
            }

            ErrorWriter.WriteLine(helpMassage + ex.ToString());
        }

        private static void WriteOutputFile(string outputFilePath, string content)
        {
            var outputFileDirectory = Path.GetDirectoryName(outputFilePath);
            if (!string.IsNullOrEmpty(outputFileDirectory))
            {
                Directory.CreateDirectory(outputFileDirectory);
            }

            File.WriteAllText(outputFilePath, content);
        }
    }
}
