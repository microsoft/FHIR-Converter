// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;

namespace Microsoft.Health.Fhir.Liquid.Converter.Tool.Models
{
    public class ConverterError
    {
        public ConverterError(Exception exception, string templateDirectory = null)
        {
            Status = ProcessStatus.Fail;

            // For TemplateNotFound, add template directory information in error message to help find template
            if (exception is RenderException re && re.FhirConverterErrorCode == FhirConverterErrorCode.TemplateNotFound)
            {
                ErrorType = re.GetType().ToString();
                ErrorCode = re.FhirConverterErrorCode.ToString();
                ErrorMessage = $"{re.Message} in template folder: {templateDirectory}.";
                ErrorDetails = re.ToString();
            }
            else
            {
                ErrorType = exception.GetType().ToString();
                ErrorCode = exception is FhirConverterException fce ? fce.FhirConverterErrorCode.ToString() : string.Empty;
                ErrorMessage = exception.Message;
                ErrorDetails = exception.ToString();
            }
        }

        public ProcessStatus Status { get; set; }

        public string ErrorType { get; set; }

        public string ErrorCode { get; set; }

        public string ErrorMessage { get; set; }

        public string ErrorDetails { get; set; }
    }
}
