// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.Liquid.Converter.Models.Hl7v2
{
    public class Hl7v2EncodingCharacters
    {
        public char FieldSeparator { get; set; } = '|';

        public char ComponentSeparator { get; set; } = '^';

        public char RepetitionSeparator { get; set; } = '~';

        public char EscapeCharacter { get; set; } = '\\';

        public char SubcomponentSeparator { get; set; } = '&';
    }
}
