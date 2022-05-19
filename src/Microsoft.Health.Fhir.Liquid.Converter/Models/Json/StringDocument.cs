
using System.Collections.Generic;
using System.Linq;
using DotLiquid;

namespace Microsoft.Health.Fhir.Liquid.Converter.Models.Json
{
    public class StringDocument : Document
    {
        public StringDocument(List<string> contents)
        {
            NodeList = contents.Cast<object>().ToList();
        }
    }
}
