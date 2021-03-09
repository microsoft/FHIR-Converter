// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Linq;
using Microsoft.Azure.ContainerRegistry.Models;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Models;

namespace Microsoft.Health.Fhir.TemplateManagement.Utilities
{
    public static class ValidationUtility
    {
        public static void ValidateOneBlob(byte[] content, string digest)
        {
            if (content == null)
            {
                throw new ImageValidationException(TemplateManagementErrorCode.InvalidBlobContent, "Image content invalid");
            }

            var hashedValue = StreamUtility.CalculateDigestFromSha256(content);
            if (!string.Equals(hashedValue, digest))
            {
                throw new ImageValidationException(TemplateManagementErrorCode.InvalidBlobContent, "Image content invalid");
            }
        }

        public static void ValidateManifest(ManifestWrapper manifestInfo)
        {
            if (manifestInfo?.Layers?.Any() != true)
            {
                throw new ImageValidationException(TemplateManagementErrorCode.InvalidManifestInfo, $"Manifest is invalid");
            }

            foreach (var oneLayer in manifestInfo.Layers)
            {
                if (string.IsNullOrEmpty(oneLayer.Digest))
                {
                    throw new ImageValidationException(TemplateManagementErrorCode.InvalidManifestInfo, $"Manifest is incomplete. Layer's digest could not be empty");
                }
            }
        }
    }
}
