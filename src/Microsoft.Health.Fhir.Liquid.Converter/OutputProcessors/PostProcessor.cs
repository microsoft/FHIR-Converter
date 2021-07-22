// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Liquid.Converter.OutputProcessors
{
    public static class PostProcessor
    {
        public static JObject Process(string input)
        {
            var jsonObject = ParseJson(input);
            return MergeJson(jsonObject);
        }

        public static JObject ParseJson(string input)
        {
            var stream = new AntlrInputStream(input);
            var lexer = new jsonLexer(stream);
            var tokens = new CommonTokenStream(lexer);
            var errorBuilder = new StringBuilder();
            var parser = new jsonParser(tokens, new StringWriter(new StringBuilder()), new StringWriter(errorBuilder));
            lexer.RemoveErrorListeners();
            parser.RemoveErrorListeners();
            parser.AddErrorListener(new JsonParserErrorListener());
            parser.BuildParseTree = true;
            var tree = parser.json();
            if (parser.NumberOfSyntaxErrors > 0)
            {
                throw new PostprocessException(FhirConverterErrorCode.JsonParsingError, string.Format(Resources.JsonParsingError, errorBuilder));
            }

            var listener = new JsonListener();
            ParseTreeWalker.Default.Walk(listener, tree);
            var result = listener.GetResult().ToString();

            return JsonConvert.DeserializeObject<JObject>(result, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
        }

        public static JObject MergeJson(JObject obj)
        {
            try
            {
                var mergedEntity = new JArray();
                var resourceKeyToIndexMap = new Dictionary<string, int>();

                if (obj.TryGetValue("entry", out var entries))
                {
                    foreach (var entry in entries)
                    {
                        var resourceKey = GetKey(entry);
                        if (resourceKeyToIndexMap.ContainsKey(resourceKey))
                        {
                            var index = resourceKeyToIndexMap[resourceKey];
                            mergedEntity[index] = Merge((JObject)mergedEntity[index], (JObject)entry);
                        }
                        else
                        {
                            mergedEntity.Add(entry);
                            resourceKeyToIndexMap[resourceKey] = mergedEntity.Count - 1;
                        }
                    }

                    obj["entry"] = mergedEntity;
                }
            }
            catch (Exception ex)
            {
                throw new PostprocessException(FhirConverterErrorCode.JsonMergingError, string.Format(Resources.JsonMergingError, ex.Message), ex);
            }

            return obj;
        }

        private static JObject Merge(JObject obj1, JObject obj2)
        {
            var setting = new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Union,
            };

            obj1.Merge(obj2, setting);

            return obj1;
        }

        private static string GetKey(JToken obj)
        {
            var resourceType = obj.SelectToken("$.resource.resourceType")?.Value<string>();
            if (resourceType != null)
            {
                var key = resourceType;
                var versionId = obj.SelectToken("$.resource.meta.versionId")?.Value<string>();
                key += versionId != null ? $"_{versionId}" : string.Empty;

                var resourceId = obj.SelectToken("$.resource.id")?.Value<string>();
                key += resourceId != null ? $"_{resourceId}" : string.Empty;

                return key;
            }

            return JsonConvert.SerializeObject(obj);
        }
    }
}
