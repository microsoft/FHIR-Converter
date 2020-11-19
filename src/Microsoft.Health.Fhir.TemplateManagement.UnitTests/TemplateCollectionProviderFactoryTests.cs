// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Health.Fhir.TemplateManagement.ArtifactProviders;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Xunit;

namespace Microsoft.Health.Fhir.TemplateManagement.UnitTests
{
    public class TemplateCollectionProviderFactoryTests
    {
        private readonly TemplateCollectionConfiguration _config = new TemplateCollectionConfiguration();
        private readonly string _token = "Basic FakeToken";
        private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions() { SizeLimit = 100000000 });

        public static IEnumerable<object[]> GetValidImageInfoWithTag()
        {
            yield return new object[] { "testacr.azurecr.io/templateset:v1" };
            yield return new object[] { "testacr.azurecr.io/templateset:v2" };
        }

        public static IEnumerable<object[]> GetValidImageInfoWithDigest()
        {
            yield return new object[] { "testacr.azurecr.io/templateset@sha256:87b600f187bde328de7a8fd98a4f3dad1ce71803c5fc4eced77b3349da136a5f" };
            yield return new object[] { "testacr.azurecr.io/templateset@sha256:95074605bbe28f191fffd85a8e3a581d6fdbe440908c3b5f36dd1b532907a530" };
        }

        [Theory]
        [MemberData(nameof(GetValidImageInfoWithTag))]
        [MemberData(nameof(GetValidImageInfoWithDigest))]
        public void GiveImageReference_WhenGetTemplateProvider_ACorrectTemplateProviderWillBeReturnedAsync(string imageReference)
        {
            TemplateCollectionProviderFactory factory = new TemplateCollectionProviderFactory(_cache, Options.Create(_config));
            Assert.NotNull(factory.CreateProvider(imageReference, _token));
            Assert.NotNull(factory.CreateTemplateCollectionProvider(imageReference, _token));
        }

        [Fact]
        public void GiveDefaultImageReference_WhenGetTemplateProviderWithEmptyToken_ADefaultTemplateProviderWillBeReturnedAsync()
        {
            string imageReference = "MicrosoftHealth/FhirConverter:default";
            TemplateCollectionProviderFactory factory = new TemplateCollectionProviderFactory(_cache, Options.Create(_config));
            Assert.NotNull(factory.CreateProvider(imageReference, string.Empty));
            Assert.NotNull(factory.CreateTemplateCollectionProvider(imageReference, string.Empty));
        }

        [Fact]
        public void GivenAnInvalidToken_WhenGetTemplateProvider_AnExceptionWillBeThrown()
        {
            string imageReference = "testacr.azurecr.io/templates:test1";
            TemplateCollectionProviderFactory factory = new TemplateCollectionProviderFactory(_cache, Options.Create(_config));
            Assert.Throws<ContainerRegistryAuthenticationException>(() => factory.CreateProvider(imageReference, string.Empty));
            Assert.Throws<ContainerRegistryAuthenticationException>(() => factory.CreateProvider(imageReference, string.Empty));
        }

        [Fact]
        public void GivenAWrongDefaultTemplatePath_WhenInitDefaultTemplate_ExceptionWillBeThrown()
        {
            TemplateCollectionProviderFactory factory = new TemplateCollectionProviderFactory(_cache, Options.Create(_config));
            Assert.Throws<DefaultTemplatesInitializeException>(() => factory.InitDefaultTemplates("WrongPath"));
        }
    }
}
