// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.IO;
using Antlr4.Runtime;

namespace Microsoft.Health.Fhir.Liquid.Converter.OutputProcessors
{
    public class JsonParserErrorListener : BaseErrorListener
    {
        private bool _includeMessage = false;

        public JsonParserErrorListener(bool includeMessage = false)
        {
            _includeMessage = includeMessage;
        }

        public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            var syntaxError = "line " + line + ":" + charPositionInLine;

            if (_includeMessage)
            {
                syntaxError = syntaxError + " message: " + msg;
            }

            output.WriteLine(syntaxError);
        }
    }
}
