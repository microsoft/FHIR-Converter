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
        public static Metric TotalDuration { get => new Metric(nameof(TotalDuration), dimensions: new Dictionary<string, object> { }); }

        public static Metric InputDeserializationDuration { get => new Metric(nameof(InputDeserializationDuration), dimensions: new Dictionary<string, object> { }); }

        public static Metric TemplateRetrievalDuration { get => new Metric(nameof(TemplateRetrievalDuration), dimensions: new Dictionary<string, object> { }); }

        public static Metric TemplateRenderDuration { get => new Metric(nameof(TemplateRenderDuration), dimensions: new Dictionary<string, object> { }); }

        public static Metric PostProcessDuration { get => new Metric(nameof(PostProcessDuration), dimensions: new Dictionary<string, object> { }); }
    }
}
