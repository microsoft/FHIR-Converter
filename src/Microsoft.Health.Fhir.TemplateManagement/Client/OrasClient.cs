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
using EnsureThat;
using Microsoft.Azure.ContainerRegistry.Models;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Microsoft.Health.Fhir.TemplateManagement.Utilities;

namespace Microsoft.Health.Fhir.TemplateManagement.Client
{
    /// <summary>
    /// Partial class.
    /// </summary>
    public partial class OrasClient : IOciClient
    {
        private readonly string _imageFolder;
        private readonly string _registry;

        public OrasClient(string registry, string imageFolder)
        {
            _imageFolder = imageFolder;
            _registry = registry;
            InitCacheEnviroment();
        }

        public async Task<ArtifactImage> PullImageAsync(string name, string reference, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNull(name, nameof(name));
            EnsureArg.IsNotNull(reference, nameof(reference));

            DirectoryHelper.ClearFolder(_imageFolder);

            string imageReference;
            if (Digest.IsDigest(reference))
            {
                imageReference = string.Format("{0}/{1}@{2}", _registry, name, reference);
            }
            else
            {
                imageReference = string.Format("{0}/{1}:{2}", _registry, name, reference);
            }

            string command = $"pull  \"{imageReference}\" -o \"{_imageFolder}\"";

            string output = await OrasExecutionAsync(command, null);
            var digest = GetImageDigest(output);

            // Oras will create output folder if not existed.
            if (!Directory.Exists(_imageFolder) || !Directory.EnumerateFileSystemEntries(_imageFolder).Any())
            {
                throw new OciClientException(TemplateManagementErrorCode.OrasProcessFailed, "Image not found, pull image failed or image is empty.");
            }

            var artifactImage = new ArtifactImage
            {
                ImageDigest = digest.Value,
            };
            try
            {
                artifactImage.Manifest = await GetManifestFromCacheAsync(digest.Value, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new OciClientException(TemplateManagementErrorCode.OrasCacheManifestFailed, "Get manifest from oras cache failed.", ex);
            }

            try
            {
                foreach (var layerInfo in artifactImage.Manifest.Layers)
                {
                    artifactImage.Blobs.Add(await GetBlobFromCacheAsync(layerInfo.Digest, cancellationToken));
                }
            }
            catch (Exception ex)
            {
                throw new OciClientException(TemplateManagementErrorCode.OrasCacheBlobFailed, "Get blobs from oras cache failed.", ex);
            }

            return artifactImage;
        }

        public async Task<string> PushImageAsync(string name, string tag, ArtifactImage artifactImage, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNull(name, nameof(name));
            EnsureArg.IsNotNull(tag, nameof(tag));

            DirectoryHelper.ClearFolder(_imageFolder);

            string imageReference = string.Format("{0}/{1}:{2}", _registry, name, tag);
            List<string> fileNameList = new List<string>();
            string command = $"push \"{imageReference}\"";

            foreach (var layer in artifactImage.Blobs)
            {
                if (layer.Content != null)
                {
                    await layer.WriteToFileAsync(Path.Combine(_imageFolder, layer.FileName));
                    fileNameList.Add($"\"{layer.FileName}\"");
                }
            }

            if (fileNameList.Count == 0)
            {
                throw new OverlayException(TemplateManagementErrorCode.ImageLayersNotFound, "No file to push.");
            }

            string argument = string.Join(" ", fileNameList);

            // In order to remove image's directory prefix. (e.g. "layers/layer1" --> "layer1")
            // Change oras working folder to inputFolder
            var output = await OrasExecutionAsync(string.Concat(command, " ", argument), _imageFolder);
            var digest = GetImageDigest(output);
            return digest.Value;
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
                throw new OciClientException(TemplateManagementErrorCode.OrasProcessFailed, "Oras process failed", ex);
            }

            StreamReader errStreamReader = process.StandardError;
            StreamReader outputStreamReader = process.StandardOutput;
            await Task.WhenAny(eventHandled.Task, Task.Delay(Constants.TimeOutMilliseconds));
            if (process.HasExited)
            {
                if (process.ExitCode != 0)
                {
                    throw new OciClientException(TemplateManagementErrorCode.OrasProcessFailed, $"Oras process failed. {errStreamReader.ReadToEnd()}");
                }

                return outputStreamReader.ReadToEnd();
            }

            try
            {
                process.Kill();
            }
            catch (Exception ex)
            {
                throw new OciClientException(TemplateManagementErrorCode.OrasTimeOut, "Oras request timeout. Fail to kill oras process.", ex);
            }

            throw new OciClientException(TemplateManagementErrorCode.OrasTimeOut, "Oras request timeout");
        }

        private Digest GetImageDigest(string input)
        {
            var digests = Digest.GetDigest(input);
            if (digests.Count == 0)
            {
                throw new OciClientException(TemplateManagementErrorCode.OrasProcessFailed, "Failed to parse image digest.");
            }

            return digests[0];
        }

        public Task<ManifestWrapper> GetManifestAsync(string name, string digest, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<ArtifactBlob> GetBlobAsync(string name, string digest, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}