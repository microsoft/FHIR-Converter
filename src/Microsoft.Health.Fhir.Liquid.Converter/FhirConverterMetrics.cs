// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.Liquid.Converter
{
    public static class FhirConverterMetrics
    {
        public static string TotalDuration { get => nameof(TotalDuration); }

        public static string InputDeserializationDuration { get => nameof(InputDeserializationDuration); }

        public static string TemplateRetrievalDuration { get => nameof(TemplateRetrievalDuration); }

        public static string TemplateRenderDuration { get => nameof(TemplateRenderDuration); }

        public static string PostProcessDuration { get => nameof(PostProcessDuration); }
    }
}
