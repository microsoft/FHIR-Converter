// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotLiquid;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Health.Fhir.Liquid.Converter.Hl7v2;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Health.Fhir.TemplateManagement.FunctionalTests
{
    public class FunctionalTests
    {
        private readonly string token;
        private readonly TemplateCollectionConfiguration _config = new TemplateCollectionConfiguration();
        private readonly IMemoryCache cache = new MemoryCache(new MemoryCacheOptions());
        private readonly string baseLayerTemplatePath = "TestData/TarGzFiles/DefaultTemplates.tar.gz";
        private readonly string userLayerTemplatePath = "TestData/TarGzFiles/userV2.tar.gz";
        private readonly string invalidTarGzPath = "TestData/TarGzFiles/invalid1.tar.gz";
        private readonly string invalidTemplatePath = "TestData/TarGzFiles/invalidTemplates.tar.gz";
        private readonly string testOneLayerImageReference;
        private readonly string testMultiLayerImageReference;
        private readonly string testInvalidImageReference;
        private readonly string testInvalidTemplateImageReference;
        private readonly ContainerRegistry _containerRegistry;
        private readonly ContainerRegistryInfo _containerRegistryInfo;

        public FunctionalTests()
        {
            _containerRegistry = new ContainerRegistry();
            _containerRegistryInfo = _containerRegistry.GetTestContainerRegistryInfo();
            if (_containerRegistryInfo == null)
            {
                return;
            }

            testOneLayerImageReference = _containerRegistryInfo.ContainerRegistryServer + "/templatetest:onelayer";
            testMultiLayerImageReference = _containerRegistryInfo.ContainerRegistryServer + "/templatetest:multilayers";
            testInvalidImageReference = _containerRegistryInfo.ContainerRegistryServer + "/templatetest:invalidlayers";
            testInvalidTemplateImageReference = _containerRegistryInfo.ContainerRegistryServer + "/templatetest:invalidtemplateslayers";
            token = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_containerRegistryInfo.ContainerRegistryUsername}:{_containerRegistryInfo.ContainerRegistryPassword}"));
            new Action(async () => await InitOneLayerImageAsync()).Invoke();
            new Action(async () => await InitMultiLayerImageAsync()).Invoke();
            new Action(async () => await InitInvalidTarGzImageAsync()).Invoke();
            new Action(async () => await InitInvalidTemplateImageAsync()).Invoke();
        }

        private void Lazy(Action p)
        {
            throw new NotImplementedException();
        }

        private async Task InitOneLayerImageAsync()
        {
            List<string> templateFiles = new List<string> { baseLayerTemplatePath };
            await _containerRegistry.GenerateTemplateImageAsync(testOneLayerImageReference, templateFiles);
        }

        private async Task InitMultiLayerImageAsync()
        {
            List<string> templateFiles = new List<string> { userLayerTemplatePath, baseLayerTemplatePath };
            await _containerRegistry.GenerateTemplateImageAsync(testMultiLayerImageReference, templateFiles);
        }

        private async Task InitInvalidTarGzImageAsync()
        {
            List<string> templateFiles = new List<string> { invalidTarGzPath };
            await _containerRegistry.GenerateTemplateImageAsync(testInvalidImageReference, templateFiles);
        }

        private async Task InitInvalidTemplateImageAsync()
        {
            List<string> templateFiles = new List<string> { invalidTemplatePath };
            await _containerRegistry.GenerateTemplateImageAsync(testInvalidTemplateImageReference, templateFiles);
        }

        public static IEnumerable<object[]> GetValidImageInfoWithTag()
        {
            yield return new object[] { new List<int> { 817 }, "templatetest", "onelayer" };
            yield return new object[] { new List<int> { 817, 767 }, "templatetest", "multilayers" };
        }

        public static IEnumerable<object[]> GetHl7v2DataAndTemplateImageReference()
        {
            yield return new object[] { @"..\..\..\..\..\data\SampleData\Hl7v2\IZ_1_1.1_Admin_Child_Max_Message.hl7", "VXU_V04", @"TestData\Expected\Hl7v2\VXU_V04\IZ_1_1.1_Admin_Child_Max_Message-expected.json" };
            yield return new object[] { @"..\..\..\..\..\data\SampleData\Hl7v2\LAB-ORU-1.hl7", "ORU_R01", @"TestData\Expected\Hl7v2\ORU_R01\LAB-ORU-1-expected.json" };
            yield return new object[] { @"..\..\..\..\..\data\SampleData\Hl7v2\MDHHS-OML-O21-1.hl7", "OML_O21", @"TestData\Expected\Hl7v2\OML_O21\MDHHS-OML-O21-1-expected.json" };
        }

        public static IEnumerable<object[]> GetHl7v2DataAndTemplateImageReferenceWithoutGivenTemplate()
        {
            yield return new object[] { @"..\..\..\..\..\data\SampleData\Hl7v2\IZ_1_1.1_Admin_Child_Max_Message.hl7", "VXU_V04" };
            yield return new object[] { @"..\..\..\..\..\data\SampleData\Hl7v2\LAB-ORU-1.hl7", "ORU_R01" };
            yield return new object[] { @"..\..\..\..\..\data\SampleData\Hl7v2\MDHHS-OML-O21-1.hl7", "OML_O21" };
        }

        public static IEnumerable<object[]> GetNotExistImageInfo()
        {
            yield return new object[] { "templatetest", "notExist" };
            yield return new object[] { "notExist", "multilayers" };
        }

        public static IEnumerable<object[]> GetInvalidImageReference()
        {
            yield return new object[] { "testacr.azurecr.io@v1" };
            yield return new object[] { "testacr.azurecr.io:templateset:v1" };
            yield return new object[] { "testacr.azurecr.io_v1" };
            yield return new object[] { "testacr.azurecr.io:v1" };
            yield return new object[] { "testacr.azurecr.io/" };
            yield return new object[] { "testacr.azurecr.io/name:" };
            yield return new object[] { "testacr.azurecr.io/name@" };
        }

        [Fact]
        public async Task GiveImageReference_WhenGetTemplateCollection_IfImageTooLarge_ExceptionWillBeThrownAsync()
        {
            if (_containerRegistryInfo == null)
            {
                return;
            }

            var config = new TemplateCollectionConfiguration() { TemplateCollectionSizeLimitMegabytes = 0 };
            string imageReference = testOneLayerImageReference;
            TemplateCollectionProviderFactory factory = new TemplateCollectionProviderFactory(cache, Options.Create(config));
            var templateCollectionProvider = factory.CreateTemplateCollectionProvider(imageReference, token);
            await Assert.ThrowsAsync<ImageTooLargeException>(async () => await templateCollectionProvider.GetTemplateCollectionAsync());
        }

        [Fact]
        public async Task GiveImageReference_WhenGetTemplateCollection_IfTemplateParsedFailed_ExceptionWillBeThrownAsync()
        {
            if (_containerRegistryInfo == null)
            {
                return;
            }

            string imageReference = testInvalidTemplateImageReference;
            TemplateCollectionProviderFactory factory = new TemplateCollectionProviderFactory(cache, Options.Create(_config));
            var templateCollectionProvider = factory.CreateTemplateCollectionProvider(imageReference, token);
            await Assert.ThrowsAsync<TemplateParseException>(async () => await templateCollectionProvider.GetTemplateCollectionAsync());
        }

        [Fact]
        public async Task GiveImageReference_WhenGetTemplateCollection_IfImageDecompressedFailed_ExceptionWillBeThrownAsync()
        {
            if (_containerRegistryInfo == null)
            {
                return;
            }

            string imageReference = testInvalidImageReference;
            TemplateCollectionProviderFactory factory = new TemplateCollectionProviderFactory(cache, Options.Create(_config));
            var templateCollectionProvider = factory.CreateTemplateCollectionProvider(imageReference, token);
            await Assert.ThrowsAsync<ArtifactDecompressException>(async () => await templateCollectionProvider.GetTemplateCollectionAsync());
        }

        [Theory]
        [MemberData(nameof(GetInvalidImageReference))]
        public void GiveInvalidImageReference_WhenGetTemplateCollection_ExceptionWillBeThrownAsync(string imageReference)
        {
            if (_containerRegistryInfo == null)
            {
                return;
            }

            TemplateCollectionProviderFactory factory = new TemplateCollectionProviderFactory(cache, Options.Create(_config));
            Assert.Throws<ImageReferenceException>(() => factory.CreateTemplateCollectionProvider(imageReference, token));
        }

        [Theory]
        [MemberData(nameof(GetNotExistImageInfo))]
        public async Task GiveImageReference_WhenGetTemplateCollection_IfImageNotFound_ExceptionWillBeThrownAsync(string imageName, string tag)
        {
            if (_containerRegistryInfo == null)
            {
                return;
            }

            string imageReference = string.Format("{0}/{1}:{2}", _containerRegistryInfo.ContainerRegistryServer, imageName, tag);
            TemplateCollectionProviderFactory factory = new TemplateCollectionProviderFactory(cache, Options.Create(_config));
            var templateCollectionProvider = factory.CreateTemplateCollectionProvider(imageReference, token);
            await Assert.ThrowsAsync<ImageNotFoundException>(async () => await templateCollectionProvider.GetTemplateCollectionAsync());
        }

        [Theory]
        [MemberData(nameof(GetValidImageInfoWithTag))]
        public async Task GiveImageReference_WhenGetTemplateCollection_IfTokenInvalid_ExceptionWillBeThrownAsync(object _, string imageName, string tag)
        {
            if (_containerRegistryInfo == null)
            {
                return;
            }

            var fakeToken = "fakeToken";
            string imageReference = string.Format("{0}/{1}:{2}", _containerRegistryInfo.ContainerRegistryServer, imageName, tag);
            TemplateCollectionProviderFactory factory = new TemplateCollectionProviderFactory(cache, Options.Create(_config));
            var templateCollectionProvider = factory.CreateTemplateCollectionProvider(imageReference, fakeToken);
            await Assert.ThrowsAsync<ContainerRegistryAuthenticationException>(async () => await templateCollectionProvider.GetTemplateCollectionAsync());
            var emptyToken = string.Empty;
            Assert.Throws<ContainerRegistryAuthenticationException>(() => factory.CreateTemplateCollectionProvider(imageReference, emptyToken));
        }

        [Theory]
        [MemberData(nameof(GetValidImageInfoWithTag))]
        public async Task GiveImageReference_WhenGetTemplateCollection_ACorrectTemplateCollectionWillBeReturnedAsync(List<int> expectedTemplatesCounts, string imageName, string tag)
        {
            if (_containerRegistryInfo == null)
            {
                return;
            }

            string imageReference = string.Format("{0}/{1}:{2}", _containerRegistryInfo.ContainerRegistryServer, imageName, tag);
            TemplateCollectionProviderFactory factory = new TemplateCollectionProviderFactory(cache, Options.Create(_config));
            var templateCollectionProvider = factory.CreateTemplateCollectionProvider(imageReference, token);
            var templateCollection = await templateCollectionProvider.GetTemplateCollectionAsync();
            Assert.Equal(expectedTemplatesCounts.Count(), templateCollection.Count());
            for (var i = 0; i < expectedTemplatesCounts.Count(); i++)
            {
                Assert.Equal(expectedTemplatesCounts[i], templateCollection[i].Count());
            }
        }

        [Theory]
        [MemberData(nameof(GetHl7v2DataAndTemplateImageReference))]
        public async Task GetTemplateCollectionFromACR_WhenGivenHl7v2DataForConverting__ExpectedFhirResourceShouldBeReturnedAsync(string hl7v2Data, string entryTemplate, string expectedResult)
        {
            if (_containerRegistryInfo == null)
            {
                return;
            }

            TemplateCollectionProviderFactory factory = new TemplateCollectionProviderFactory(cache, Options.Create(_config));
            var templateCollectionProvider = factory.CreateTemplateCollectionProvider(testOneLayerImageReference, token);
            var templateCollection = await templateCollectionProvider.GetTemplateCollectionAsync();
            TestByTemplate(hl7v2Data, expectedResult, entryTemplate, templateCollection);
        }

        [Theory]
        [MemberData(nameof(GetHl7v2DataAndTemplateImageReferenceWithoutGivenTemplate))]
        public async Task GetTemplateCollectionFromACR_WhenGivenHl7v2DataForConverting_IfTemplateNotExist_ExeceptionWillBeThrownAsync(string hl7v2Data, string entryTemplate)
        {
            if (_containerRegistryInfo == null)
            {
                return;
            }

            TemplateCollectionProviderFactory factory = new TemplateCollectionProviderFactory(cache, Options.Create(_config));
            var templateCollectionProvider = factory.CreateTemplateCollectionProvider(testMultiLayerImageReference, token);
            await Assert.ThrowsAsync<ArgumentNullException>(async () => TestByTemplate(hl7v2Data, null, entryTemplate, await templateCollectionProvider.GetTemplateCollectionAsync()));
        }

        [Fact]
        public async Task GiveDefaultImageReference_WhenGetTemplateCollectionWithEmptyToken_DefaultTemplatesWillBeReturnedAsync()
        {
            if (_containerRegistryInfo == null)
            {
                return;
            }

            int defaultTemplatesCounts = 817;
            string imageReference = "MicrosoftHealth/FhirConverter:default";
            TemplateCollectionProviderFactory factory = new TemplateCollectionProviderFactory(cache, Options.Create(_config));
            var templateCollectionProvider = factory.CreateTemplateCollectionProvider(imageReference, string.Empty);
            var templateCollection = await templateCollectionProvider.GetTemplateCollectionAsync();
            Assert.Single(templateCollection);
            Assert.Equal(defaultTemplatesCounts, templateCollection.First().Count());
        }

        private void TestByTemplate(string inputFile, string expectedFile, string entryTemplate, List<Dictionary<string, Template>> templateProvider)
        {
            var hl7v2Processor = new Hl7v2Processor();
            var inputContent = File.ReadAllText(inputFile);
            var expectedContent = File.ReadAllText(expectedFile);
            var actualContent = hl7v2Processor.Convert(inputContent, entryTemplate, new Hl7v2TemplateProvider(templateProvider));

            // Remove ID
            var regex = new Regex(@"(?<=(""urn:uuid:|""|/))([A-Za-z0-9\-]{36})(?="")");
            expectedContent = regex.Replace(expectedContent, string.Empty);
            actualContent = regex.Replace(actualContent, string.Empty);

            // Normalize time zone
            JsonSerializer serializer = new JsonSerializer { DateTimeZoneHandling = DateTimeZoneHandling.Utc };
            var expectedObject = serializer.Deserialize<JObject>(new JsonTextReader(new StringReader(expectedContent)));
            var actualObject = serializer.Deserialize<JObject>(new JsonTextReader(new StringReader(actualContent)));
            Assert.True(JToken.DeepEquals(expectedObject, actualObject));
        }
    }
}