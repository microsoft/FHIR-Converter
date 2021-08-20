// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Health.Fhir.TemplateManagement.Models;

namespace Microsoft.Health.Fhir.TemplateManagement.Overlay
{
    public interface IOverlayFileSystem
    {
        OCIFileLayer ReadOCIFileLayer();

        void WriteOCIFileLayer(OCIFileLayer oneLayer);

        List<OCIArtifactLayer> ReadImageLayers();

        void WriteImageLayers(List<OCIArtifactLayer> imageLayers);

        OCIArtifactLayer ReadBaseLayer();

        void WriteBaseLayer(OCIArtifactLayer baseLayer);

        void ClearImageLayerFolder();

        void ClearBaseLayerFolder();
    }
}
