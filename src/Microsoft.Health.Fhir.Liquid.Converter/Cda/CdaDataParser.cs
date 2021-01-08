// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Liquid.Converter.Cda
{
    public class CdaDataParser
    {
        public IDictionary<string, object> Parse(string document)
        {
            var xdoc = XDocument.Parse(document);

            // Remove namespaces
            RemoveNamespacePrefix(xdoc.Root);

            var jsonString = JsonConvert.SerializeObject(xdoc);
            return new Dictionary<string, object>()
            {
                { "msg", JsonConvert.DeserializeObject<IDictionary<string, object>>(jsonString, new DictionaryConverter()) },
            };
        }

        private void RemoveNamespacePrefix(XElement element)
        {
            // TODO: Check namespace, especially for sdtc
            if (element?.Name?.Namespace != null)
            {
                element.Name = element.Name.LocalName;
            }

            foreach (var child in element?.Descendants())
            {
                RemoveNamespacePrefix(child);
            }
        }
    }
}
