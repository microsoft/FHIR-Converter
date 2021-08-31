// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ContainerRegistry.Models;
using Microsoft.Health.Fhir.TemplateManagement.Client;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.TemplateManagement.UnitTests
{
    public class MockClient : IOciClient
    {
        private readonly Dictionary<string, List<byte[]>> _mockImages = new Dictionary<string, List<byte[]>>();
        private readonly Dictionary<string, ManifestWrapper> _mockManifests = new Dictionary<string, ManifestWrapper> { };
        private readonly Dictionary<string, byte[]> _mockBlobs = new Dictionary<string, byte[]> { };
        private readonly string _registry;
        private readonly string _realToken = "realToken";
        private readonly string _currentToken;

        public MockClient(string registry, string token)
        {
            _registry = registry;
            _currentToken = token;
        }

        public void PushLayers(ImageInfo imageInfo, List<byte[]> rawLayers)
        {
            if (!_mockImages.ContainsKey(imageInfo.ImageReference))
            {
                _mockImages.Add(imageInfo.ImageReference, rawLayers);
            }
            else
            {
                _mockImages[imageInfo.ImageReference] = rawLayers;
            }

            foreach (var layer in rawLayers)
            {
                var key = imageInfo.ImageName + ":" + CalculateHash(layer);
                if (!_mockBlobs.ContainsKey(key))
                {
                    _mockBlobs.Add(key, layer);
                }
            }
        }

        public void PushManifest(ImageInfo imageInfo, ManifestWrapper manifest)
        {
            if (!_mockManifests.ContainsKey(imageInfo.ImageReference))
            {
                _mockManifests.Add(imageInfo.ImageReference, manifest);
            }
            else
            {
                _mockManifests[imageInfo.ImageReference] = manifest;
            }

            var rawByte = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(manifest));
            var imageDigest = CalculateHash(rawByte);

            string digestKey = string.Format("{0}/{1}:{2}", imageInfo.Registry, imageInfo.ImageName, imageDigest);
            if (!_mockManifests.ContainsKey(digestKey))
            {
                _mockManifests.Add(digestKey, manifest);
            }
            else
            {
                _mockManifests[digestKey] = manifest;
            }
        }

        public Task<ManifestWrapper> GetManifestAsync(string imageName, string label, CancellationToken cancellationToken = default)
        {
            if (!string.Equals(_currentToken, _realToken))
            {
                throw new ContainerRegistryAuthenticationException(TemplateManagementErrorCode.RegistryUnauthorized, "unAuth");
            }

            string reference = _registry + "/" + imageName + ":" + label;
            if (_mockManifests.ContainsKey(reference))
            {
                return Task.FromResult(_mockManifests[reference]);
            }
            else
            {
                throw new ImageNotFoundException(TemplateManagementErrorCode.ImageNotFound, "Not Found");
            }
        }

        public Task<Stream> PullBlobAsStreamAcync(string imageName, string digest, CancellationToken cancellationToken = default)
        {
            if (!string.Equals(_currentToken, _realToken))
            {
                throw new ContainerRegistryAuthenticationException(TemplateManagementErrorCode.RegistryUnauthorized, "unAuth");
            }

            var key = imageName + ":" + digest;
            if (_mockBlobs.ContainsKey(key))
            {
                return Task.FromResult((Stream)new MemoryStream(_mockBlobs[key]));
            }
            else
            {
                throw new ImageNotFoundException(TemplateManagementErrorCode.ImageNotFound, "Not Found");
            }
        }

        public Task<ArtifactBlob> GetBlobAsync(string imageName, string digest, CancellationToken cancellationToken = default)
        {
            if (!string.Equals(_currentToken, _realToken))
            {
                throw new ContainerRegistryAuthenticationException(TemplateManagementErrorCode.RegistryUnauthorized, "unAuth");
            }

            var key = imageName + ":" + digest;
            if (_mockBlobs.ContainsKey(key))
            {
                return Task.FromResult(new ArtifactBlob() { Content = _mockBlobs[key] });
            }
            else
            {
                throw new ImageNotFoundException(TemplateManagementErrorCode.ImageNotFound, "Not Found");
            }
        }

        public Task<List<byte[]>> PullImageAsync(ImageInfo imageInfo, CancellationToken cancellationToken = default)
        {
            if (!string.Equals(_currentToken, _realToken))
            {
                throw new ContainerRegistryAuthenticationException(TemplateManagementErrorCode.RegistryUnauthorized, "unAuth");
            }

            if (_mockImages.ContainsKey(imageInfo.ImageReference))
            {
                return Task.FromResult(_mockImages[imageInfo.ImageReference]);
            }
            else
            {
                throw new ImageNotFoundException(TemplateManagementErrorCode.ImageNotFound, "Not Found");
            }
        }

        private string CalculateHash(byte[] rawBytes)
        {
            using SHA256 mySHA256 = SHA256.Create();
            string hashedValue = "sha256:";
            byte[] hashData = mySHA256.ComputeHash(new MemoryStream(rawBytes));
            string[] hashedStrings = hashData.Select(x => string.Format("{0,2:x2}", x)).ToArray();
            hashedValue += string.Join(string.Empty, hashedStrings);
            return hashedValue;
        }

        public Task<ArtifactImage> PullImageAsync(string imageName, string reference, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> PushImageAsync(string imageName, string reference, ArtifactImage image, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}
