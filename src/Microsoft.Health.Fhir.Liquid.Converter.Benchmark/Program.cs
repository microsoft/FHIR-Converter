// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using BenchmarkDotNet.Running;
using Microsoft.Health.Fhir.Liquid.Converter.Benchmark;

var summary = BenchmarkRunner.Run<Hl7v2Benchmark>();
