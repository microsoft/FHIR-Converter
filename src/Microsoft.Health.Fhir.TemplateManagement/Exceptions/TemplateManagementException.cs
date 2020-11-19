// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Fhir.TemplateManagement.Models;

namespace Microsoft.Health.Fhir.TemplateManagement.Exceptions
{
    public class TemplateManagementException : Exception
    {
        public TemplateManagementException()
        {
        }

        public TemplateManagementException(string message)
            : base(message)
        {
        }

        public TemplateManagementException(TemplateManagementErrorCode templateManagementErrorCode, string message)
            : base(message)
        {
            TemplateManagementErrorCode = templateManagementErrorCode;
        }

        public TemplateManagementException(TemplateManagementErrorCode templateManagementErrorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            TemplateManagementErrorCode = templateManagementErrorCode;
        }

        public TemplateManagementErrorCode TemplateManagementErrorCode { get; }
    }
}
