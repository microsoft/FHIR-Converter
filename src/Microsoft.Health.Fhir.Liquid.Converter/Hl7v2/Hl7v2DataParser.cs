// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Hl7v2.InputProcessor;
using Microsoft.Health.Fhir.Liquid.Converter.Hl7v2.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Models;

namespace Microsoft.Health.Fhir.Liquid.Converter.Hl7v2
{
    public class Hl7v2DataParser
    {
        private readonly Hl7v2DataValidator _validator = new Hl7v2DataValidator();
        private static readonly string[] SegmentSeparators = { "\r\n", "\r", "\n" };

        public Hl7v2Data Parse(string message)
        {
            var result = new Hl7v2Data(message);
            try
            {
                if (string.IsNullOrEmpty(message))
                {
                    throw new DataParseException(FhirConverterErrorCode.NullOrEmptyInput, Resources.NullOrEmptyInput);
                }

                var segments = message.Split(SegmentSeparators, StringSplitOptions.RemoveEmptyEntries);
                _validator.ValidateMessageHeader(segments[0]);
                var encodingCharacters = ParseHl7v2EncodingCharacters(segments[0]);
                result.EncodingCharacters = encodingCharacters;

                for (var i = 0; i < segments.Length; ++i)
                {
                    var fields = ParseFields(segments[i], encodingCharacters, isHeaderSegment: i == 0);
                    var segment = new Hl7v2Segment(SlackValue(segments[i], encodingCharacters), fields);
                    result.Meta.Add(fields.First()?.Value ?? string.Empty);
                    result.Data.Add(segment);
                }
            }
            catch (Exception ex)
            {
                throw new DataParseException(FhirConverterErrorCode.InputParsingError, string.Format(Resources.InputParsingError, ex.Message), ex);
            }

            return result;
        }

        private List<Hl7v2Field> ParseFields(string dataString, Hl7v2EncodingCharacters encodingCharacters, bool isHeaderSegment = false)
        {
            var fields = new List<Hl7v2Field>();
            var fieldValues = dataString.Split(encodingCharacters.FieldSeparator);
            for (var f = 0; f < fieldValues.Length; ++f)
            {
                // MSH segment need to be handled separatedly since it's first field is the field separator `|`,
                // and the second field is encoding characters
                if (isHeaderSegment && f == 1)
                {
                    // field separator
                    var fieldSeparatorComponents = new List<Hl7v2Component>
                    {
                        null,
                        new Hl7v2Component(encodingCharacters.FieldSeparator.ToString(), new List<string> { null, encodingCharacters.FieldSeparator.ToString() }),
                    };
                    var fieldSeparatorField = new Hl7v2Field(encodingCharacters.FieldSeparator.ToString(), fieldSeparatorComponents);
                    fields.Add(fieldSeparatorField);

                    // encoding characters
                    var seperatorFieldComponents = new List<Hl7v2Component>
                    {
                        null,
                        new Hl7v2Component(fieldValues[f], new List<string> { null, fieldValues[f] }),
                    };
                    var separatorField = new Hl7v2Field(fieldValues[f], seperatorFieldComponents);
                    fields.Add(separatorField);
                }
                else
                {
                    if (!string.IsNullOrEmpty(fieldValues[f]))
                    {
                        var components = ParseComponents(fieldValues[f], encodingCharacters);
                        var field = new Hl7v2Field(SlackValue(fieldValues[f], encodingCharacters), components);
                        var repetitions = fieldValues[f].Split(encodingCharacters.RepetitionSeparator);
                        for (var r = 0; r < repetitions.Length; ++r)
                        {
                            var repetitionComponents = ParseComponents(repetitions[r], encodingCharacters);
                            var repetition = new Hl7v2Field(SlackValue(repetitions[r], encodingCharacters), repetitionComponents);
                            field.Repeats.Add(repetition);
                        }

                        fields.Add(field);
                    }
                    else
                    {
                        fields.Add(null);
                    }
                }
            }

            return fields;
        }

        private List<Hl7v2Component> ParseComponents(string dataString, Hl7v2EncodingCharacters encodingCharacters)
        {
            // Add a null value at first to keep consistent indexes with HL7 v2 spec
            var components = new List<Hl7v2Component> { null };
            var componentValues = dataString.Split(encodingCharacters.ComponentSeparator);
            foreach (var componentValue in componentValues)
            {
                if (!string.IsNullOrEmpty(componentValue))
                {
                    var subcomponents = ParseSubcomponents(componentValue, encodingCharacters);
                    var component = new Hl7v2Component(SlackValue(componentValue, encodingCharacters), subcomponents);
                    components.Add(component);
                }
                else
                {
                    components.Add(null);
                }
            }

            return components;
        }

        private List<string> ParseSubcomponents(string dataString, Hl7v2EncodingCharacters encodingCharacters)
        {
            // Add a null value at first to keep consistent indexes with HL7 v2 spec
            var subcomponents = new List<string>() { null };
            var subcomponentValues = dataString.Split(encodingCharacters.SubcomponentSeparator);
            foreach (var subcomponentValue in subcomponentValues)
            {
                subcomponents.Add(SlackValue(subcomponentValue, encodingCharacters));
            }

            return subcomponents;
        }

        private Hl7v2EncodingCharacters ParseHl7v2EncodingCharacters(string headerSegment)
        {
            return new Hl7v2EncodingCharacters
            {
                FieldSeparator = headerSegment[3],
                ComponentSeparator = headerSegment[4],
                RepetitionSeparator = headerSegment[5],
                EscapeCharacter = headerSegment[6],
                SubcomponentSeparator = headerSegment[7],
            };
        }

        private string SlackValue(string value, Hl7v2EncodingCharacters encodingCharacters)
        {
            var semanticalUnescape = Hl7v2EscapeSequenceProcessor.Unescape(value, encodingCharacters);
            var grammarEscape = SpecialCharProcessor.Escape(semanticalUnescape);
            return grammarEscape;
        }
    }
}
