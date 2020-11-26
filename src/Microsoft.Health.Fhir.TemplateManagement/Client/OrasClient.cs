// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using EnsureThat;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Models;

namespace Microsoft.Health.Fhir.TemplateManagement.Client
{
    // Todo need to be async

    public class OrasClient : IOrasClient
    {
        private readonly string _imageReference;

        private readonly string _workingImageLayerFolder;

        public OrasClient(string imageReference, string workingFolder)
        {
            EnsureArg.IsNotNull(imageReference, nameof(imageReference));
            EnsureArg.IsNotNull(workingFolder, nameof(workingFolder));

            _imageReference = imageReference;
            _workingImageLayerFolder = Path.Combine(workingFolder, Constants.HiddenLayersFolder);
        }

        public void PullImage()
        {
            string command = $"pull  {_imageReference} -o {_workingImageLayerFolder}";
            OrasExecution(command, Directory.GetCurrentDirectory());
        }

        public void PushImage()
        {
            var filePathToPush = Directory.EnumerateFiles(_workingImageLayerFolder, "*.tar.gz", SearchOption.AllDirectories);

            if (filePathToPush == null || filePathToPush.Count() == 0)
            {
                throw new OrasException("No file will be pushed");
            }

            string command = $"push {_imageReference}";
            foreach (var filePath in filePathToPush)
            {
                command += $" {Path.GetRelativePath(_workingImageLayerFolder, filePath)}";
            }

            OrasExecution(command, Path.Combine(Directory.GetCurrentDirectory(), _workingImageLayerFolder));
        }

        private void OrasExecution(string command, string workingDirectory)
        {

            Process process = new Process
            {
                StartInfo = new ProcessStartInfo(Path.Combine(AppContext.BaseDirectory, "oras.exe")),
            };

            process.StartInfo.Arguments = command;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.WorkingDirectory = workingDirectory;
            process.Start();

            StreamReader errStreamReader = process.StandardError;
            process.WaitForExit(60000);
            if (process.HasExited)
            {
                string error = errStreamReader.ReadToEnd();
                if (!string.IsNullOrEmpty(error))
                {
                    throw new OrasException(error);
                }
            }
            else
            {
                throw new OrasException(TemplateManagementErrorCode.OrasTimeOut, "Oras request timeout");
            }
        }
    }
}
