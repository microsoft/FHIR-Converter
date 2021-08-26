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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ContainerRegistry.Models;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Microsoft.Health.Fhir.TemplateManagement.Utilities;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.TemplateManagement.Client
{
    public class OrasClient : IOCIClient
    {
        private readonly string _workingFolder = ".orasWorkingFolder";
        private readonly string _url;

        // Format of digest is: <algorithm>:<hex>
        // e.g. sha256:d377125165eb6d770f344429a7a55379d4028774aebe267fe620cd1fcd2daab7
        private readonly Regex _digestRegex = new Regex("(?<algorithm>[A-Za-z][A-Za-z0-9]*([+.-_][A-Za-z][A-Za-z0-9]*)*):(?<hex>[0-9a-fA-F]{32,})");

        public OrasClient(string url)
        {
            InitClientEnvironment();
            _url = url;
        }

        public async Task<ArtifactImage> PullImageAsync(string imageName, string reference, CancellationToken cancellationToken = default)
        {
            ClearClientWorkingFolder();
            try
            {
                string imageReference = string.Format("{0}/{1}:{2}", _url, imageName, reference);
                string command = $"pull  \"{imageReference}\" -o \"{_workingFolder}\"";

                string output = await OrasExecutionAsync(command, null);
                string digest = GetImageDigest(output);

                // Oras will create output folder if not existed.
                if (!Directory.Exists(_workingFolder) || !Directory.EnumerateFileSystemEntries(_workingFolder).Any())
                {
                    throw new OCIClientException(TemplateManagementErrorCode.OrasProcessFailed, "Image not found, pull image failed or image is empty.");
                }

                string manifestContent;
                try
                {
                    manifestContent = LoadManifestContentFromCache(digest);
                }
                catch (Exception ex)
                {
                    ClearClientWorkingFolder();
                    throw new OCIClientException(TemplateManagementErrorCode.CacheManifestFailed, "Read manifest from oras cache failed.", ex);
                }

                var result = new ArtifactImage();
                var layersPath = Directory.EnumerateFiles(_workingFolder, "*.tar.gz", SearchOption.AllDirectories);

                foreach (var tarGzFile in layersPath)
                {
                    var artifactLayer = new Models.ArtifactBlob();
                    artifactLayer.ReadFromFile(tarGzFile);
                    if (artifactLayer.Content != null)
                    {
                        result.Blobs.Add(artifactLayer);
                    }
                }

                var imageInfo = ImageInfo.CreateFromImageReference(imageReference);
                imageInfo.Digest = digest;
                result.Info = imageInfo;

                try
                {
                    result.Manifest = JsonConvert.DeserializeObject<ManifestWrapper>(manifestContent);
                    ClearClientWorkingFolder();
                    return result;
                }
                catch (Exception ex)
                {
                    throw new OCIClientException(TemplateManagementErrorCode.CacheManifestFailed, "Deserialize manifest failed.", ex);
                }
            }
            catch
            {
                ClearClientWorkingFolder();
                throw;
            }
        }

        public async Task<ArtifactImage> PushImageAsync(string imageName, string reference, ArtifactImage artifactImage, CancellationToken cancellationToken = default)
        {
            ClearClientWorkingFolder();
            try
            {
                string imageReference = string.Format("{0}/{1}:{2}", _url, imageName, reference);
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
                var imageInfo = ImageInfo.CreateFromImageReference(imageReference);
                imageInfo.Digest = GetImageDigest(output);
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

        private void InitClientEnvironment()
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

        private string GetImageDigest(string input)
        {
            try
            {
                return _digestRegex.Matches(input)[0].Groups["hex"].ToString();
            }
            catch (Exception ex)
            {
                throw new OCIClientException(TemplateManagementErrorCode.OrasProcessFailed, "Return image digest failed.", ex);
            }
        }

        public Task<ManifestWrapper> GetManifestAsync(string imageName, string reference, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<ArtifactBlob> GetBlobAsync(string imageName, string reference, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}