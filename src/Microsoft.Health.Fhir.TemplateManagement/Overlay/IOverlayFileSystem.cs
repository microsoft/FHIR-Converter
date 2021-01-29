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
        OCIFileLayer ReadMergedOCIFileLayer();

        void WriteMergedOCIFileLayer(OCIFileLayer oneLayer);

        List<OCIArtifactLayer> ReadImageLayers();

        void WriteImageLayers(List<OCIArtifactLayer> imageLayers);

        List<OCIArtifactLayer> ReadBaseLayers();

        void WriteBaseLayers(List<OCIArtifactLayer> layers);

        ManifestWrapper ReadManifest(string digest);

        ManifestWrapper ReadManifest(string dir, string fileName);

        void WriteManifest(ManifestWrapper manifest);

        void ClearImageLayerFolder();

        void ClearBaseLayerFolder();
    }
}
