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
        public OCIFileLayer ExtractOCIFileLayer(OCIArtifactLayer artifactLayer)
        {
            EnsureArg.IsNotNull(artifactLayer, nameof(artifactLayer));

            if (artifactLayer.Content == null)
            {
                return new OCIFileLayer();
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

            return new OCIFileLayer()
            {
                Content = artifactLayer.Content,
                FileContent = artifacts,
                FileName = artifactLayer.FileName,
                SequenceNumber = metaJson?.SequenceNumber ?? -1,
                Digest = artifactLayer.Digest,
                Size = artifactLayer.Size,
            };
        }

        public List<OCIFileLayer> ExtractOCIFileLayers(List<OCIArtifactLayer> artifactLayers)
        {
            EnsureArg.IsNotNull(artifactLayers, nameof(artifactLayers));

            return artifactLayers.Select(ExtractOCIFileLayer).ToList();
        }

        public List<OCIFileLayer> SortOCIFileLayersBySequenceNumber(List<OCIFileLayer> fileLayers)
        {
            EnsureArg.IsNotNull(fileLayers, nameof(fileLayers));

            var sortedLayers = fileLayers.OrderBy(layer => layer.SequenceNumber <= -1 ? int.MaxValue : layer.SequenceNumber).ToList();
            ValidateSortedLayersBySequenceNumber(sortedLayers);
            return sortedLayers;
        }

        public List<OCIFileLayer> SortOCIFileLayersByManifest(List<OCIFileLayer> fileLayers, ManifestWrapper manifest)
        {
            EnsureArg.IsNotNull(fileLayers, nameof(fileLayers));
            EnsureArg.IsNotNull(manifest, nameof(manifest));

            List<OCIFileLayer> sortedLayers = new List<OCIFileLayer>();
            foreach (var layerInfo in manifest.Layers)
            {
                var currentLayer = fileLayers.Where(layer => layer.Digest == layerInfo.Digest).ToList();
                if (currentLayer == null)
                {
                    throw new OverlayException(TemplateManagementErrorCode.SortLayersFailed, "Layers in manifest not found.");
                }

                if (currentLayer.Count > 1)
                {
                    throw new OverlayException(TemplateManagementErrorCode.SortLayersFailed, "Duplicated layers.");
                }

                sortedLayers.Add(currentLayer[0]);
            }

            return sortedLayers;
        }

        public OCIFileLayer MergeOCIFileLayers(List<OCIFileLayer> sortedLayers)
        {
            if (sortedLayers == null || !sortedLayers.Any())
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

            return new OCIFileLayer() { FileContent = mergedFiles, FileName = "mergedLayer", SequenceNumber = sortedLayers.Count() };
        }

        public OCIFileLayer GenerateDiffLayer(OCIFileLayer fileLayer, OCIFileLayer baseLayer = null)
        {
            EnsureArg.IsNotNull(fileLayer, nameof(fileLayer));

            int sequenceNumber = 1;
            var baseContents = new Dictionary<string, byte[]> { };
            if (baseLayer != null && baseLayer.FileContent != null)
            {
                baseContents = baseLayer.FileContent;
                sequenceNumber = baseLayer.SequenceNumber + 1;
            }

            var diffLayer = new OCIFileLayer() { SequenceNumber = sequenceNumber, FileName = $"layer{sequenceNumber}.tar.gz" };
            if (fileLayer.FileContent == null)
            {
                return diffLayer;
            }

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

        public OCIArtifactLayer ArchiveOCIFileLayer(OCIFileLayer fileLayer)
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

        public List<OCIArtifactLayer> ArchiveOCIFileLayers(List<OCIFileLayer> fileLayers)
        {
            EnsureArg.IsNotNull(fileLayers, nameof(fileLayers));

            return fileLayers.Select(ArchiveOCIFileLayer).ToList();
        }

        private void ValidateSortedLayersBySequenceNumber(List<OCIFileLayer> sortedLayers)
        {
            for (var index = 1; index <= sortedLayers.Count(); index++)
            {
                if (index == sortedLayers.Count() && sortedLayers[index - 1].SequenceNumber == -1)
                {
                    continue;
                }

                if (sortedLayers[index - 1].SequenceNumber != index)
                {
                    throw new OverlayException(TemplateManagementErrorCode.SortLayersFailed, "Some layer's sequence number are invalid. Layers could not be sorted and merged.");
                }
            }
        }
    }
}
