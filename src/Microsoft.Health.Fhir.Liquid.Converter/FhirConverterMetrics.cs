using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
