// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Common.Telemetry;

namespace Microsoft.Health.Fhir.Liquid.Converter.Telemetry
{
    public sealed class ConverterMetricDefinition : MetricDefinition
    {
        public ConverterMetricDefinition(string metricName)
            : base(metricName)
        {
        }

        public static ConverterMetricDefinition Test { get; } = new ConverterMetricDefinition(nameof(Test));
    }
}
