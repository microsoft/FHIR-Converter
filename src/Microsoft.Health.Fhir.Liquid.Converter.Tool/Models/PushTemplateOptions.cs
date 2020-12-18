// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using CommandLine;

namespace Microsoft.Health.Fhir.Liquid.Converter.Tool.Models
{
    [Verb("push", HelpText = "Push a template image to a registry")]
    public class PushTemplateOptions
    {
        [Value(0, Required = true, HelpText = "Image reference: <registry>/<imageName>:<imageTag>")]
        public string ImageReference { get; set; }

        [Value(1, Required = true, HelpText = "Input template folder")]
        public string InputTemplateFolder { get; set; }

        [Option('n', "NewBaseLayer", Required = false, Default = false, HelpText = "Build new base layer")]
        public bool BuildNewBaseLayer { get; set; }
    }
}
