// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Fhir.Liquid.Converter.Hl7v2;
using Microsoft.Health.Fhir.Liquid.Converter.Hl7v2.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Tool.Models;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Liquid.Converter.Tool
{
    internal static class ConverterLogicHandler
    {
        private const string MetadataFileName = "metadata.json";
        private static readonly ILogger Logger;

        static ConverterLogicHandler()
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddFilter("Microsoft.Health.Fhir.Liquid.Converter", LogLevel.Trace)
                       .AddConsole();
            });
            FhirConverterLogging.LoggerFactory = loggerFactory;
            Logger = FhirConverterLogging.CreateLogger(typeof(ConverterLogicHandler));
        }

        internal static void Convert(ConverterOptions options)
        {
            if (!IsValidOptions(options))
            {
                Logger.LogError("Invalid command-line options.");
                return;
            }

            if (!string.IsNullOrEmpty(options.InputDataContent))
            {
                ConvertSingleFile(options);
            }
            else
            {
                ConvertBatchFiles(options);
            }
        }

        private static void ConvertSingleFile(ConverterOptions options)
        {
            try
            {
                var dataType = GetDataTypes(options.TemplateDirectory);
                var dataProcessor = CreateDataProcessor(dataType);
                var templateProvider = CreateTemplateProvider(dataType, options.TemplateDirectory);
                var traceInfo = CreateTraceInfo(dataType);
                var resultString = dataProcessor.Convert(options.InputDataContent, options.RootTemplate, templateProvider, traceInfo);
                var result = new ConverterResult(ProcessStatus.OK, resultString, traceInfo);
                WriteOutputFile(options.OutputDataFile, JsonConvert.SerializeObject(result, Formatting.Indented));
                Logger.LogInformation("Process completed");
            }
            catch (Exception ex)
            {
                var error = new ConverterError(ex, options.TemplateDirectory);
                WriteOutputFile(options.OutputDataFile, JsonConvert.SerializeObject(error, Formatting.Indented));
                Logger.LogError($"Error occurred when converting input data: {error.ErrorMessage}");
            }
        }

        private static void ConvertBatchFiles(ConverterOptions options)
        {
            try
            {
                int succeededCount = 0;
                int failedCount = 0;
                var dataType = GetDataTypes(options.TemplateDirectory);
                var dataProcessor = CreateDataProcessor(dataType);
                var templateProvider = CreateTemplateProvider(dataType, options.TemplateDirectory);
                var files = GetInputFiles(dataType, options.InputDataFolder);
                foreach (var file in files)
                {
                    try
                    {
                        var result = dataProcessor.Convert(File.ReadAllText(file), options.RootTemplate, templateProvider);
                        var outputFileDirectory = Path.Join(options.OutputDataFolder, Path.GetRelativePath(options.InputDataFolder, Path.GetDirectoryName(file)));
                        var outputFilePath = Path.Join(outputFileDirectory, Path.GetFileNameWithoutExtension(file) + ".json");
                        WriteOutputFile(outputFilePath, result);
                        succeededCount++;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Error occurred when converting file {file}: {ex.Message}");
                        failedCount++;
                    }
                }

                Logger.LogInformation($"Process completed with {succeededCount} files succeeded and {failedCount} files failed");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
            }
        }

        private static DataType GetDataTypes(string templateDirectory)
        {
            if (!Directory.Exists(templateDirectory))
            {
                throw new DirectoryNotFoundException($"Could not find template directory: {templateDirectory}");
            }

            var metadataPath = Path.Join(templateDirectory, MetadataFileName);
            if (File.Exists(metadataPath))
            {
                var content = File.ReadAllText(metadataPath);
                var metadata = JsonConvert.DeserializeObject<Metadata>(content);
                if (Enum.TryParse<DataType>(metadata?.Type, ignoreCase: true, out var type))
                {
                    return type;
                }
                else
                {
                    throw new NotImplementedException($"The conversion from data type '{metadata?.Type}' to FHIR is not supported");
                }
            }
            else
            {
                throw new FileNotFoundException($"Could not find metadata.json in template directory: {templateDirectory}.");
            }
        }

        private static IFhirConverter CreateDataProcessor(DataType dataType)
        {
            if (dataType == DataType.Hl7v2)
            {
                return new Hl7v2Processor();
            }

            throw new NotImplementedException($"The conversion from data type {dataType} to FHIR is not supported");
        }

        private static ITemplateProvider CreateTemplateProvider(DataType dataType, string templateDirectory)
        {
            if (dataType == DataType.Hl7v2)
            {
                return new Hl7v2TemplateProvider(templateDirectory);
            }

            throw new NotImplementedException($"The conversion from data type {dataType} to FHIR is not supported");
        }

        private static TraceInfo CreateTraceInfo(DataType dataType)
        {
            return dataType == DataType.Hl7v2 ? new Hl7v2TraceInfo() : new TraceInfo();
        }

        private static List<string> GetInputFiles(DataType dataType, string inputDataFolder)
        {
            if (dataType == DataType.Hl7v2)
            {
                return Directory.EnumerateFiles(inputDataFolder, "*.hl7", SearchOption.AllDirectories).ToList();
            }

            return new List<string>();
        }

        private static void WriteOutputFile(string outputFilePath, string content)
        {
            var outputFileDirectory = Path.GetDirectoryName(outputFilePath);
            Directory.CreateDirectory(outputFileDirectory);
            File.WriteAllText(outputFilePath, content);
        }

        private static bool IsValidOptions(ConverterOptions options)
        {
            var contentTofile = !string.IsNullOrEmpty(options.InputDataContent) &&
                !string.IsNullOrEmpty(options.OutputDataFile) &&
                string.IsNullOrEmpty(options.InputDataFolder) &&
                string.IsNullOrEmpty(options.OutputDataFolder);

            var folderTofolder = string.IsNullOrEmpty(options.InputDataContent) &&
                string.IsNullOrEmpty(options.OutputDataFile) &&
                !string.IsNullOrEmpty(options.InputDataFolder) &&
                !string.IsNullOrEmpty(options.OutputDataFolder) &&
                !IsSameDirectory(options.InputDataFolder, options.OutputDataFolder);

            return contentTofile || folderTofolder;
        }

        private static bool IsSameDirectory(string inputFolder, string outputFolder)
        {
            string inputFolderPath = Path.GetFullPath(inputFolder)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string outputFolderPath = Path.GetFullPath(outputFolder)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            return string.Equals(inputFolderPath, outputFolderPath, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
