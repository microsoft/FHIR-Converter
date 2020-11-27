// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Models;

namespace Microsoft.Health.Fhir.TemplateManagement.Client
{
    // Todo need to be async

    public class OrasClient : IOrasClient
    {
        private readonly string _imageReference;

        private readonly string _imageLayerFolder;

        public OrasClient(string imageReference, string imageLayerFolder)
        {
            EnsureArg.IsNotNull(imageReference, nameof(imageReference));
            EnsureArg.IsNotNull(imageLayerFolder, nameof(imageLayerFolder));

            _imageReference = imageReference;
            _imageLayerFolder = imageLayerFolder;
        }

        public async Task<bool> PullImageAsync()
        {
            string command = $"pull  {_imageReference} -o {_imageLayerFolder}";
            await OrasExecutionAsync(command, ".");
            return true;
        }

        public async Task<bool> PushImageAsync()
        {
            string argument = string.Empty;
            string command = $"push {_imageReference}";

            if (!Directory.Exists(_imageLayerFolder))
            {
                Console.WriteLine($"No file for push.");
                return false;
            }

            var filePathToPush = Directory.EnumerateFiles(_imageLayerFolder, "*.tar.gz", SearchOption.AllDirectories);
            foreach (var filePath in filePathToPush)
            {
                argument += $" {Path.GetRelativePath(_imageLayerFolder, filePath)}";
            }

            if (string.IsNullOrEmpty(argument))
            {
                Console.WriteLine($"No file for push.");
                return false;
            }

            // In order to remove image's directory prefix. (e.g. "layers/layer1.tar.gz" --> "layer1.tar.gz"
            // Change oras working folder into imageLayerFolder
            await OrasExecutionAsync(string.Concat(command, argument), _imageLayerFolder);
            return true;
        }

        private async Task OrasExecutionAsync(string command, string orasWorkingDirectory)
        {
            TaskCompletionSource<bool> eventHandled = new TaskCompletionSource<bool>();
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo(Path.Combine(AppContext.BaseDirectory, "oras.exe")),
            };

            process.StartInfo.Arguments = command;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.WorkingDirectory = orasWorkingDirectory;
            process.EnableRaisingEvents = true;

            // Add event to make it async.
            process.Exited += new EventHandler((sender, e) => { eventHandled.TrySetResult(true); });
            try
            {
                process.Start();
            }
            catch (Exception ex)
            {
                throw new OrasException(TemplateManagementErrorCode.OrasProcessFailed, "Oras process failed", ex);
            }

            StreamReader errStreamReader = process.StandardError;
            await Task.WhenAny(eventHandled.Task, Task.Delay(30000));
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