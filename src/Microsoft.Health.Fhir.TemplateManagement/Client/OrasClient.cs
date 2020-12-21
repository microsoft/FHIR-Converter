// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Microsoft.Health.Fhir.TemplateManagement.Utilities;

namespace Microsoft.Health.Fhir.TemplateManagement.Client
{
    public class OrasClient : IOrasClient
    {
        private readonly string _imageReference;
        private readonly string _orasFileName;

        public OrasClient(string imageReference)
        {
            EnsureArg.IsNotNull(imageReference, nameof(imageReference));

            _orasFileName = OrasUtility.GetOrasFileName();
            _imageReference = imageReference;
        }

        public async Task PullImageAsync(string outputFolder)
        {
            string command = $"pull  {_imageReference} -o {outputFolder}";
            await OrasUtility.OrasExecutionAsync(command, _orasFileName, Directory.GetCurrentDirectory());
        }

        public async Task PushImageAsync(string inputFolder)
        {
            string argument = string.Empty;
            string command = $"push {_imageReference}";

            var filePathToPush = Directory.EnumerateFiles(inputFolder, "*.tar.gz", SearchOption.AllDirectories);

            // In order to remove image's directory prefix. (e.g. "layers/layer1.tar.gz" --> "layer1.tar.gz"
            // Change oras working folder to inputFolder
            foreach (var filePath in filePathToPush)
            {
                argument += $" {Path.GetRelativePath(inputFolder, filePath)}";
            }

            if (string.IsNullOrEmpty(argument))
            {
                throw new OverlayException(TemplateManagementErrorCode.ImageLayersNotFound, "No file for push.");
            }

            await OrasUtility.OrasExecutionAsync(string.Concat(command, argument), _orasFileName, inputFolder);
        }
    }
}