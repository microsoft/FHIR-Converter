// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using EnsureThat;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Logging.Telemetry;
using Xunit.Abstractions;

namespace Microsoft.Health.Fhir.Liquid.Converter.Telemetry
{
    public class XunitTelemetryLogger : ITelemetryLogger
    {
        private readonly ITestOutputHelper _output;

        public XunitTelemetryLogger(ITestOutputHelper outputHelper)
        {
            _output = EnsureArg.IsNotNull(outputHelper, nameof(outputHelper));
        }

        public void LogError(Exception ex)
        {
            _output.WriteLine($"Error: {ex.Message}");
        }

        public void LogMetric(Metric metric, double metricValue)
        {
            _output.WriteLine($"Metric \"{metric.Name}\" Value: {metricValue}");
        }

        public void LogTrace(string message)
        {
            _output.WriteLine(message);
        }
    }
}
