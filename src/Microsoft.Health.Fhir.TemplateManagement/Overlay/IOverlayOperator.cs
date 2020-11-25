// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Health.Fhir.TemplateManagement.Models;

namespace Microsoft.Health.Fhir.TemplateManagement.Overlay
{
    public interface IOverlayOperator
    {
        /// <summary>
        /// Extract one OCIArtifactLayer to OCIFileLayer
        /// </summary>
        /// <param name="artifactLayer">One OCIArtifactLayer</param>
        /// <returns>One OCIFileLayer</returns>
        public OCIFileLayer ExtractOCIFileLayer(OCIArtifactLayer artifactLayer);

        /// <summary>
        /// Extract List of OCIArtifactLayers to a list of OCIFileLayers
        /// </summary>
        /// <param name="artifactLayers">List of OCIArtifactLayers</param>
        /// <returns>List of OCIFileLayers</returns>
        public List<OCIFileLayer> ExtractOCIFileLayers(List<OCIArtifactLayer> artifactLayers);

        /// <summary>
        /// Sort OCIFileLayers by build number.
        /// First read build number from digest.json file.
        /// Then sort layers by build number.
        /// If read build number failed, the build number is -1 which will be placed at the end of list.
        /// </summary>
        /// <param name="fileLayers">Lsit of OCIFileLayers</param>
        /// <returns>The sorted list of OCIFileLayers</returns>
        List<OCIFileLayer> SortOCIFileLayersBySequenceNumber(List<OCIFileLayer> fileLayers);

        /// <summary>
        /// Merge sorted OCIFileLayers.
        /// If the value of File is null, the file is removed by this layer.
        /// The upper layer will override the lower layers file with same filename.
        /// </summary>
        /// <param name="sortedLayers">List of sorted OCIFileLayers</param>
        /// <returns>One Merged OCIFileLayer.</returns>
        public OCIFileLayer MergeOCIFileLayers(List<OCIFileLayer> sortedLayers);

        /// <summary>
        /// Generate diff OCIFileLayer by comprare two OCIArtifactLayers.
        /// </summary>
        /// <param name="fileLayer">source OCIFileLayer</param>
        /// <param name="snapshotLayer">target OCIFileLayer</param>
        /// <returns>The diff layer.</returns>
        public OCIFileLayer GenerateDiffLayer(OCIFileLayer fileLayer, OCIFileLayer snapshotLayer);

        public OCIArtifactLayer ArchiveOCIFileLayer(OCIFileLayer fileLayer);

        public List<OCIArtifactLayer> ArchiveOCIFileLayers(List<OCIFileLayer> fileLayers);
    }
}
