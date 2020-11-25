// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Health.Fhir.TemplateManagement;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Models;

namespace Microsoft.Health.Fhir.TemplateManagement.Client
{
    // Todo need to be async

    public class OrasClient : IOrasClient
    {
        private readonly string _imageReference;

        public string WorkingImageLayerFolder { get; }

        public OrasClient(string imageReference, string workingFolder)
        {
            _imageReference = imageReference;
            WorkingImageLayerFolder = Path.Combine(workingFolder, Constants.HiddenLayersFolder);
        }

        public string PullImage()
        {
            string command = $"pull  {_imageReference} -o {WorkingImageLayerFolder}";
            var output = OrasExecution(command);
            return output;
        }

        public string PushImage()
        {
            //var realRunningPath = Directory.GetCurrentDirectory();
            //Directory.SetCurrentDirectory(Path.Combine(realRunningPath, WorkingImageLayerFolder));
            var filePathToPush = Directory.EnumerateFiles(".", "*.tar.gz", SearchOption.AllDirectories);

            if (filePathToPush == null || filePathToPush.Count() == 0)
            {
                throw new OrasException("No file will be pushed");
            }

            string command = $"push {_imageReference}";
            foreach (var filePath in filePathToPush)
            {
                command += $" {Path.GetRelativePath(".", filePath)}";
            }

            var output = OrasExecution(command);
            //Directory.SetCurrentDirectory(realRunningPath);
            return output;
        }

        private string OrasExecution(string command)
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo(Path.Combine(AppContext.BaseDirectory, "oras.exe")),
            };

            process.StartInfo.Arguments = command;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.Start();

            StreamReader outputStreamReader = process.StandardOutput;
            StreamReader errStreamReader = process.StandardError;
            process.WaitForExit(60000);
            if (process.HasExited)
            {
                string output = outputStreamReader.ReadToEnd();
                string error = errStreamReader.ReadToEnd();
                if (!string.IsNullOrEmpty(error))
                {
                    throw new OrasException(error);
                }

                return output;
            }
            else
            {
                throw new OrasException(TemplateManagementErrorCode.OrasTimeOut, "Oras request timeout");
            }
        }
    }
}
