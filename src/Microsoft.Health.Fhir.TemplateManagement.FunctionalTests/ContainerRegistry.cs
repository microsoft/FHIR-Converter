// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ContainerRegistry;
using Microsoft.Azure.ContainerRegistry.Models;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Microsoft.Rest;

namespace Microsoft.Health.Fhir.TemplateManagement.FunctionalTests
{
    public class ContainerRegistry
    {
        public ContainerRegistryInfo GetTestContainerRegistryInfo()
        {
            var containerRegistry = new ContainerRegistryInfo
            {
                ContainerRegistryServer = Environment.GetEnvironmentVariable("TestContainerRegistryServer"),
                ContainerRegistryUsername = Environment.GetEnvironmentVariable("TestContainerRegistryServer")?.Split('.')[0],
                ContainerRegistryPassword = Environment.GetEnvironmentVariable("TestContainerRegistryPassword"),
            };
            if (containerRegistry == null || string.IsNullOrEmpty(containerRegistry.ContainerRegistryServer))
            {
                return null;
            }

            return containerRegistry;
        }

        public async Task GenerateTemplateImageAsync(ContainerRegistryInfo registry, string imageReference, List<string> templateFiles)
        {
            ImageInfo imageInfo = ImageInfo.CreateFromImageReference(imageReference);

            if (imageInfo.Registry != registry.ContainerRegistryServer)
            {
                return;
            }

            await PushTemplateCollection(registry, imageInfo.ImageName, imageInfo.Tag, templateFiles);
        }

        private async Task PushTemplateCollection(ContainerRegistryInfo registry, string repository, string tag, List<string> templateFiles)
        {
            AzureContainerRegistryClient acrClient = new AzureContainerRegistryClient(registry.ContainerRegistryServer, new AcrBasicToken(registry));

            int schemaV2 = 2;
            string mediatypeV2Manifest = "application/vnd.docker.distribution.manifest.v2+json";
            string mediatypeV1Manifest = "application/vnd.oci.image.config.v1+json";
            string emptyConfigStr = "{}";

            // Upload config blob
            byte[] originalConfigBytes = Encoding.UTF8.GetBytes(emptyConfigStr);
            using var originalConfigStream = new MemoryStream(originalConfigBytes);
            string originalConfigDigest = ComputeDigest(originalConfigStream);
            await UploadBlob(acrClient, originalConfigStream, repository, originalConfigDigest);

            // Upload memory blob

            List<Descriptor> layers = new List<Descriptor>();

            foreach (var templateFile in templateFiles)
            {
                using FileStream fileStream = File.OpenRead(templateFile);
                using MemoryStream byteStream = new MemoryStream();
                fileStream.CopyTo(byteStream);
                var blobLength = byteStream.Length;
                string blobDigest = ComputeDigest(byteStream);
                await UploadBlob(acrClient, byteStream, repository, blobDigest);
                layers.Add(new Descriptor("application/vnd.oci.image.layer.v1.tar", blobLength, blobDigest));
            }

            // Push manifest
            var v2Manifest = new V2Manifest(schemaV2, mediatypeV2Manifest, new Descriptor(mediatypeV1Manifest, originalConfigBytes.Length, originalConfigDigest), layers);
            await acrClient.Manifests.CreateAsync(repository, tag, v2Manifest);
        }

        private async Task UploadBlob(AzureContainerRegistryClient acrClient, Stream stream, string repository, string digest)
        {
            stream.Position = 0;
            var uploadInfo = await acrClient.Blob.StartUploadAsync(repository);
            var uploadedLayer = await acrClient.Blob.UploadAsync(stream, uploadInfo.Location);
            await acrClient.Blob.EndUploadAsync(digest, uploadedLayer.Location);
        }

        private static string ComputeDigest(Stream s)
        {
            s.Position = 0;
            StringBuilder sb = new StringBuilder();

            using (var hash = SHA256.Create())
            {
                byte[] result = hash.ComputeHash(s);
                foreach (byte b in result)
                {
                    sb.Append(b.ToString("x2"));
                }
            }

            return "sha256:" + sb.ToString();
        }

        internal class AcrBasicToken : ServiceClientCredentials
        {
            private readonly ContainerRegistryInfo _registry;

            public AcrBasicToken(ContainerRegistryInfo registry)
            {
                _registry = registry;
            }

            public override void InitializeServiceClient<T>(ServiceClient<T> client)
            {
                base.InitializeServiceClient(client);
            }

            public override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var basicToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_registry.ContainerRegistryUsername}:{_registry.ContainerRegistryPassword}"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basicToken);
                return base.ProcessHttpRequestAsync(request, cancellationToken);
            }
        }
    }
}