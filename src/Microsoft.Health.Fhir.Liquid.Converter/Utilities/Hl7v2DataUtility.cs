using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Health.Fhir.Liquid.Converter.Utilities
{
    public class Hl7v2DataUtility
    {
        private static readonly string[] SegmentSeparators = { "\r\n", "\r", "\n" };

        public string[] SplitSegment(string message)
        {
            var segments = message.Split(SegmentSeparators, StringSplitOptions.RemoveEmptyEntries);
            return segments;
        }
    }
}
