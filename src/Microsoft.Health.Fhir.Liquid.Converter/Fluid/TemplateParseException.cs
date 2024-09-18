// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Runtime.Serialization;

namespace Microsoft.Health.Fhir.Liquid.Converter.Fluid
{
    [Serializable]
    internal class TemplateParseException : Exception
    {
        public TemplateParseException()
        {
        }

        public TemplateParseException(string message)
            : base(message)
        {
        }

        public TemplateParseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected TemplateParseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}