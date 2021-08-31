// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EnsureThat;
using Microsoft.Azure.ContainerRegistry.Models;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Microsoft.Health.Fhir.TemplateManagement.Utilities;
using Newtonsoft.Json;
using SharpCompress.Common;
using SharpCompress.Writers;
using SharpCompress.Writers.Tar;

namespace Microsoft.Health.Fhir.TemplateManagement.Overlay
{
    public class OverlayOperator : IOverlayOperator
    {
        public OciFileLayer Extract(ArtifactBlob artifactLayer)
        {
            EnsureArg.IsNotNull(artifactLayer, nameof(artifactLayer));

            if (artifactLayer.Content == null)
            {
                return new OciFileLayer();
            }

            var artifacts = StreamUtility.DecompressTarGzStream(new MemoryStream(artifactLayer.Content));
            var metaBytes = artifacts.GetValueOrDefault(Constants.OverlayMetaJsonFile);
            OverlayMetadata metaJson = null;
            if (metaBytes != null)
            {
                try
                {
                    metaJson = JsonConvert.DeserializeObject<OverlayMetadata>(Encoding.UTF8.GetString(metaBytes));
                }
                catch (Exception ex)
                {
                    throw new OverlayException(TemplateManagementErrorCode.OverlayMetaJsonInvalid, $"{Constants.OverlayMetaJsonFile} file invalid.", ex);
                }
            }

            return new OciFileLayer()
            {
                Content = artifactLayer.Content,
                FileContent = artifacts,
                FileName = artifactLayer.FileName,
                SequenceNumber = metaJson?.SequenceNumber ?? -1,
                Digest = artifactLayer.Digest,
                Size = artifactLayer.Size,
            };
        }

        public List<OciFileLayer> Extract(List<ArtifactBlob> artifactLayers)
        {
            EnsureArg.IsNotNull(artifactLayers, nameof(artifactLayers));

            return artifactLayers.Select(Extract).ToList();
        }

        public List<ArtifactBlob> Sort(List<ArtifactBlob> imageLayers, ManifestWrapper manifest)
        {
            var sortedLayers = new List<ArtifactBlob>();
            ValidationUtility.ValidateManifest(manifest);
            foreach (var layerInfo in manifest.Layers)
            {
                var currentLayers = imageLayers.Where(layer => layer.Digest == layerInfo.Digest).ToList();
                if (!currentLayers.Any())
                {
                    throw new OverlayException(TemplateManagementErrorCode.ImageLayersNotFound, $"Layer {layerInfo.Digest} not found.");
                }

                sortedLayers.Add(currentLayers[0]);
            }

            return sortedLayers;
        }

        public OciFileLayer Merge(List<OciFileLayer> sortedLayers)
        {
            EnsureArg.IsNotNull(sortedLayers, nameof(sortedLayers));

            if (!sortedLayers.Any())
            {
                return null;
            }

            List<string> removedFiles = new List<string>();
            Dictionary<string, byte[]> mergedFiles = new Dictionary<string, byte[]> { };

            for (var sequenceNumber = sortedLayers.Count - 1; sequenceNumber >= 0; sequenceNumber--)
            {
                var oneLayer = sortedLayers[sequenceNumber];
                var fileContent = oneLayer.FileContent;
                foreach (var oneFile in fileContent)
                {
                    if (oneFile.Value == null)
                    {
                        removedFiles.Add(oneFile.Key);
                    }
                    else if (!removedFiles.Contains(oneFile.Key) && !mergedFiles.ContainsKey(oneFile.Key))
                    {
                        mergedFiles.Add(oneFile.Key, oneFile.Value);
                    }
                }
            }

            return new OciFileLayer() { FileContent = mergedFiles, FileName = "mergedLayer", SequenceNumber = sortedLayers.Count() };
        }

        public OciFileLayer GenerateDiffLayer(OciFileLayer fileLayer, OciFileLayer baseLayer = null)
        {
            EnsureArg.IsNotNull(fileLayer, nameof(fileLayer));
            EnsureArg.IsNotNull(fileLayer.FileContent, nameof(fileLayer.FileContent));

            int sequenceNumber = 1;
            var baseContents = new Dictionary<string, byte[]> { };
            if (baseLayer != null && baseLayer.FileContent != null)
            {
                baseContents = baseLayer.FileContent;
                sequenceNumber = baseLayer.SequenceNumber + 1;
            }

            var diffLayer = new OciFileLayer() { SequenceNumber = sequenceNumber, FileName = $"layer{sequenceNumber}.tar.gz" };

            var diffLayerFileDigest = new Dictionary<string, string> { };
            foreach (var oneFile in fileLayer.FileContent)
            {
                if (string.Equals(oneFile.Key, Constants.OverlayMetaJsonFile))
                {
                    continue;
                }

                var fileName = oneFile.Key.Replace("\\", "/");
                var oneDigest = StreamUtility.CalculateDigestFromSha256(oneFile.Value);

                diffLayerFileDigest.Add(fileName, oneDigest);
                if (baseContents.ContainsKey(fileName))
                {
                    if (!string.Equals(StreamUtility.CalculateDigestFromSha256(baseContents[fileName]), oneDigest))
                    {
                        diffLayer.FileContent.Add(fileName, oneFile.Value);
                    }

                    baseContents.Remove(fileName);
                }
                else
                {
                    diffLayer.FileContent.Add(fileName, oneFile.Value);
                }
            }

            foreach (var removedfile in baseContents)
            {
                if (string.Equals(removedfile.Key, Constants.OverlayMetaJsonFile))
                {
                    continue;
                }

                var fileName = Path.Combine(Path.GetDirectoryName(removedfile.Key), ".wh." + Path.GetFileName(removedfile.Key));
                diffLayer.FileContent.Add(fileName, Encoding.UTF8.GetBytes(string.Empty));
            }

            if (diffLayer.FileContent.Count != 0)
            {
                var metaJson = new OverlayMetadata() { SequenceNumber = sequenceNumber, FileDigests = diffLayerFileDigest };
                var metaContent = JsonConvert.SerializeObject(metaJson);
                diffLayer.FileContent.Add(Constants.OverlayMetaJsonFile, Encoding.UTF8.GetBytes(metaContent));
            }

            return diffLayer;
        }

        public ArtifactBlob Archive(OciFileLayer fileLayer)
        {
            EnsureArg.IsNotNull(fileLayer, nameof(fileLayer));

            if (fileLayer.FileContent.Count == 0)
            {
                return fileLayer;
            }

            var fileContents = fileLayer.FileContent;
            var resultStream = new MemoryStream();
            using (Stream stream = resultStream)
            using (var tarWriter = new TarWriter(stream, new TarWriterOptions(CompressionType.GZip, true)))
            {
                foreach (var eachFile in fileContents)
                {
                    tarWriter.Write(eachFile.Key, new MemoryStream(eachFile.Value));
                }
            }

            fileLayer.Content = resultStream.ToArray();
            return fileLayer;
        }

        public List<ArtifactBlob> Archive(List<OciFileLayer> fileLayers)
        {
            EnsureArg.IsNotNull(fileLayers, nameof(fileLayers));

            return fileLayers.Select(Archive).ToList();
        }
    }
}
