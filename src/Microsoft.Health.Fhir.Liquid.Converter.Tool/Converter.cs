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
    public class Converter
    {
        private readonly IFhirConverter dataProcessor;
        private readonly ITemplateProvider templateProvider;

        private readonly string rootTemplate;
        private readonly TraceInfo traceInfo;

        public Converter(IFhirConverter dataProcessor, ITemplateProvider templateProvider, string rootTemplate, TraceInfo traceInfo)
        {
            this.dataProcessor = dataProcessor;
            this.templateProvider = templateProvider;
            this.rootTemplate = rootTemplate;
            this.traceInfo = traceInfo;
        }

        public void SingleFile(string inputContent, string outputFile)
        {
            var resultString = this.dataProcessor.Convert(inputContent, this.rootTemplate, templateProvider, traceInfo);
            var result = new ConverterResult(ProcessStatus.OK, resultString, traceInfo);
            SaveResult(outputFile, result);
        }

        public void BatchFiles(List<string> files, string outputFolder)
        {
            foreach (var file in files)
            {
                Console.WriteLine($"Processing {Path.GetFullPath(file)}");
                var fileContent = File.ReadAllText(file);
                var outputFilePath = Path.Join(outputFolder, Path.GetFileNameWithoutExtension(file) + ".json");
                SingleFile(fileContent, outputFilePath);
            }
        }

        private static void SaveResult(string outputFilePath, ConverterResult result)
        {
            var outputFileDirectory = Path.GetDirectoryName(outputFilePath);
            Directory.CreateDirectory(outputFileDirectory);

            var content = JsonConvert.SerializeObject(result, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            File.WriteAllText(outputFilePath, content);
        }
    }
}
