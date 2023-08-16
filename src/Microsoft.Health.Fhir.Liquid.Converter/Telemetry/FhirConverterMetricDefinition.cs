// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Common.Telemetry;

namespace Microsoft.Health.Fhir.Liquid.Converter.Telemetry
{
    public sealed class FhirConverterMetricDefinition : MetricDefinition
    {
        public FhirConverterMetricDefinition(string metricName)
            : base(metricName)
        {
        }

        /// <summary>
        /// Overall duration of entire FHIR convert process.
        /// </summary>
        public static FhirConverterMetricDefinition TotalDuration { get; } = new FhirConverterMetricDefinition(GetMetricName(nameof(TotalDuration)));

        /// <summary>
        /// Breakdown - input data deserialization duration.
        /// </summary>
        public static FhirConverterMetricDefinition InputDataParsingDuration { get; } = new FhirConverterMetricDefinition(GetMetricName(nameof(InputDataParsingDuration)));

        /// <summary>
        /// Breakdown - template retrieving duration.
        /// </summary>
        public static FhirConverterMetricDefinition TemplateRetrievalDuration { get; } = new FhirConverterMetricDefinition(GetMetricName(nameof(TemplateRetrievalDuration)));

        /// <summary>
        /// Breakdown - liquid template rendering time.
        /// </summary>
        public static FhirConverterMetricDefinition TemplateRenderingDuration { get; } = new FhirConverterMetricDefinition(GetMetricName(nameof(TemplateRenderingDuration)));

        /// <summary>
        /// Breakdown - any post processing duration.
        /// </summary>
        public static FhirConverterMetricDefinition PostProcessingDuration { get; } = new FhirConverterMetricDefinition(GetMetricName(nameof(PostProcessingDuration)));

        private static string GetMetricName(string definitionName)
        {
            return $"FhirConverter{definitionName}";
        }
    }
}
