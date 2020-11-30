// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
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
            if (artifactLayer == null)
            {
                return null;
            }

            var artifacts = StreamUtility.DecompressTarGzStream(new MemoryStream(artifactLayer.Content));
            var metaBytes = artifacts.GetValueOrDefault(Constants.OverlayMetadataFile);
            OverlayMetadata metaJson = null;
            if (metaBytes != null)
            {
                try
                {
                    metaJson = JsonConvert.DeserializeObject<OverlayMetadata>(Encoding.UTF8.GetString(metaBytes));
                }
                catch (Exception ex)
                {
                    throw new OverlayException(TemplateManagementErrorCode.OverlayMetaJsonNotFound, "overlayMeta.json file invalid", ex);
                }
            }

            artifacts.Remove(Constants.OverlayMetadataFile);
            return new OCIFileLayer() { Content = artifactLayer.Content, FileContent = artifacts, FileName = artifactLayer.FileName, SequenceNumber = metaJson == null ? -1 : metaJson.SequenceNumber };
        }

        public List<OCIFileLayer> ExtractOCIFileLayers(List<OCIArtifactLayer> artifactLayers)
        {
            if (artifactLayers == null)
            {
                return null;
            }

            var result = new List<OCIFileLayer>();
            foreach (var layer in artifactLayers)
            {
                result.Add(ExtractOCIFileLayer(layer));
            }

            return result;
        }

        public List<OCIFileLayer> SortOCIFileLayersBySequenceNumber(List<OCIFileLayer> fileLayers)
        {
            if (fileLayers == null)
            {
                return null;
            }

            return fileLayers.OrderBy(layer => layer.SequenceNumber).ToList();
        }

        public OCIFileLayer MergeOCIFileLayers(List<OCIFileLayer> sortedLayers)
        {
            if (sortedLayers == null)
            {
                return null;
            }

            List<string> removedFiles = new List<string>();
            List<string> currentLayerRemovedFiles = new List<string>();
            Dictionary<string, byte[]> mergedFiles = new Dictionary<string, byte[]> { };

            for (var sequenceNumber = sortedLayers.Count - 1; sequenceNumber >= 0; sequenceNumber--)
            {
                var oneLayer = sortedLayers[sequenceNumber];
                var fileContent = oneLayer.FileContent;
                foreach (var oneFile in fileContent)
                {
                    if (oneFile.Value == null)
                    {
                        currentLayerRemovedFiles.Add(oneFile.Key);
                    }
                    else if (!removedFiles.Contains(oneFile.Key) && !mergedFiles.ContainsKey(oneFile.Key))
                    {
                        mergedFiles.Add(oneFile.Key, oneFile.Value);
                    }
                }

                removedFiles.AddRange(currentLayerRemovedFiles);
                currentLayerRemovedFiles.Clear();
            }

            return new OCIFileLayer() { FileContent = mergedFiles, FileName = "mergedLayer", SequenceNumber = sortedLayers.Count() };
        }

        public OCIFileLayer GenerateDiffLayer(OCIFileLayer fileLayer, OCIFileLayer snapshotLayer)
        {
            Dictionary<string, byte[]> diffLayerFiles = new Dictionary<string, byte[]> { };
            var snapshotContents = new Dictionary<string, byte[]> { };
            var fileContents = fileLayer.FileContent;

            bool editFlag = false;

            int buildNumber = 1;
            if (snapshotLayer != null)
            {
                snapshotContents = snapshotLayer.FileContent;
                buildNumber = snapshotLayer.SequenceNumber + 1;
            }

            var diffLayerFileDigest = new Dictionary<string, string> { };
            foreach (var oneFile in fileContents)
            {
                var fileName = oneFile.Key.Replace("\\", "/");
                var oneDigest = StreamUtility.CalculateDigestFromSha256(oneFile.Value);

                diffLayerFileDigest.Add(fileName, oneDigest);
                if (snapshotContents.ContainsKey(fileName))
                {
                    if (!string.Equals(StreamUtility.CalculateDigestFromSha256(snapshotContents[fileName]), oneDigest))
                    {
                        diffLayerFiles.Add(fileName, oneFile.Value);
                        editFlag = true;
                    }

                    snapshotContents.Remove(fileName);
                }
                else
                {
                    diffLayerFiles.Add(fileName, oneFile.Value);
                    editFlag = true;
                }
            }

            foreach (var removedfile in snapshotContents)
            {
                var fileName = Path.Combine(Path.GetDirectoryName(removedfile.Key), ".wh." + Path.GetFileName(removedfile.Key));
                diffLayerFiles.Add(fileName, Encoding.UTF8.GetBytes(string.Empty));
                editFlag = true;
            }

            if (editFlag)
            {
                var metaJson = new OverlayMetadata() { SequenceNumber = buildNumber, FileDigests = diffLayerFileDigest };
                var metaContent = JsonConvert.SerializeObject(metaJson);
                diffLayerFiles.Add(Constants.OverlayMetadataFile, Encoding.UTF8.GetBytes(metaContent));
                return new OCIFileLayer() { FileContent = diffLayerFiles, SequenceNumber = buildNumber, FileName = $"layer{buildNumber}.tar.gz" };
            }

            return null;
        }

        public OCIArtifactLayer ArchiveOCIFileLayer(OCIFileLayer fileLayer)
        {
            if (fileLayer == null)
            {
                return null;
            }

            var fileContents = fileLayer.FileContent;
            using (Stream stream = File.Open("temp.tar.gz", FileMode.OpenOrCreate, FileAccess.Write))
            using (var tarWriter = new TarWriter(stream, new TarWriterOptions(CompressionType.GZip, true)))
            {
                foreach (var eachFile in fileContents)
                {
                    tarWriter.Write(eachFile.Key, new MemoryStream(eachFile.Value));
                }
            }

            var resultLayer = new OCIArtifactLayer() { SequenceNumber = fileLayer.SequenceNumber, Content = File.ReadAllBytes("temp.tar.gz"), FileName = fileLayer.FileName };
            File.Delete("temp.tar.gz");
            return resultLayer;
        }

        public List<OCIArtifactLayer> ArchiveOCIFileLayers(List<OCIFileLayer> fileLayers)
        {
            if (fileLayers == null)
            {
                return null;
            }

            var result = new List<OCIArtifactLayer>();

            foreach (var layer in fileLayers)
            {
                result.Add(ArchiveOCIFileLayer(layer));
            }

            return result;
        }
    }
}
