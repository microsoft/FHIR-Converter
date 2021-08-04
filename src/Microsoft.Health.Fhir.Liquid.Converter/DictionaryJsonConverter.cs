// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Liquid.Converter
{
    /// <summary>
    /// One-way JsonConverter to deserialize XML-converted JSON string to IDictionary
    /// </summary>
    public class DictionaryJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
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
                    throw new JsonSerializationException(Resources.UnexpectedJsonConvertToken);
                }
            }

            switch (reader.TokenType)
            {
                case JsonToken.StartObject:
                    return ReadObject(reader);
                case JsonToken.StartArray:
                    return ReadArray(reader);
                case JsonToken.String:
                    // Remove line breaks to avoid invalid line breaks in json value
                    // A line break is a normal character in XML but invalid in JSON
                    return Regex.Replace(reader.Value.ToString(), @"\r\n?|\n", string.Empty);
                case JsonToken.Integer:
                case JsonToken.Float:
                case JsonToken.Boolean:
                case JsonToken.Undefined:
                case JsonToken.Null:
                case JsonToken.Date:
                case JsonToken.Bytes:
                    return reader.Value;
                default:
                    throw new JsonSerializationException(Resources.UnexpectedJsonConvertToken);
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

            throw new JsonSerializationException(Resources.UnexpectedJsonConvertEnd);
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
                            throw new JsonSerializationException(Resources.UnexpectedJsonConvertEnd);
                        }

                        // Remove "@" if it is attribute
                        if (propertyName.StartsWith("@"))
                        {
                            propertyName = propertyName[1..];
                        }

                        var v = ReadValue(reader);
                        obj[propertyName] = v;
                        break;
                    case JsonToken.Comment:
                        break;
                    case JsonToken.EndObject:
                        return obj;
                }
            }

            throw new JsonSerializationException(Resources.UnexpectedJsonConvertEnd);
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(IDictionary<string, object>).IsAssignableFrom(objectType);
        }
    }
}
