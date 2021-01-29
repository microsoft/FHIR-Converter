// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Models;

namespace Microsoft.Health.Fhir.TemplateManagement.Client
{
    public class OrasClient : IOrasClient
    {
        private readonly string _imageReference;
        private readonly Regex _digestRx = new Regex("(?<rule>[A-Za-z][A-Za-z0-9]*([+.-_][A-Za-z][A-Za-z0-9]*)*):(?<digest>[0-9a-fA-F]{32,})");

        public OrasClient(string imageReference)
        {
            EnsureArg.IsNotNull(imageReference, nameof(imageReference));

            _imageReference = imageReference;
        }

        public async Task<string> PullImageAsync(string outputFolder)
        {
            string command = $"pull  \"{_imageReference}\" -o \"{outputFolder}\"";
            string output = await OrasExecutionAsync(command, Directory.GetCurrentDirectory());
            Console.WriteLine(output);
            var digest = _digestRx.Matches(output)[0].Groups["digest"].ToString();
            return digest;
        }

        public async Task PushImageAsync(string inputFolder, List<string> filePathList)
        {
            string argument = string.Empty;
            string command = $"push \"{_imageReference}\"";

            // In order to remove image's directory prefix. (e.g. "layers/layer1.tar.gz" --> "layer1.tar.gz"
            // Change oras working folder to inputFolder
            foreach (var filePath in filePathList)
            {
                if (!File.Exists(Path.Combine(inputFolder, filePath)))
                {
                    throw new OverlayException(TemplateManagementErrorCode.ImageLayersNotFound, "Image layer not found");
                }

                argument += $" \"{filePath}\"";
            }

            if (string.IsNullOrEmpty(argument))
            {
                throw new OverlayException(TemplateManagementErrorCode.ImageLayersNotFound, "No file for push.");
            }

            Console.WriteLine(await OrasExecutionAsync(string.Concat(command, argument), inputFolder));
        }

        public static async Task<string> OrasExecutionAsync(string command, string orasWorkingDirectory)
        {
            TaskCompletionSource<bool> eventHandled = new TaskCompletionSource<bool>();

            Process process = new Process
            {
                StartInfo = new ProcessStartInfo(Constants.OrasFile),
            };

            process.StartInfo.Arguments = command;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
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
            StreamReader outputStreamReader = process.StandardOutput;
            await Task.WhenAny(eventHandled.Task, Task.Delay(Constants.TimeOutMilliseconds));
            if (process.HasExited)
            {
                string error = errStreamReader.ReadToEnd();
                if (!string.IsNullOrEmpty(error))
                {
                    throw new OrasException(TemplateManagementErrorCode.OrasProcessFailed, error);
                }

                return outputStreamReader.ReadToEnd();
            }
            else
            {
                throw new OrasException(TemplateManagementErrorCode.OrasTimeOut, "Oras request timeout");
            }
        }
    }
}