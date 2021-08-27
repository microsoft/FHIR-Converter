// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ContainerRegistry.Models;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.TemplateManagement.Client
{
    public class OrasCacheClient
    {
        private readonly string _blobCacheFolder;

        // Format of digest is: <algorithm>:<hex>
        // e.g. sha256:d377125165eb6d770f344429a7a55379d4028774aebe267fe620cd1fcd2daab7
        private readonly Regex _digestRegex = new Regex("(?<algorithm>[A-Za-z][A-Za-z0-9]*([+.-_][A-Za-z][A-Za-z0-9]*)*):(?<hex>[0-9a-fA-F]{32,})");

        public OrasCacheClient()
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(Constants.OrasCacheEnvironmentVariableName)))
            {
                Environment.SetEnvironmentVariable(Constants.OrasCacheEnvironmentVariableName, Constants.DefaultOrasCacheEnvironmentVariable);
            }

            _blobCacheFolder = Path.Combine(Environment.GetEnvironmentVariable(Constants.OrasCacheEnvironmentVariableName), "blobs", "sha256");
        }

        public async Task<ManifestWrapper> GetManifestAsync(string name, string reference, CancellationToken cancellationToken = default)
        {
            string manifest;
            try
            {
                manifest = await File.ReadAllTextAsync(Path.Combine(_blobCacheFolder, reference));
            }
            catch (Exception ex)
            {
                throw new OCIClientException(TemplateManagementErrorCode.CacheManifestFailed, "Read manifest from oras cache failed.", ex);
            }

            try
            {

                return JsonConvert.DeserializeObject<ManifestWrapper>(manifest);
            }
            catch (Exception ex)
            {
                throw new OCIClientException(TemplateManagementErrorCode.CacheManifestFailed, "Deserialize manifest failed.", ex);
            }
        }

        public async Task<ArtifactBlob> GetBlobAsync(string name, string reference, CancellationToken cancellationToken = default)
        {
            var blob = new ArtifactBlob();
            try
            {
                blob.Content = await File.ReadAllBytesAsync(Path.Combine(_blobCacheFolder, GetImageDigest(reference).Item2));
                blob.Digest = reference;
                blob.Size = blob.Content.Length;
                return blob;
            }
            catch (Exception ex)
            {
                throw new OCIClientException(TemplateManagementErrorCode.CacheBlobFailed, "Read blob from oras cache failed.", ex);
            }
        }

        public Tuple<string, string> GetImageDigest(string input)
        {
            try
            {
                return Tuple.Create(_digestRegex.Matches(input)[0].Groups["algorithm"].ToString(), _digestRegex.Matches(input)[0].Groups["hex"].ToString());
            }
            catch (Exception ex)
            {
                throw new OCIClientException(TemplateManagementErrorCode.OrasProcessFailed, "Return image digest failed.", ex);
            }
        }
    }
}
