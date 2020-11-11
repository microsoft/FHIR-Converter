// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Liquid.Converter.Tool.Models
{
    public class ConverterResult
    {
        public ConverterResult(ProcessStatus status, string fhirResource)
        {
            Status = status;
            FhirResource = new JRaw(fhirResource);
        }

        public ProcessStatus Status { get; set; }

        public JRaw FhirResource { get; set; }
    }
}
