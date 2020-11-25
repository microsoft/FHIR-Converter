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
        public OCIFileLayer ReadMergedOCIFileLayer();

        public void WriteMergedOCIFileLayer(OCIFileLayer oneLayer);

        public List<OCIArtifactLayer> ReadImageLayers();

        public void WriteImageLayers(List<OCIArtifactLayer> imageLayers);

        public List<OCIArtifactLayer> ReadBaseLayers();

        public void WriteBaseLayers(List<OCIArtifactLayer> layers);

        public void ClearImageLayerFolder();

        public void ClearBaseLayerFolder();
    }
}
