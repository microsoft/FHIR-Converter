// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using CommandLine;

namespace Microsoft.Health.Fhir.Liquid.Converter.Tool.Models
{
    [Verb("pull", HelpText = "Pull template image to registry")]
    public class PullTemplateOptions
    {
        [Value(0, Required = true, HelpText = "Image reference: < >/<imageName>:<imageTag>")]
        public string ImageReference { get; set; }

        [Option('o', "OutputTemplateFolder", Required = false, Default = ".", HelpText = "Output template folder")]
        public string OutputTemplateFolder { get; set; }
    }
}
