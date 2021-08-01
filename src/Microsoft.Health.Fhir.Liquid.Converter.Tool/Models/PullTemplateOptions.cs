// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using CommandLine;

namespace Microsoft.Health.Fhir.Liquid.Converter.Tool.Models
{
    [Verb("pull", HelpText = "Pull a template image from a registry")]
    public class PullTemplateOptions
    {
        [Value(0, Required = true, HelpText = "Image reference: <registry>/<imageName>:<imageTag> or <registry>/<imageName>@<imageDigest>")]
        public string ImageReference { get; set; }

        [Value(1, Required = true, HelpText = "Output template folder")]
        public string OutputTemplateFolder { get; set; }

        [Option('f', "ForceOverride", Required = false, Default = false, HelpText = "Force to override existed files")]
        public bool ForceOverride { get; set; }
    }
}
