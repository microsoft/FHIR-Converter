// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.ContainerRegistry.Models;
using Microsoft.Health.Fhir.TemplateManagement.Models;

namespace Microsoft.Health.Fhir.TemplateManagement.Overlay
{
    public interface IOverlayFileSystem
    {
        /// <summary>
        /// Read all files from working folder.
        /// Ignore files in hidden image folder
        /// </summary>
        /// <returns> One layer of OCI files.</returns>
        OCIFileLayer ReadOCIFileLayer();

        /// <summary>
        /// Write all files of the OCIfilelayer to the working folder.
        /// </summary>
        /// <param name="oneLayer">One layer of OCI files.</param>
        void WriteOCIFileLayer(OCIFileLayer oneLayer);

        /// <summary>
        /// Read layers (only .tar.gz files) from working image forder.
        /// </summary>
        /// <returns>One OCIArtifact layer.</returns>
        List<OCIArtifactLayer> ReadImageLayers();

        /// <summary>
        /// Write compressed artifacts into tar.gz files to working image folder.
        /// </summary>
        /// <param name="imageLayers">A list of OCIArtifactLayer.</param>
        void WriteImageLayers(List<OCIArtifactLayer> imageLayers);

        /// <summary>
        /// Write manifest into working image folder.
        /// </summary>
        /// <param name="manifest">Manifest of the image.</param>
        public void WriteManifest(ManifestWrapper manifest);

        /// <summary>
        /// Read base OCIArtifactLayer from base layer folder.
        /// </summary>
        /// <returns>One base OCIArtifactLayer.</returns>
        OCIArtifactLayer ReadBaseLayer();

        /// <summary>
        /// Write base OCIArtifactLayer to base layer folser.
        /// </summary>
        /// <param name="baseLayer">One base OCIArtifactLayer.</param>
        void WriteBaseLayer(OCIArtifactLayer baseLayer);

        /// <summary>
        /// Clear working folder.
        /// </summary>
        void ClearWorkingFolder();

        /// <summary>
        /// Clear hidden image folder.
        /// </summary>
        void ClearImageLayerFolder();

        /// <summary>
        /// Clear hidden base layer folder.
        /// </summary>
        void ClearBaseLayerFolder();
    }
}
