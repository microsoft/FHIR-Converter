// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Parlot.Fluent;
using static Parlot.Fluent.Parsers;

namespace Microsoft.Health.Fhir.Liquid.Converter.Fluid.Tags
{
    internal static class EvaluateTagParser
    {
        public static readonly Parser<EvaluateTag> Instance = CreateParser();

        private static Parser<EvaluateTag> CreateParser()
        {
            // {% evaluate patientId using 'ID/Patient' PID: firstSegments.PID, type: 'First' -%}
            // {% assign fullPatientId = patientId | prepend: 'Patient/' -%}
            // {% evaluate messageHeaderId using 'ID/MessageHeader' MSH: firstSegments.MSH -%}

            var outputVariable = Terms.NonWhiteSpace().ElseError("Excepted output variable");
            var usingClause = Terms.Text("using").ElseError("Expected using");
            var targetTemplate = Terms.String(StringLiteralQuotes.Single).ElseError("Expected target template");
            var oneAttribute = Terms.NonWhiteSpace()
                .And(Terms.Char(':'))
                .And(Terms.NonWhiteSpace());
            var attributes = Separated(Terms.Text(","), oneAttribute);

            var parser = outputVariable
                .AndSkip(usingClause)
                .And(targetTemplate)
                // .And(attributes)
                .Then(v =>
                {
                    return new EvaluateTag
                    {
                        OutputVariable = v.Item1.ToString(),
                        TargetTemplate = v.Item2.ToString(),

                        // Attributes = v.Item3
                    };
                });

            return parser;
        }
    }
}
