// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ContainerRegistry.Models;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.TemplateManagement.Client
{
    /// <summary>
    /// Get manifest and blob from ORAS cache.
    /// </summary>
    public partial class OrasClient : IOciClient
    {
        private string _blobCacheFolder;

        private async Task<ManifestWrapper> GetManifestFromCacheAsync(string digest, CancellationToken cancellationToken = default)
        {
            var manifest = await File.ReadAllTextAsync(Path.Combine(_blobCacheFolder, Digest.GetDigest(digest)[0].Hex), cancellationToken);
            return JsonConvert.DeserializeObject<ManifestWrapper>(manifest);
        }

        private async Task<ArtifactBlob> GetBlobFromCacheAsync(string digest, CancellationToken cancellationToken = default)
        {
            var blob = new ArtifactBlob();
            await blob.ReadFromFileAsync(Path.Combine(_blobCacheFolder, Digest.GetDigest(digest)[0].Hex));
            return blob;
        }

        private void InitCacheEnviroment()
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(Constants.OrasCacheEnvironmentVariableName)))
            {
                Environment.SetEnvironmentVariable(Constants.OrasCacheEnvironmentVariableName, Constants.DefaultOrasCacheEnvironmentVariable);
            }

            _blobCacheFolder = Path.Combine(Environment.GetEnvironmentVariable(Constants.OrasCacheEnvironmentVariableName), "blobs", "sha256");
        }
    }
}
