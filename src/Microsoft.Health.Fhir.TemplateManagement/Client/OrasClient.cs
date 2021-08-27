// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Microsoft.Health.Fhir.TemplateManagement.Utilities;

namespace Microsoft.Health.Fhir.TemplateManagement.Client
{
    public class OrasClient : OrasCacheClient, IOCIClient
    {
        private readonly string _workingFolder = ".orasWorkingFolder";
        private readonly string _registry;

        public OrasClient(string registry)
        {
            _registry = registry;
        }

        public async Task<ArtifactImage> PullImageAsync(string name, string reference, CancellationToken cancellationToken = default)
        {
            ClearClientWorkingFolder();
            try
            {
                string imageReference = string.Format("{0}/{1}:{2}", _registry, name, reference);
                string command = $"pull  \"{imageReference}\" -o \"{_workingFolder}\"";

                string output = await OrasExecutionAsync(command, null);
                Tuple<string, string> digest = GetImageDigest(output);

                // Oras will create output folder if not existed.
                if (!Directory.Exists(_workingFolder) || !Directory.EnumerateFileSystemEntries(_workingFolder).Any())
                {
                    throw new OCIClientException(TemplateManagementErrorCode.OrasProcessFailed, "Image not found, pull image failed or image is empty.");
                }

                var artifactImage = new ArtifactImage
                {
                    Info = ImageInfo.CreateFromImageReference(imageReference),
                };
                artifactImage.Info.Digest = string.Concat(digest.Item1, ":", digest.Item2);
                artifactImage.Manifest = await GetManifestAsync(name, digest.Item2, cancellationToken);

                ValidationUtility.ValidateManifest(artifactImage.Manifest);

                foreach (var layerInfo in artifactImage.Manifest.Layers)
                {
                    artifactImage.Blobs.Add(await GetBlobAsync(name, layerInfo.Digest, cancellationToken));
                }

                return artifactImage;
            }
            catch
            {
                ClearClientWorkingFolder();
                throw;
            }
        }

        public async Task<ArtifactImage> PushImageAsync(string name, string reference, ArtifactImage artifactImage, CancellationToken cancellationToken = default)
        {
            ClearClientWorkingFolder();
            try
            {
                string imageReference = string.Format("{0}/{1}:{2}", _registry, name, reference);
                List<string> fileNameList = new List<string>();
                string command = $"push \"{imageReference}\"";

                foreach (var layer in artifactImage.Blobs)
                {
                    if (layer.Content != null)
                    {
                        layer.WriteToFile(Path.Combine(_workingFolder, layer.FileName));
                        fileNameList.Add(layer.FileName);
                    }
                }

                if (fileNameList.Count == 0)
                {
                    throw new OverlayException(TemplateManagementErrorCode.ImageLayersNotFound, "No file to push.");
                }

                string argument = string.Join(" ", fileNameList);

                // In order to remove image's directory prefix. (e.g. "layers/layer1" --> "layer1")
                // Change oras working folder to inputFolder
                var output = await OrasExecutionAsync(string.Concat(command, argument), _workingFolder);
                Tuple<string, string> digest = GetImageDigest(output);

                var imageInfo = ImageInfo.CreateFromImageReference(imageReference);
                imageInfo.Digest = string.Concat(digest.Item1, ":", digest.Item2);
                artifactImage.Info = imageInfo;

                return artifactImage;
            }
            catch
            {
                ClearClientWorkingFolder();
                throw;
            }
        }

        public static async Task<string> OrasExecutionAsync(string command, string orasWorkingDirectory = null)
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
            if (orasWorkingDirectory != null)
            {
                process.StartInfo.WorkingDirectory = orasWorkingDirectory;
            }

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
                    throw new OCIClientException(TemplateManagementErrorCode.OrasProcessFailed, "Oras process failed." + error);
                }

                return outputStreamReader.ReadToEnd();
            }

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

        private void ClearClientWorkingFolder()
        {
            IoUtility.ClearFolder(_workingFolder);
        }

        

    }
}