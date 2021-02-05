﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Health.Fhir.Liquid.Converter.Cda;
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
        private static readonly List<string> CdaExtensions = new List<string> { ".cda", ".xml" };

        internal static void Convert(ConverterOptions options)
        {
            if (!IsValidOptions(options))
            {
                throw new InputParameterException("Invalid command-line options.");
            }

            var dataType = GetDataTypes(options.TemplateDirectory);
            var dataProcessor = CreateDataProcessor(dataType);
            var templateProvider = CreateTemplateProvider(dataType, options.TemplateDirectory);

            if (!string.IsNullOrEmpty(options.InputDataContent))
            {
                ConvertSingleFile(dataProcessor, templateProvider, dataType, options.RootTemplate, options.InputDataContent, options.OutputDataFile, options.IsTraceInfo);
            }
            else
            {
                ConvertBatchFiles(dataProcessor, templateProvider, dataType, options.RootTemplate, options.InputDataFolder, options.OutputDataFolder, options.IsTraceInfo);
            }

            Console.WriteLine($"Conversion completed!");
        }

        private static void ConvertSingleFile(IFhirConverter dataProcessor, ITemplateProvider templateProvider, DataType dataType, string rootTemplate, string inputContent, string outputFile, bool isTraceInfo)
        {
            var traceInfo = CreateTraceInfo(dataType, isTraceInfo);
            var resultString = dataProcessor.Convert(inputContent, rootTemplate, templateProvider, traceInfo);
            var result = new ConverterResult(ProcessStatus.OK, resultString, traceInfo);
            SaveConverterResult(outputFile, result);
        }

        private static void ConvertBatchFiles(IFhirConverter dataProcessor, ITemplateProvider templateProvider, DataType dataType, string rootTemplate, string inputFolder, string outputFolder, bool isTraceInfo)
        {
            var files = GetInputFiles(dataType, inputFolder);
            foreach (var file in files)
            {
                Console.WriteLine($"Processing {Path.GetFullPath(file)}");
                var fileContent = File.ReadAllText(file);
                var outputFileDirectory = Path.Join(outputFolder, Path.GetRelativePath(inputFolder, Path.GetDirectoryName(file)));
                var outputFilePath = Path.Join(outputFileDirectory, Path.GetFileNameWithoutExtension(file) + ".json");
                ConvertSingleFile(dataProcessor, templateProvider, dataType, rootTemplate, fileContent, outputFilePath, isTraceInfo);
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
            return dataType switch
            {
                DataType.Hl7v2 => new Hl7v2Processor(),
                DataType.Cda => new CdaProcessor(),
                _ => throw new NotImplementedException($"The conversion from data type {dataType} to FHIR is not supported")
            };
        }

        private static ITemplateProvider CreateTemplateProvider(DataType dataType, string templateDirectory)
        {
            return dataType switch
            {
                DataType.Hl7v2 => new Hl7v2TemplateProvider(templateDirectory),
                DataType.Cda => new CdaTemplateProvider(templateDirectory),
                _ => throw new NotImplementedException($"The conversion from data type {dataType} to FHIR is not supported")
            };
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
                DataType.Cda => Directory.EnumerateFiles(inputDataFolder, "*.*", SearchOption.AllDirectories)
                    .Where(x => CdaExtensions.Contains(Path.GetExtension(x).ToLower())).ToList(),
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
                !string.IsNullOrEmpty(options.OutputDataFile) &&
                string.IsNullOrEmpty(options.InputDataFolder) &&
                string.IsNullOrEmpty(options.OutputDataFolder);

            var folderToFolder = string.IsNullOrEmpty(options.InputDataContent) &&
                string.IsNullOrEmpty(options.OutputDataFile) &&
                !string.IsNullOrEmpty(options.InputDataFolder) &&
                !string.IsNullOrEmpty(options.OutputDataFolder) &&
                !IsSameDirectory(options.InputDataFolder, options.OutputDataFolder);

            return contentToFile || folderToFolder;
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
