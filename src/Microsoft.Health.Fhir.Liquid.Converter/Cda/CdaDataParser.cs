// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Liquid.Converter.Cda
{
    public class CdaDataParser
    {
        public IDictionary<string, object> Parse(string document)
        {
            try
            {
                var xDocument = XDocument.Parse(document);

                // Remove redundant xml namespaces
                var defaultNamespace = xDocument?.Root?.GetDefaultNamespace().NamespaceName;
                var attributes = xDocument?.Root?.Attributes();
                foreach (var attribute in attributes?.Where(attribute => IsRedundantNamespaceAttribute(attribute, defaultNamespace))?.ToList())
                {
                    attribute.Remove();
                }

                // Normalize non-default namespace in xml attributes
                var namespaces = xDocument?.Root?.Attributes()?.Where(x => x.IsNamespaceDeclaration && x.Value != defaultNamespace);
                NormalizeNamespacePrefixInAttributes(xDocument?.Root, namespaces);

                // Convert to json
                var jsonString = JsonConvert.SerializeXNode(xDocument);
                var dataDictionary = JsonConvert.DeserializeObject<IDictionary<string, object>>(jsonString, new DictionaryJsonConverter()) ?? new Dictionary<string, object>();
                dataDictionary["_originalData"] = document;

                return new Dictionary<string, object>()
                {
                    { "msg", dataDictionary },
                };
            }
            catch (Exception ex)
            {
                throw new DataParseException(FhirConverterErrorCode.InputParsingError, string.Format(Resources.InputParsingError, ex.Message), ex);
            }
        }

        private bool IsRedundantNamespaceAttribute(XAttribute attribute, string defaultNamespace)
        {
            return attribute.IsNamespaceDeclaration &&
                   !string.Equals(attribute.Name.LocalName, "xmlns", StringComparison.InvariantCultureIgnoreCase) &&
                   string.Equals(attribute.Value, defaultNamespace, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Replace "namespace:attribute" to "namespace_attribute" to be compatible with DotLiquids
        /// </summary>
        private void NormalizeNamespacePrefixInAttributes(XElement element, IEnumerable<XAttribute> namespaces)
        {
            if (element == null || namespaces == null)
            {
                return;
            }

            foreach (var ns in namespaces)
            {
                if (string.Equals(ns.Value, element.Name.NamespaceName, StringComparison.InvariantCultureIgnoreCase))
                {
                    element.Name = $"{ns.Name.LocalName}_{element.Name.LocalName}";
                }
            }

            foreach (var child in element.Descendants())
            {
                NormalizeNamespacePrefixInAttributes(child, namespaces);
            }
        }
    }
}
