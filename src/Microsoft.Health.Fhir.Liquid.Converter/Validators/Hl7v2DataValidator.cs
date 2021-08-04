// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;

namespace Microsoft.Health.Fhir.Liquid.Converter.Validators
{
    public class Hl7v2DataValidator
    {
        private const string HeaderSegmentId = "MSH";
        private const char EscapeCharacter = '\\';

        public void ValidateMessageHeader(string headerSegment)
        {
            if (string.IsNullOrWhiteSpace(headerSegment))
            {
                throw new DataParseException(FhirConverterErrorCode.NullOrWhiteSpaceInput, Resources.NullOrWhiteSpaceInput);
            }

            if (headerSegment.Length < HeaderSegmentId.Length)
            {
                throw new DataParseException(FhirConverterErrorCode.InvalidHl7v2Message, string.Format(Resources.InvalidHl7v2Message, headerSegment));
            }

            var segmentId = headerSegment.Substring(0, HeaderSegmentId.Length);
            if (!string.Equals(segmentId, HeaderSegmentId, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new DataParseException(FhirConverterErrorCode.InvalidHl7v2Message, string.Format(Resources.InvalidHl7v2Message, segmentId));
            }

            if (headerSegment.Length < 8)
            {
                throw new DataParseException(FhirConverterErrorCode.MissingHl7v2Separators, Resources.MissingHl7v2Separators);
            }

            var separators = headerSegment.Substring(HeaderSegmentId.Length, 5);
            if (separators.Distinct().Count() != separators.Length)
            {
                throw new DataParseException(FhirConverterErrorCode.DuplicateHl7v2Separators, Resources.DuplicateHl7v2Separators);
            }

            var escapeCharacter = headerSegment[6];
            if (!escapeCharacter.Equals(EscapeCharacter))
            {
                throw new DataParseException(FhirConverterErrorCode.InvalidHl7v2EscapeCharacter, Resources.InvalidHl7v2EscapeCharacter);
            }
        }
    }
}
