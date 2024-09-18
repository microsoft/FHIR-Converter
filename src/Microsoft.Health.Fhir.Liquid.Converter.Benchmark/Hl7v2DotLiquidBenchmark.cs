// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Models.Hl7v2;
using Microsoft.Health.Fhir.Liquid.Converter.Processors;
using Microsoft.Health.Fhir.TemplateManagement.Configurations;
using Microsoft.Health.Fhir.TemplateManagement.Factory;

namespace Microsoft.Health.Fhir.Liquid.Converter.Benchmark
{
    [SimpleJob(launchCount: 1, warmupCount: 1, iterationCount: 10, invocationCount: 10)]
    [MemoryDiagnoser]
    public class Hl7v2DotLiquidBenchmark
    {
        private string? _template;
        private string? _data;
        private readonly Hl7v2TraceInfo _traceInfo;
        private readonly Hl7v2Processor _processor;
        private readonly ITemplateProvider _templateProvider;

        public Hl7v2DotLiquidBenchmark()
        {
            _traceInfo = new Hl7v2TraceInfo();
            _processor = new Hl7v2Processor(new ProcessorSettings(), FhirConverterLogging.CreateLogger<Hl7v2Processor>());

            // Build template collection from embedded default template resources
            var templateCollection = new ConvertDataTemplateCollectionProviderFactory(new MemoryCache(new MemoryCacheOptions()), Options.Create(new TemplateCollectionConfiguration()))
                .CreateTemplateCollectionProvider()
                .GetTemplateCollectionAsync(CancellationToken.None)
                .GetAwaiter()
                .GetResult();

            _templateProvider = new TemplateProvider(templateCollection, isDefaultTemplateProvider: true);
        }

        [ParamsSource(nameof(GetTests))]
        public string? TestName { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            var (template, datafile) = BenchmarkUtils.GetHl7v2TemplatesAndDataFiles()[TestName ?? throw new ArgumentNullException(nameof(TestName))];
            _template = template;
            _data = File.ReadAllText(datafile);
        }

        [Benchmark]
        public string ConvertHl7v2Message()
        {
            return _processor.Convert(_data, _template, _templateProvider, _traceInfo);
        }

        public static IEnumerable<string> GetTests() => BenchmarkUtils.GetHl7v2TemplatesAndDataFiles().Keys;
    }
}