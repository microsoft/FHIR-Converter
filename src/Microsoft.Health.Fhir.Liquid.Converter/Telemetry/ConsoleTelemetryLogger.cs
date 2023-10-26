// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Logging.Telemetry;

namespace Microsoft.Health.Fhir.Liquid.Converter.Telemetry
{
    /// <summary>
    /// This is a simple logger to print out the telemetry in the console.
    /// Note: this will not work for Xunit.
    /// </summary>
    public class ConsoleTelemetryLogger : ITelemetryLogger
    {
        public void LogError(Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        public void LogMetric(Metric metric, double metricValue)
        {
            Console.WriteLine($"Metric \"{metric.Name}\" Value: {metricValue}");
        }

        public void LogTrace(string message)
        {
            Console.WriteLine(message);
        }
    }
}
