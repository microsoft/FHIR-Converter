// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Liquid.Converter.Parsers
{
    public class CcdaDataParser : IDataParser
    {
        public object Parse(string document)
        {
            if (string.IsNullOrWhiteSpace(document))
            {
                throw new DataParseException(FhirConverterErrorCode.NullOrWhiteSpaceInput, Resources.NullOrWhiteSpaceInput);
            }

            try
            {
                var xDocument = XDocument.Parse(document);

                // Serialize contents of `text` elements as string in `_innerText` child element
                StringifyTextNodeContents(xDocument?.Root);

                // Remove redundant namespaces to avoid appending namespace prefix before elements
                var defaultNamespace = xDocument.Root?.GetDefaultNamespace().NamespaceName;
                xDocument.Root?.Attributes()
                    .Where(attribute => IsRedundantNamespaceAttribute(attribute, defaultNamespace))
                    .Remove();

                // Normalize non-default namespace prefix in elements
                var namespaces = xDocument.Root?.Attributes()
                    .Where(x => x.IsNamespaceDeclaration && x.Value != defaultNamespace);
                NormalizeNamespacePrefix(xDocument?.Root, namespaces);

                // Change XText to XElement with name "_" to avoid serializing depth difference, e.g., given="foo" and given.#text="foo"
                ReplaceTextWithElement(xDocument?.Root);

                // Convert to json dictionary
                var jsonString = JsonConvert.SerializeXNode(xDocument);
                var dataDictionary = JsonConvert.DeserializeObject<IDictionary<string, object>>(jsonString, new DictionaryJsonConverter()) ??
                                     new Dictionary<string, object>();

                // Remove line breaks in original data
                dataDictionary["_originalData"] = Regex.Replace(document, @"\r\n?|\n", string.Empty);

                return dataDictionary;
            }
            catch (Exception ex)
            {
                throw new DataParseException(FhirConverterErrorCode.InputParsingError, string.Format(Resources.InputParsingError, ex.Message), ex);
            }
        }

        private static bool IsRedundantNamespaceAttribute(XAttribute attribute, string defaultNamespace)
        {
            return attribute != null &&
                   attribute.IsNamespaceDeclaration &&
                   !string.Equals(attribute.Name.LocalName, "xmlns", StringComparison.InvariantCultureIgnoreCase) &&
                   string.Equals(attribute.Value, defaultNamespace, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Replace "namespace:attribute" to "namespace_attribute" to be compatible with DotLiquids, e.g., from sdtc:raceCode to sdtc_raceCode
        /// </summary>
        private static void NormalizeNamespacePrefix(XElement element, IEnumerable<XAttribute> namespaces)
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
                    break;
                }
            }

            foreach (var childElement in element.Elements())
            {
                NormalizeNamespacePrefix(childElement, namespaces);
            }
        }

        private static void ReplaceTextWithElement(XElement element)
        {
            if (element == null)
            {
                return;
            }

            // Iterate reversely as the list itself is updating
            var nodes = element.Nodes().ToList();
            for (var i = nodes.Count - 1; i >= 0; --i)
            {
                switch (nodes[i])
                {
                    case XText textNode:
                        element.Add(new XElement("_", textNode.Value));
                        textNode.Remove();
                        break;
                    case XElement elementNode:
                        ReplaceTextWithElement(elementNode);
                        break;
                }
            }
        }

        private static void StringifyTextNodeContents(XElement element)
        {
            if (element == null)
            {
                return;
            }

            System.Xml.XmlWriterSettings xws = new ()
            {
                OmitXmlDeclaration = true,
                ConformanceLevel = System.Xml.ConformanceLevel.Fragment,
            };

            foreach (var el in element.Descendants())
            {
                if (el.Name.LocalName == "text")
                {
                    System.Text.StringBuilder sb = new ();

                    using (System.Xml.XmlWriter xw = System.Xml.XmlWriter.Create(sb, xws))
                    {
                        foreach (var node in el.Nodes())
                        {
                            node.WriteTo(xw);
                        }
                    }

                    var content = sb.ToString();
                    el.SetAttributeValue("_innerText", content);
                }
            }
        }
    }
}
