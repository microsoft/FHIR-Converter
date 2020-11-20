// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Fhir.TemplateManagement.Models;

namespace Microsoft.Health.Fhir.TemplateManagement.Exceptions
{
    public class ImageValidationException : TemplateManagementException
    {
        public ImageValidationException(TemplateManagementErrorCode templateManagementErrorCode, string message)
            : base(templateManagementErrorCode, message)
        {
        }

        public ImageValidationException(TemplateManagementErrorCode templateManagementErrorCode, string message, Exception innerException)
            : base(templateManagementErrorCode, message, innerException)
        {
        }
    }
}
