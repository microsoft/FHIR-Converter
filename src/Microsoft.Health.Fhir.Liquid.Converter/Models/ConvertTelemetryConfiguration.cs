using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
