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
using System.Threading;
using System.Threading.Tasks;
using DotLiquid;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Health.Fhir.Liquid.Converter;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Processors;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Health.Fhir.TemplateManagement.FunctionalTests
{
    public class TemplateCollectionFunctionalTests : IAsyncLifetime
    {
        private readonly string token;
        private readonly TemplateCollectionConfiguration _config = new TemplateCollectionConfiguration();
        private readonly IMemoryCache cache = new MemoryCache(new MemoryCacheOptions());
        private readonly string baseLayerTemplatePath = "TestData/TarGzFiles/baseLayer.tar.gz";
        private readonly string userLayerTemplatePath = "TestData/TarGzFiles/userV2.tar.gz";
        private readonly string invalidTarGzPath = "TestData/TarGzFiles/invalid1.tar.gz";
        private readonly string invalidTemplatePath = "TestData/TarGzFiles/invalidTemplates.tar.gz";
        private readonly string _defaultHl7v2TemplateImageReference = "microsofthealth/hl7v2templates:default";
        private readonly string _defaultCcdaTemplateImageReference = "microsofthealth/ccdatemplates:default";
        private readonly string _defaultJsonTemplateImageReference = "microsofthealth/jsontemplates:default";
        private readonly string testOneLayerImageReference;
        private readonly string testMultiLayerImageReference;
        private readonly string testInvalidImageReference;
        private readonly string testInvalidTemplateImageReference;
        private readonly ContainerRegistry _containerRegistry = new ContainerRegistry();
        private readonly ContainerRegistryInfo _containerRegistryInfo;
        private static readonly string _templateDirectory = Path.Join("..", "..", "data", "Templates");
        private static readonly string _sampleDataDirectory = Path.Join("..", "..", "data", "SampleData");

        public TemplateCollectionFunctionalTests()
        {
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
        }

        public async Task InitializeAsync()
        {
            if (_containerRegistryInfo == null)
            {
                return;
            }

            await InitOneLayerImageAsync();
            await InitMultiLayerImageAsync();
            await InitInvalidTarGzImageAsync();
            await InitInvalidTemplateImageAsync();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        public static IEnumerable<object[]> GetValidImageInfoWithTag()
        {
            yield return new object[] { new List<int> { 838 }, "templatetest", "onelayer" };
            yield return new object[] { new List<int> { 767, 838 }, "templatetest", "multilayers" };
        }

        public static IEnumerable<object[]> GetHl7v2DataAndEntryTemplate()
        {
            var data = new List<object[]>
            {
                new object[] { @"ADT01-23.hl7", @"ADT_A01" },
                new object[] { @"IZ_1_1.1_Admin_Child_Max_Message.hl7", @"VXU_V04" },
                new object[] { @"LAB-ORU-1.hl7", @"ORU_R01" },
                new object[] { @"MDHHS-OML-O21-1.hl7", @"OML_O21" },
            };
            return data.Select(item => new object[]
            {
                Path.Join(_sampleDataDirectory, "Hl7v2", Convert.ToString(item[0])),
                Convert.ToString(item[1]),
            });
        }

        public static IEnumerable<object[]> GetHl7v2DataAndTemplateSources()
        {
            var data = new List<object[]>
            {
                new object[] { @"ADT01-23.hl7", @"ADT_A01" },
                new object[] { @"IZ_1_1.1_Admin_Child_Max_Message.hl7", @"VXU_V04" },
                new object[] { @"LAB-ORU-1.hl7", @"ORU_R01" },
                new object[] { @"MDHHS-OML-O21-1.hl7", @"OML_O21" },
            };
            return data.Select(item => new object[]
            {
                Path.Join(_sampleDataDirectory, "Hl7v2", Convert.ToString(item[0])),
                Path.Join(_templateDirectory, "Hl7v2"),
                Convert.ToString(item[1]),
            });
        }

        public static IEnumerable<object[]> GetCcdaDataAndTemplateSources()
        {
            var data = new List<object[]>
            {
                new object[] { @"170.314B2_Amb_CCD.ccda", @"CCD" },
                new object[] { @"C-CDA_R2-1_CCD.xml.ccda", @"CCD" },
                new object[] { @"CCD.ccda", @"CCD" },
                new object[] { @"CCD-Parent-Document-Replace-C-CDAR2.1.ccda", @"CCD" },
            };
            return data.Select(item => new object[]
            {
                Path.Join(_sampleDataDirectory, "Ccda", Convert.ToString(item[0])),
                Path.Join(_templateDirectory, "Ccda"),
                Convert.ToString(item[1]),
            });
        }

        public static IEnumerable<object[]> GetJsonDataAndTemplateSources()
        {
            var data = new List<object[]>
            {
                new object[] { @"ExamplePatient.json", @"ExamplePatient" },
                new object[] { @"Stu3ChargeItem.json", @"Stu3ChargeItem" },
            };
            return data.Select(item => new object[]
            {
                Path.Join(_sampleDataDirectory, "Json", Convert.ToString(item[0])),
                Path.Join(_templateDirectory, "Json"),
                Convert.ToString(item[1]),
            });
        }

        public static IEnumerable<object[]> GetNotExistImageInfo()
        {
            yield return new object[] { "templatetest", "notexist" };
            yield return new object[] { "notexist", "multilayers" };
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

        public static IEnumerable<object[]> GetDefaultTemplatesInfo()
        {
            yield return new object[] { "microsofthealth/fhirconverter:default", "Hl7v2" };
            yield return new object[] { "microsofthealth/hl7v2templates:default", "Hl7v2" };
            yield return new object[] { "microsofthealth/ccdatemplates:default", "Ccda" };
            yield return new object[] { "microsofthealth/jsontemplates:default", "Json" };
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
            await Assert.ThrowsAsync<ArtifactArchiveException>(async () => await templateCollectionProvider.GetTemplateCollectionAsync());
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
        public async Task GiveImageReference_WhenGetTemplateCollection_IfTokenInvalid_ExceptionWillBeThrownAsync(List<int> expectedTemplatesCounts, string imageName, string tag)
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
            Assert.NotNull(expectedTemplatesCounts);
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
        [MemberData(nameof(GetHl7v2DataAndEntryTemplate))]
        public async Task GetTemplateCollectionFromAcr_WhenGivenHl7v2DataForConverting__ExpectedFhirResourceShouldBeReturnedAsync(string hl7v2Data, string entryTemplate)
        {
            if (_containerRegistryInfo == null)
            {
                return;
            }

            TemplateCollectionProviderFactory factory = new TemplateCollectionProviderFactory(cache, Options.Create(_config));
            var templateCollectionProvider = factory.CreateTemplateCollectionProvider(testOneLayerImageReference, token);
            var templateCollection = await templateCollectionProvider.GetTemplateCollectionAsync();
            TestByTemplate(hl7v2Data, entryTemplate, templateCollection);
        }

        [Theory]
        [MemberData(nameof(GetHl7v2DataAndEntryTemplate))]
        public async Task GetTemplateCollectionFromAcr_WhenGivenHl7v2DataForConverting_IfTemplateNotExist_ExceptionWillBeThrownAsync(string hl7v2Data, string entryTemplate)
        {
            if (_containerRegistryInfo == null)
            {
                return;
            }

            TemplateCollectionProviderFactory factory = new TemplateCollectionProviderFactory(cache, Options.Create(_config));
            var templateCollectionProvider = factory.CreateTemplateCollectionProvider(testMultiLayerImageReference, token);
            await Assert.ThrowsAsync<RenderException>(async () => TestByTemplate(hl7v2Data, entryTemplate, await templateCollectionProvider.GetTemplateCollectionAsync()));
        }

        [Theory]
        [MemberData(nameof(GetDefaultTemplatesInfo))]
        public async Task GiveDefaultImageReference_WhenGetTemplateCollectionWithEmptyToken_DefaultTemplatesWillBeReturnedAsync(string imageReference, string expectedTemplatesFolder)
        {
            TemplateCollectionProviderFactory factory = new TemplateCollectionProviderFactory(cache, Options.Create(_config));
            var templateCollectionProvider = factory.CreateTemplateCollectionProvider(imageReference, string.Empty);
            var templateCollection = await templateCollectionProvider.GetTemplateCollectionAsync();
            Assert.Single(templateCollection);

            // metadata.json will not be returned as templates.
            Assert.Equal(Directory.GetFiles(Path.Join(_templateDirectory, expectedTemplatesFolder), "*", SearchOption.AllDirectories).Length - 1, templateCollection.First().Count());
        }

        // Conversion results of DefaultTemplates.tar.gz and default template folder should be the same.
        [Theory]
        [MemberData(nameof(GetHl7v2DataAndTemplateSources))]
        public async Task GivenHl7v2SameInputData_WithDifferentTemplateSource_WhenConvert_ResultShouldBeIdentical(string inputFile, string defaultTemplateDirectory, string rootTemplate)
        {
            var folderTemplateProvider = new TemplateProvider(defaultTemplateDirectory, DataType.Hl7v2);

            var templateProviderFactory = new TemplateCollectionProviderFactory(new MemoryCache(new MemoryCacheOptions()), Options.Create(new TemplateCollectionConfiguration()));
            var templateProvider = templateProviderFactory.CreateTemplateCollectionProvider(_defaultHl7v2TemplateImageReference, string.Empty);
            var imageTemplateProvider = new TemplateProvider(await templateProvider.GetTemplateCollectionAsync(CancellationToken.None));

            var hl7v2Processor = new Hl7v2Processor();
            var inputContent = File.ReadAllText(inputFile);

            var imageResult = hl7v2Processor.Convert(inputContent, rootTemplate, imageTemplateProvider);
            var folderResult = hl7v2Processor.Convert(inputContent, rootTemplate, folderTemplateProvider);

            var regex = new Regex(@"<div .*>.*</div>");
            imageResult = regex.Replace(imageResult, string.Empty);
            folderResult = regex.Replace(folderResult, string.Empty);

            Assert.Equal(imageResult, folderResult);
        }

        [Theory]
        [MemberData(nameof(GetCcdaDataAndTemplateSources))]
        public async Task GivenCcdaSameInputData_WithDifferentTemplateSource_WhenConvert_ResultShouldBeIdentical(string inputFile, string defaultTemplateDirectory, string rootTemplate)
        {
            var folderTemplateProvider = new TemplateProvider(defaultTemplateDirectory, DataType.Ccda);

            var templateProviderFactory = new TemplateCollectionProviderFactory(new MemoryCache(new MemoryCacheOptions()), Options.Create(new TemplateCollectionConfiguration()));
            var templateProvider = templateProviderFactory.CreateTemplateCollectionProvider(_defaultCcdaTemplateImageReference, string.Empty);
            var imageTemplateProvider = new TemplateProvider(await templateProvider.GetTemplateCollectionAsync(CancellationToken.None));

            var ccdaProcessor = new CcdaProcessor();
            var inputContent = File.ReadAllText(inputFile);

            var imageResult = ccdaProcessor.Convert(inputContent, rootTemplate, imageTemplateProvider);
            var folderResult = ccdaProcessor.Convert(inputContent, rootTemplate, folderTemplateProvider);

            var imageResultObject = JObject.Parse(imageResult);
            var folderResultObject = JObject.Parse(folderResult);

            // Remove DocumentReference, where date is different every time conversion is run and gzip result is OS dependent
            imageResultObject["entry"]?.Last()?.Remove();
            folderResultObject["entry"]?.Last()?.Remove();

            Assert.True(JToken.DeepEquals(imageResultObject, folderResultObject));
        }

        [Theory]
        [MemberData(nameof(GetJsonDataAndTemplateSources))]
        public async Task GivenJsonSameInputData_WithDifferentTemplateSource_WhenConvert_ResultShouldBeIdentical(string inputFile, string defaultTemplateDirectory, string rootTemplate)
        {
            var folderTemplateProvider = new TemplateProvider(defaultTemplateDirectory, DataType.Json);

            var templateProviderFactory = new TemplateCollectionProviderFactory(new MemoryCache(new MemoryCacheOptions()), Options.Create(new TemplateCollectionConfiguration()));
            var templateProvider = templateProviderFactory.CreateTemplateCollectionProvider(_defaultJsonTemplateImageReference, string.Empty);
            var imageTemplateProvider = new TemplateProvider(await templateProvider.GetTemplateCollectionAsync(CancellationToken.None));

            var jsonProcessor = new JsonProcessor();
            var inputContent = File.ReadAllText(inputFile);

            var imageResult = jsonProcessor.Convert(inputContent, rootTemplate, imageTemplateProvider);
            var folderResult = jsonProcessor.Convert(inputContent, rootTemplate, folderTemplateProvider);

            var imageResultObject = JObject.Parse(imageResult);
            var folderResultObject = JObject.Parse(folderResult);

            Assert.True(JToken.DeepEquals(imageResultObject, folderResultObject));
        }

        private void TestByTemplate(string inputFile, string entryTemplate, List<Dictionary<string, Template>> templateProvider)
        {
            var hl7v2Processor = new Hl7v2Processor();
            var inputContent = File.ReadAllText(inputFile);
            var actualContent = hl7v2Processor.Convert(inputContent, entryTemplate, new TemplateProvider(templateProvider));

            Assert.True(actualContent.Length != 0);
        }

        private async Task InitOneLayerImageAsync()
        {
            List<string> templateFiles = new List<string> { baseLayerTemplatePath };
            await _containerRegistry.GenerateTemplateImageAsync(_containerRegistryInfo, testOneLayerImageReference, templateFiles);
        }

        private async Task InitMultiLayerImageAsync()
        {
            List<string> templateFiles = new List<string> { baseLayerTemplatePath, userLayerTemplatePath };
            await _containerRegistry.GenerateTemplateImageAsync(_containerRegistryInfo, testMultiLayerImageReference, templateFiles);
        }

        private async Task InitInvalidTarGzImageAsync()
        {
            List<string> templateFiles = new List<string> { invalidTarGzPath };
            await _containerRegistry.GenerateTemplateImageAsync(_containerRegistryInfo, testInvalidImageReference, templateFiles);
        }

        private async Task InitInvalidTemplateImageAsync()
        {
            List<string> templateFiles = new List<string> { invalidTemplatePath };
            await _containerRegistry.GenerateTemplateImageAsync(_containerRegistryInfo, testInvalidTemplateImageReference, templateFiles);
        }
    }
}