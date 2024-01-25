// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.Liquid.Converter.Models
{
    public class ConvertTelemetryConfiguration
    {
        /// <summary>
        /// Enable the performance telemetry logging.
        /// </summary>
        public bool EnableTelemetryLogger { get; set; } = false;
    }
}
