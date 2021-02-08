// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
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
        private readonly Regex _digestRegex = new Regex("(?<algorithm>[A-Za-z][A-Za-z0-9]*([+.-_][A-Za-z][A-Za-z0-9]*)*):(?<hex>[0-9a-fA-F]{32,})");

        public OrasClient(string imageReference)
        {
            EnsureArg.IsNotNull(imageReference, nameof(imageReference));

            _imageReference = imageReference;
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(Constants.OrasCacheEnvironmentVariableName)))
            {
                Environment.SetEnvironmentVariable(Constants.OrasCacheEnvironmentVariableName, Constants.DefaultOrasCacheEnvironmentVariable);
            }
        }

        public async Task<OCIOperationResult> PullImageAsync(string outputFolder)
        {
            string command = $"pull  \"{_imageReference}\" -o \"{outputFolder}\"";
            string output = await OrasExecutionAsync(command, Directory.GetCurrentDirectory());
            string digest;
            try
            {
                digest = _digestRegex.Matches(output)[0].Groups["hex"].ToString();
            }
            catch (Exception ex)
            {
                throw new OrasException(TemplateManagementErrorCode.OrasProcessFailed, "Return image digest failed.", ex);
            }

            if (!Directory.Exists(outputFolder))
            {
                throw new OrasException(TemplateManagementErrorCode.OrasProcessFailed, "Pull image failed or image is empty.");
            }

            File.WriteAllText(Path.Combine(outputFolder, Constants.ManifestFileName), LoadManifestContentFromCache(digest));
            return new OCIOperationResult() { ClientResponse = output };
        }

        public async Task<OCIOperationResult> PushImageAsync(string inputFolder)
        {
            if (!Directory.Exists(inputFolder))
            {
                throw new OverlayException(TemplateManagementErrorCode.ImageLayersNotFound, "Image folder not found.");
            }

            string argument = string.Empty;
            string command = $"push \"{_imageReference}\"";
            var filePathesToPush = Directory.EnumerateFiles(inputFolder, "*.tar.gz", SearchOption.AllDirectories);

            // In order to remove image's directory prefix. (e.g. "layers/layer1" --> "layer1"
            // Change oras working folder to inputFolder
            foreach (var filePath in filePathesToPush)
            {
                argument += $" \"{Path.GetRelativePath(inputFolder, filePath)}\"";
            }

            if (string.IsNullOrEmpty(argument))
            {
                throw new OverlayException(TemplateManagementErrorCode.ImageLayersNotFound, "No file to push.");
            }

            return new OCIOperationResult() { ClientResponse = await OrasExecutionAsync(string.Concat(command, argument), inputFolder) };
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

        private string LoadManifestContentFromCache(string digest)
        {
            try
            {
                var cachePath = Environment.GetEnvironmentVariable(Constants.OrasCacheEnvironmentVariableName);
                return File.ReadAllText(Path.Combine(cachePath, "blobs", "sha256", digest));
            }
            catch (Exception ex)
            {
                throw new OrasException(TemplateManagementErrorCode.OrasCacheFailed, "Read manifest from oras cache failed.", ex);
            }
        }
    }
}