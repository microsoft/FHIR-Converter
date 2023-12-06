// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotLiquid;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Health.Common;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.TemplateManagement.ArtifactProviders;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Xunit;

namespace Microsoft.Health.Fhir.TemplateManagement.UnitTests.Providers
{
    public class DefaultTemplateCollectionProviderTests
    {
        private readonly TemplateCollectionConfiguration _config;
        private readonly IMemoryCache _cache;
        private readonly IConvertDataTemplateCollectionProvider _defaultTemplateCollectionProvider;

        public DefaultTemplateCollectionProviderTests()
        {
            _config = new TemplateCollectionConfiguration();
            _cache = new MemoryCache(new MemoryCacheOptions());
            _defaultTemplateCollectionProvider = new DefaultTemplateCollectionProvider(_cache, _config);
        }

        [Fact]
        public async Task GivenDefaultTemplateProvider_WhenGetTemplateCollectionAsync_ThenDefaultTemplatesAreReturned()
        {
            var templateCollection = await _defaultTemplateCollectionProvider.GetTemplateCollectionAsync(CancellationToken.None);

            Assert.Single(templateCollection);

            // Verify expected number of templates per data type as per templates packaged from the data/Templates directory.
            Assert.Equal(2002, templateCollection[0].Count);
            Assert.Equal(915, templateCollection[0].Where(x => x.Key.StartsWith(DataType.Hl7v2.ToString())).Count());
            Assert.Equal(821, templateCollection[0].Where(x => x.Key.StartsWith(DataType.Ccda.ToString())).Count());
            Assert.Equal(2, templateCollection[0].Where(x => x.Key.StartsWith(DataType.Json.ToString())).Count());
            Assert.Equal(264, templateCollection[0].Where(x => x.Key.StartsWith(DataType.Fhir.ToString())).Count());

            // Verify templates are cached
            Assert.True(_cache.TryGetValue("cached-default-templates", out List<Dictionary<string, Template>> cachedTemplateCollection));
            Assert.Equal(templateCollection, cachedTemplateCollection);

            // Call again to test cache
            cachedTemplateCollection = await _defaultTemplateCollectionProvider.GetTemplateCollectionAsync(CancellationToken.None);

            Assert.NotNull(cachedTemplateCollection);
            Assert.Equal(templateCollection, cachedTemplateCollection);
        }
    }
}
