// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using Microsoft.Health.Fhir.Liquid.Converter.Models;

namespace Microsoft.Health.Fhir.Liquid.Converter.Processors
{
    public interface IFhirConverter
    {
        public string Convert(string data, string rootTemplate, ITemplateProvider templateProvider, TraceInfo traceInfo = null);

        public string Convert(string data, string rootTemplate, ITemplateProvider templateProvider, CancellationToken cancellationToken, TraceInfo traceInfo = null);

        public string Convert(string data, string rootTemplate, ITemplateProvider templateProvider, IDictionary<string, string> variables, TraceInfo traceInfo = null);

        public string Convert(string data, string rootTemplate, ITemplateProvider templateProvider, IDictionary<string, string> variables, CancellationToken cancellationToken, TraceInfo traceInfo = null);

        public string Convert(string data, string rootTemplate, ITemplateProvider templateProvider, IList<VariableDefinition> variables, TraceInfo traceInfo = null);

        public string Convert(string data, string rootTemplate, ITemplateProvider templateProvider, IList<VariableDefinition> variables, CancellationToken cancellationToken, TraceInfo traceInfo = null);
    }
}
