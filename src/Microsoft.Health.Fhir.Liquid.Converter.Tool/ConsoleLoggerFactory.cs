// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.Logging;

namespace Microsoft.Health.Fhir.Liquid.Converter.Tool
{
    public class ConsoleLoggerFactory
    {
        private static ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

        public static ILogger CreateLogger(Type type) => loggerFactory.CreateLogger(type);

        public static ILogger<T> CreateLogger<T>() => loggerFactory.CreateLogger<T>();
    }
}
