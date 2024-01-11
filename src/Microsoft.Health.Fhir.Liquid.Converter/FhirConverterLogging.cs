// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.Logging;

namespace Microsoft.Health.Fhir.Liquid.Converter
{
    public static class FhirConverterLogging
    {
        public static ILoggerFactory LoggerFactory { get; set; } = new LoggerFactory();

        public static ILogger CreateLogger(Type type) => LoggerFactory.CreateLogger(type);

        public static ILogger<T> CreateLogger<T>() => LoggerFactory.CreateLogger<T>();
    }
}
