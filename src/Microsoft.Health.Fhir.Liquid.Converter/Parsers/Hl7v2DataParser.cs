// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.InputProcessors;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Models.Hl7v2;
using Microsoft.Health.Fhir.Liquid.Converter.Utilities;
using Microsoft.Health.Fhir.Liquid.Converter.Validators;

namespace Microsoft.Health.Fhir.Liquid.Converter.Parsers
{
    public class Hl7v2DataParser : IDataParser
    {
        private static readonly Hl7v2DataValidator Validator = new Hl7v2DataValidator();

        public object Parse(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new DataParseException(FhirConverterErrorCode.NullOrWhiteSpaceInput, Resources.NullOrWhiteSpaceInput);
            }

            try
            {
                var result = new Hl7v2Data(message);

                var segments = Hl7v2DataUtility.SplitMessageToSegments(message);
                Validator.ValidateMessageHeader(segments[0]);
                var encodingCharacters = ParseHl7v2EncodingCharacters(segments[0]);
                result.EncodingCharacters = encodingCharacters;

                for (var i = 0; i < segments.Length; ++i)
                {
                    var fields = ParseFields(segments[i], encodingCharacters, isHeaderSegment: i == 0);
                    var segment = new Hl7v2Segment(NormalizeText(segments[i], encodingCharacters), fields);
                    result.Meta.Add(fields.First()?.Value ?? string.Empty);
                    result.Data.Add(segment);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new DataParseException(FhirConverterErrorCode.InputParsingError, string.Format(Resources.InputParsingError, ex.Message), ex);
            }
        }

        private static List<Hl7v2Field> ParseFields(string dataString, Hl7v2EncodingCharacters encodingCharacters, bool isHeaderSegment = false)
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
                    var separatorFieldComponents = new List<Hl7v2Component>
                    {
                        null,
                        new Hl7v2Component(fieldValues[f], new List<string> { null, fieldValues[f] }),
                    };
                    var separatorField = new Hl7v2Field(fieldValues[f], separatorFieldComponents);
                    fields.Add(separatorField);
                }
                else
                {
                    if (!string.IsNullOrEmpty(fieldValues[f]))
                    {
                        /**
                         * We have four circumstances here.
                         * 1. The templates using this field treat it as repeatable, and the field contains $RepetitionSeparator;
                         * 2. The templates using this field treat it as repeatable, and the field doesn't contain $RepetitionSeparator;
                         * 3. The templates using this field treat it as unrepeatable, and the field contains $RepetitionSeparator;
                         * 4. The templates using this field treat it as unrepeatable, and the field doesn't contain $RepetitionSeparator;
                         *
                         * For circumstance #1 and #2, it will be all ok because the $field.Repeats always contains at least one value;
                         * For circumstance #3, we just take the first element in the repetition as the whole field by default;
                         * For circumstance #4, there will also be all right to take the first element of the $Repeats as the field itself;
                         */
                        var field = new Hl7v2Field(NormalizeText(fieldValues[f], encodingCharacters), new List<Hl7v2Component>());
                        var repetitions = fieldValues[f].Split(encodingCharacters.RepetitionSeparator);
                        for (var r = 0; r < repetitions.Length; ++r)
                        {
                            var repetitionComponents = ParseComponents(repetitions[r], encodingCharacters);
                            var repetition = new Hl7v2Field(NormalizeText(repetitions[r], encodingCharacters), repetitionComponents);
                            field.Repeats.Add(repetition);
                        }

                        field.Components = ((Hl7v2Field)field.Repeats[0]).Components;
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

        private static List<Hl7v2Component> ParseComponents(string dataString, Hl7v2EncodingCharacters encodingCharacters)
        {
            // Add a null value at first to keep consistent indexes with HL7 v2 spec
            var components = new List<Hl7v2Component> { null };
            var componentValues = dataString.Split(encodingCharacters.ComponentSeparator);
            foreach (var componentValue in componentValues)
            {
                if (!string.IsNullOrEmpty(componentValue))
                {
                    var subcomponents = ParseSubcomponents(componentValue, encodingCharacters);
                    var component = new Hl7v2Component(NormalizeText(componentValue, encodingCharacters), subcomponents);
                    components.Add(component);
                }
                else
                {
                    components.Add(null);
                }
            }

            return components;
        }

        private static List<string> ParseSubcomponents(string dataString, Hl7v2EncodingCharacters encodingCharacters)
        {
            // Add a null value at first to keep consistent indexes with HL7 v2 spec
            var subcomponents = new List<string>() { null };
            var subcomponentValues = dataString.Split(encodingCharacters.SubcomponentSeparator);
            foreach (var subcomponentValue in subcomponentValues)
            {
                subcomponents.Add(NormalizeText(subcomponentValue, encodingCharacters));
            }

            return subcomponents;
        }

        private static Hl7v2EncodingCharacters ParseHl7v2EncodingCharacters(string headerSegment)
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

        private static string NormalizeText(string value, Hl7v2EncodingCharacters encodingCharacters)
        {
            var semanticalUnescape = Hl7v2EscapeSequenceProcessor.Unescape(value, encodingCharacters);
            var grammarEscape = SpecialCharProcessor.Escape(semanticalUnescape);
            return grammarEscape;
        }
    }
}
