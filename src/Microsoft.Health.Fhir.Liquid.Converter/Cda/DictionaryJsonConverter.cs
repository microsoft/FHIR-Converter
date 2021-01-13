// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Liquid.Converter.Cda
{
    public class DictionaryJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            WriteValue(writer, value);
        }

        private void WriteValue(JsonWriter writer, object value)
        {
            var t = JToken.FromObject(value);
            switch (t.Type)
            {
                case JTokenType.Object:
                    WriteObject(writer, value);
                    break;
                case JTokenType.Array:
                    WriteArray(writer, value);
                    break;
                default:
                    writer.WriteValue(value);
                    break;
            }
        }

        private void WriteObject(JsonWriter writer, object value)
        {
            writer.WriteStartObject();
            var obj = value as IDictionary<string, object>;
            foreach (var kvp in obj)
            {
                writer.WritePropertyName(kvp.Key);
                WriteValue(writer, kvp.Value);
            }

            writer.WriteEndObject();
        }

        private void WriteArray(JsonWriter writer, object value)
        {
            writer.WriteStartArray();
            var array = value as IEnumerable<object>;
            foreach (var o in array)
            {
                WriteValue(writer, o);
            }

            writer.WriteEndArray();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return ReadValue(reader);
        }

        private object ReadValue(JsonReader reader)
        {
            while (reader.TokenType == JsonToken.Comment)
            {
                if (!reader.Read())
                {
                    throw new JsonSerializationException("Unexpected Token when converting IDictionary<string, object>");
                }
            }

            switch (reader.TokenType)
            {
                case JsonToken.StartObject:
                    return ReadObject(reader);
                case JsonToken.StartArray:
                    return ReadArray(reader);
                case JsonToken.Integer:
                case JsonToken.Float:
                case JsonToken.String:
                case JsonToken.Boolean:
                case JsonToken.Undefined:
                case JsonToken.Null:
                case JsonToken.Date:
                case JsonToken.Bytes:
                    return reader.Value;
                default:
                    throw new JsonSerializationException($"Unexpected token when converting IDictionary<string, object>: {reader.TokenType}");
            }
        }

        private object ReadArray(JsonReader reader)
        {
            IList<object> list = new List<object>();

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.Comment:
                        break;
                    case JsonToken.EndArray:
                        return list;
                    default:
                        var v = ReadValue(reader);
                        list.Add(v);
                        break;
                }
            }

            throw new JsonSerializationException("Unexpected end when reading IDictionary<string, object>");
        }

        private object ReadObject(JsonReader reader)
        {
            var obj = new Dictionary<string, object>();

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        var propertyName = reader.Value.ToString();

                        if (!reader.Read())
                        {
                            throw new JsonSerializationException("Unexpected end when reading IDictionary<string, object>");
                        }

                        var v = ReadValue(reader);

                        // TODO: Check attribute "@" and text content "#"
                        if (propertyName.StartsWith("@"))
                        {
                            // If property is attribute, remove "@"
                            propertyName = propertyName[1..];
                            obj[propertyName] = v;
                        }
                        else if (propertyName.Equals("#text", StringComparison.InvariantCultureIgnoreCase))
                        {
                            // If property is text content, replace it with "_"
                            propertyName = "_";
                            obj[propertyName] = v;
                        }
                        else
                        {
                            if (v is ICollection)
                            {
                                obj[propertyName] = v;
                            }
                            else
                            {
                                // Some properties are saved in element while some others are saved in text content, e.g., <family>Betterhalf</family> and <family qualifier="SP">Betterhalf</family>
                                // If saved as element, it is saved in "x".
                                // If saved as text content, it is saved in "x.#text".
                                // We need to convert them to same level with "x._".
                                // TODO: Double check the logic (list?)
                                var innerObj = new Dictionary<string, object>() { { "_", v } };
                                obj[propertyName] = innerObj;
                            }
                        }

                        break;
                    case JsonToken.Comment:
                        break;
                    case JsonToken.EndObject:
                        return obj;
                }
            }

            throw new JsonSerializationException("Unexpected end when reading IDictionary<string, object>");
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(IDictionary<string, object>).IsAssignableFrom(objectType);
        }
    }
}
