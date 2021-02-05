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
        OCIFileLayer ExtractArtifactLayer(OCIArtifactLayer artifactLayer);

        /// <summary>
        /// Extract List of OCIArtifactLayers to a list of OCIFileLayers
        /// </summary>
        /// <param name="artifactLayers">List of OCIArtifactLayers</param>
        /// <returns>List of OCIFileLayers</returns>
        List<OCIFileLayer> ExtractArtifactLayers(List<OCIArtifactLayer> artifactLayers);

        /// <summary>
        /// Merge sorted OCIFileLayers.
        /// If the value of File is null, the file is removed by this layer.
        /// The upper layer will override the lower layers file with same filename.
        /// </summary>
        /// <param name="sortedLayers">List of sorted OCIFileLayers</param>
        /// <returns>One Merged OCIFileLayer.</returns>
        OCIFileLayer MergeOCIFileLayers(List<OCIFileLayer> sortedLayers);

        /// <summary>
        /// Generate diff OCIFileLayer by comparing two OCIArtifactLayers.
        /// </summary>
        /// <param name="fileLayer">source OCIFileLayer</param>
        /// <param name="snapshotLayer">target OCIFileLayer</param>
        /// <returns>The diff layer.</returns>
        OCIFileLayer GenerateDiffLayer(OCIFileLayer fileLayer, OCIFileLayer snapshotLayer);

        /// <summary>
        /// Archive OCIFileLayer to OCIArtifactLayer by compress file content by gzip mode.
        /// </summary>
        /// <param name="fileLayer">One OCIFileLayer</param>
        /// <returns>One OCIArtifactLayer</returns>
        OCIArtifactLayer ArchiveOCIFileLayer(OCIFileLayer fileLayer);

        /// <summary>
        /// Archive List of OCIFileLayers to OCIArtifactLayers.
        /// </summary>
        /// <param name="fileLayers">List of OCIFileLayer</param>
        /// <returns>List of OCIArtifactLayer</returns>
        List<OCIArtifactLayer> ArchiveOCIFileLayers(List<OCIFileLayer> fileLayers);
    }
}
