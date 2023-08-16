// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Metric = Microsoft.Health.Common.Telemetry.Metric;

namespace Microsoft.Health.Fhir.Liquid.Converter.Telemetry
{
    public sealed class ConverterMetrics
    {
        public static Metric TotalDuration { get => new Metric(nameof(TotalDuration), new Dictionary<string, object> { }); }
    }
}
