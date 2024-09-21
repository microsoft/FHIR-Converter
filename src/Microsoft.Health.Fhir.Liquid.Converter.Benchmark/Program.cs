// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Fluid;
using Microsoft.Health.Fhir.Liquid.Converter;
using Microsoft.Health.Fhir.Liquid.Converter.Benchmark;
using Microsoft.Health.Fhir.Liquid.Converter.Fluid;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Models.Hl7v2;

// ITemplateProvider<IFluidTemplate> provider = new SimpleFileSystemTemplateProvider(new DirectoryInfo(BenchmarkUtils.Hl7v2TemplateDirectory), strictMode: false);
ITemplateProvider<IFluidTemplate> provider = new SimpleFileSystemTemplateProvider(new DirectoryInfo(BenchmarkUtils.LocalTestTemplates), strictMode: true);
Hl7v2TraceInfo traceInfo = new Hl7v2TraceInfo();
FluidHl7v2Processor processor = new FluidHl7v2Processor(new ProcessorSettings(), FhirConverterLogging.CreateLogger<FluidHl7v2Processor>());
var (template, datafile) = BenchmarkUtils.GetHl7v2TemplatesAndDataFiles()["Test ADT_A01 0"];

string dataContent = File.ReadAllText(datafile);

string result = processor.Convert(dataContent, "Test", provider, traceInfo);

// string result = processor.Convert(dataContent, template, provider, traceInfo);

var summary = BenchmarkRunner.Run<Hl7v2DotLiquidBenchmark>();
