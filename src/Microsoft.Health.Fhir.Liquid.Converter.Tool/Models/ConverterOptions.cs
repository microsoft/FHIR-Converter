// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using CommandLine;

namespace Microsoft.Health.Fhir.Liquid.Converter.Tool.Models
{
    [Verb("convert", HelpText = "Convert input data to FHIR resources")]
    public class ConverterOptions
    {
        [Option('d', "TemplateDirectory", Required = true, HelpText = "Root directory of templates")]
        public string TemplateDirectory { get; set; }

        [Option('r', "RootTemplate", Required = true, HelpText = "Name of root template")]
        public string RootTemplate { get; set; }

        [Option('c', "InputDataContent", Required = false, HelpText = "Input data content. Please specify OutputDataFile to get the results.")]
        public string InputDataContent { get; set; }

        [Option('n', "InputDataFile", Required = false, HelpText = "Input data file. Please specify OutputDataFile to get the results.")]
        public string InputDataFile { get; set; }

        [Option('f', "OutputDataFile", Required = false, HelpText = "Output data file")]
        public string OutputDataFile { get; set; }

        [Option('i', "InputDataFolder", Required = false, HelpText = "Input data folder. Please specify OutputDataFolder to get the results.")]
        public string InputDataFolder { get; set; }

        [Option('o', "OutputDataFolder", Required = false, HelpText = "Output data folder")]
        public string OutputDataFolder { get; set; }

        [Option('t', "IsTraceInfo", Required = false, HelpText = "Provide trace information in the output")]
        public bool IsTraceInfo { get; set; }

        [Option('v', "Verbose", Required = false, HelpText = "Output detailed processor diagnostics and performance data.")]
        public bool IsVerboseEnabled { get; set; }
    }
}
