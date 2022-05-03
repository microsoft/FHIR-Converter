// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Models.Hl7v2;
using Microsoft.Health.Fhir.Liquid.Converter.Processors;
using Microsoft.Health.Fhir.Liquid.Converter.Tool.Models;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Liquid.Converter.Tool
{
    internal static class ConverterLogicHandler
    {
        private const string MetadataFileName = "metadata.json";
        private static readonly List<string> CcdaExtensions = new List<string> { ".ccda", ".xml" };

        internal static async void Convert(ConverterOptions options)
        {
            if (!IsValidOptions(options))
            {
                throw new InputParameterException("Invalid command-line options.");
            }

            var metadata = GetTemplateMetadata(options.TemplateDirectory);
            var dataType = MakeDataType(metadata);
            var dataProcessor = CreateDataProcessor(dataType, metadata.ProcessorSettings);
            var templateProvider = CreateTemplateProvider(dataType, options.TemplateDirectory);

            var traceInfo = CreateTraceInfo(dataType, options.IsTraceInfo);

            var converter = new Converter(dataProcessor, templateProvider, options.RootTemplate, traceInfo);

            if (!string.IsNullOrEmpty(options.InputDataFolder))
            {
                // Convert an entire folder of files in batch
                var files = GetInputFiles(dataType, options.InputDataFolder);
                var outputFolder = Path.Join(options.OutputDataFolder, options.InputDataFolder);

                converter.BatchFiles(files, outputFolder);
            } else {
                // Convert a single file
                var fileContent = options.InputDataContent;

                // If we have a filepath, retrieve the content
                if (!string.IsNullOrEmpty(options.InputDataFile))
                {
                    fileContent = File.ReadAllText(options.InputDataFile);
                }

                converter.SingleFile(fileContent, options.OutputDataFile);
            }

            Console.WriteLine($"Conversion completed!");
        }

        private static Metadata GetTemplateMetadata(string templateDirectory)
        {
            if (!Directory.Exists(templateDirectory))
            {
                throw new DirectoryNotFoundException($"Could not find template directory: {templateDirectory}");
            }

            var metadataPath = Path.Join(templateDirectory, MetadataFileName);
            if (!File.Exists(metadataPath))
            {
                throw new FileNotFoundException($"Could not find metadata.json in template directory: {templateDirectory}.");
            }

            var content = File.ReadAllText(metadataPath);
            var metadata = JsonConvert.DeserializeObject<Metadata>(content);

            return metadata;
        }

        private static IFhirConverter CreateDataProcessor(DataType dataType, ProcessorSettings processorSettings)
        {
            return dataType switch
            {
                DataType.Hl7v2 => new Hl7v2Processor(processorSettings),
                DataType.Ccda => new CcdaProcessor(processorSettings),
                DataType.Json => new JsonProcessor(processorSettings),
                _ => throw new NotImplementedException($"The conversion from data type {dataType} to FHIR is not supported")
            };
        }

        private static ITemplateProvider CreateTemplateProvider(DataType dataType, string templateDirectory)
        {
            return dataType switch
            {
                DataType.Hl7v2 => new TemplateProvider(templateDirectory, DataType.Hl7v2),
                DataType.Ccda => new TemplateProvider(templateDirectory, DataType.Ccda),
                DataType.Json => new TemplateProvider(templateDirectory, DataType.Json),
                _ => throw new NotImplementedException($"The conversion from data type {dataType} to FHIR is not supported")
            };
        }

        private static DataType MakeDataType(Metadata metadata)
        {
            if (Enum.TryParse<DataType>(metadata?.Type, ignoreCase: true, out var type))
            {
                return type;
            }

            throw new NotImplementedException($"The conversion from data type '{metadata?.Type}' to FHIR is not supported");
        }

        private static TraceInfo CreateTraceInfo(DataType dataType, bool isTraceInfo)
        {
            return isTraceInfo ? (dataType == DataType.Hl7v2 ? new Hl7v2TraceInfo() : new TraceInfo()) : null;
        }

        private static List<string> GetInputFiles(DataType dataType, string inputDataFolder)
        {
            return dataType switch
            {
                DataType.Hl7v2 => Directory.EnumerateFiles(inputDataFolder, "*.hl7", SearchOption.AllDirectories).ToList(),
                DataType.Ccda => Directory.EnumerateFiles(inputDataFolder, "*.*", SearchOption.AllDirectories)
                    .Where(x => CcdaExtensions.Contains(Path.GetExtension(x).ToLower())).ToList(),
                DataType.Json => Directory.EnumerateFiles(inputDataFolder, "*.json", SearchOption.AllDirectories).ToList(),
                _ => new List<string>(),
            };
        }

        private static void SaveConverterResult(string outputFilePath, ConverterResult result)
        {
            var outputFileDirectory = Path.GetDirectoryName(outputFilePath);
            Directory.CreateDirectory(outputFileDirectory);

            var content = JsonConvert.SerializeObject(result, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            File.WriteAllText(outputFilePath, content);
        }

        private static bool IsValidOptions(ConverterOptions options)
        {
            var contentToFile = !string.IsNullOrEmpty(options.InputDataContent) &&
                                string.IsNullOrEmpty(options.InputDataFile) &&
                                !string.IsNullOrEmpty(options.OutputDataFile) &&
                                string.IsNullOrEmpty(options.InputDataFolder) &&
                                string.IsNullOrEmpty(options.OutputDataFolder);

            var fileToFile = string.IsNullOrEmpty(options.InputDataContent) &&
                                !string.IsNullOrEmpty(options.InputDataFile) &&
                                !string.IsNullOrEmpty(options.OutputDataFile) &&
                                string.IsNullOrEmpty(options.InputDataFolder) &&
                                string.IsNullOrEmpty(options.OutputDataFolder) &&
                                !IsSameFile(options.InputDataFile, options.OutputDataFile);

            var folderToFolder = string.IsNullOrEmpty(options.InputDataContent) &&
                                 string.IsNullOrEmpty(options.InputDataFile) &&
                                 string.IsNullOrEmpty(options.OutputDataFile) &&
                                 !string.IsNullOrEmpty(options.InputDataFolder) &&
                                 !string.IsNullOrEmpty(options.OutputDataFolder) &&
                                 !IsSameDirectory(options.InputDataFolder, options.OutputDataFolder);

            return contentToFile || fileToFile || folderToFolder;
        }

        private static bool IsSameFile(string inputFile, string outputFile)
        {
            return string.Equals(inputFile, outputFile, StringComparison.InvariantCultureIgnoreCase);
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
