
using System.Collections.Generic;
using System.Linq;
using DotLiquid;
using Newtonsoft.Json.Schema;

namespace Microsoft.Health.Fhir.Liquid.Converter.Models.Json
{
    public class JSchemaDocument : Document
    {
        public JSchemaDocument(List<JSchema> contents)
        {
            NodeList = contents.Cast<object>().ToList();
        }
    }
}
