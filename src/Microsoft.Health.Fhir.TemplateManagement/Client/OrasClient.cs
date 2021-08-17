// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Azure.ContainerRegistry.Models;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.TemplateManagement.Client
{
    public class OrasClient : IOCIClient
    {
        private readonly string _imageReference;
        private readonly Regex _digestRegex = new Regex("(?<algorithm>[A-Za-z][A-Za-z0-9]*([+.-_][A-Za-z][A-Za-z0-9]*)*):(?<hex>[0-9a-fA-F]{32,})");

        public OrasClient(string imageReference)
        {
            EnsureArg.IsNotNull(imageReference, nameof(imageReference));

            _imageReference = imageReference;
        }

        public async Task<ManifestWrapper> PullImageAsync(string outputFolder)
        {
            if (Directory.Exists(outputFolder) && Directory.GetFileSystemEntries(outputFolder).Length != 0)
            {
                throw new OCIClientException(TemplateManagementErrorCode.OrasProcessFailed, "The output folder is not empty.");
            }

            string command = $"pull  \"{_imageReference}\" -o \"{outputFolder}\"";
            string output = await OrasExecutionAsync(command, Directory.GetCurrentDirectory());
            string digest;
            try
            {
                digest = _digestRegex.Matches(output)[0].Groups["hex"].ToString();
            }
            catch (Exception ex)
            {
                throw new OCIClientException(TemplateManagementErrorCode.OrasProcessFailed, "Return image digest failed.", ex);
            }

            // Oras will create output folder if not existed.
            // Therefore, the output folder should exist if pull succeed.
            if (!Directory.Exists(outputFolder))
            {
                throw new OCIClientException(TemplateManagementErrorCode.OrasProcessFailed, "Pull image failed or image is empty.");
            }

            try
            {
                string manifestContent = LoadManifestContentFromCache(digest);
                var manifest = JsonConvert.DeserializeObject<ManifestWrapper>(manifestContent);
                File.WriteAllText(Path.Combine(outputFolder, Constants.ManifestFileName), LoadManifestContentFromCache(digest));
                return manifest;
            }
            catch (Exception ex)
            {
                throw new OCIClientException(TemplateManagementErrorCode.OrasCacheFailed, "Read manifest from oras cache failed.", ex);
            }
        }

        public async Task PushImageAsync(string inputFolder)
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

            await OrasExecutionAsync(string.Concat(command, argument), inputFolder);
        }

        public static async Task<string> OrasExecutionAsync(string command, string orasWorkingDirectory)
        {
            TaskCompletionSource<bool> eventHandled = new TaskCompletionSource<bool>();

            string orasFileName;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                orasFileName = Constants.OrasFileForWindows;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                orasFileName = Constants.OrasFileForOSX;
            }
            else
            {
                throw new TemplateManagementException("Operation system is not supported");
            }

            // oras file is in the same directory with our tool.
            var orasFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), orasFileName);
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo(orasFilePath),
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
                throw new OCIClientException(TemplateManagementErrorCode.OrasProcessFailed, "Oras process failed", ex);
            }

            StreamReader errStreamReader = process.StandardError;
            StreamReader outputStreamReader = process.StandardOutput;
            await Task.WhenAny(eventHandled.Task, Task.Delay(Constants.TimeOutMilliseconds));
            if (process.HasExited)
            {
                string error = errStreamReader.ReadToEnd();
                if (!string.IsNullOrEmpty(error) || process.ExitCode != 0)
                {
                    throw new OCIClientException(TemplateManagementErrorCode.OrasProcessFailed, error);
                }

                return outputStreamReader.ReadToEnd();
            }
            else
            {
                try
                {
                    process.Kill();
                }
                catch (Exception ex)
                {
                    throw new OCIClientException(TemplateManagementErrorCode.OrasTimeOut, "Oras request timeout. Fail to kill oras process.", ex);
                }

                throw new OCIClientException(TemplateManagementErrorCode.OrasTimeOut, "Oras request timeout");
            }
        }

        public void InitClientEnvironment()
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(Constants.OrasCacheEnvironmentVariableName)))
            {
                Environment.SetEnvironmentVariable(Constants.OrasCacheEnvironmentVariableName, Constants.DefaultOrasCacheEnvironmentVariable);
            }
        }

        private string LoadManifestContentFromCache(string digest)
        {
            var cachePath = Environment.GetEnvironmentVariable(Constants.OrasCacheEnvironmentVariableName);
            return File.ReadAllText(Path.Combine(cachePath, "blobs", "sha256", digest));
        }
    }
}