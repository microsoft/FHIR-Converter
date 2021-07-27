// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
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
        private static readonly string _defaultTemplateImageReference = "microsofthealth/fhirconverter:default";
        private static readonly string _defaultHl7v2TemplateImageReference = "microsofthealth/hl7v2templates:default";
        private static readonly string _defaultCcdaTemplateImageReference = "microsofthealth/ccdatemplates:default";

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

        public static IEnumerable<object[]> GetDefaultTemplateTarGzFile()
        {
            yield return new object[] { "NewDefaultTemplates.tar.gz", _defaultTemplateImageReference, Path.Join( "..", "..", "..", "..", "..", "data", "Templates", "Hl7v2") };
            yield return new object[] { "Hl7v2NewDefaultTemplates.tar.gz", _defaultHl7v2TemplateImageReference, Path.Join("..", "..", "..", "..", "..", "data", "Templates", "Hl7v2") };
            yield return new object[] { "CcdaNewDefaultTemplates.tar.gz", _defaultCcdaTemplateImageReference, Path.Join("..", "..", "..", "..", "..", "data", "Templates", "Ccda") };
        }

        public static IEnumerable<object[]> GetDefaultImageReference()
        {
            yield return new object[] { _defaultTemplateImageReference };
            yield return new object[] { _defaultHl7v2TemplateImageReference };
            yield return new object[] { _defaultCcdaTemplateImageReference };
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
        public void GivenAnInvalidToken_WhenGetTemplateProvider_AnExceptionWillBeThrown()
        {
            string imageReference = "testacr.azurecr.io/templates:test1";
            TemplateCollectionProviderFactory factory = new TemplateCollectionProviderFactory(_cache, Options.Create(_config));
            Assert.Throws<ContainerRegistryAuthenticationException>(() => factory.CreateProvider(imageReference, string.Empty));
            Assert.Throws<ContainerRegistryAuthenticationException>(() => factory.CreateTemplateCollectionProvider(imageReference, string.Empty));
        }

        [Fact]
        public void GivenAWrongDefaultTemplatePath_WhenInitDefaultTemplate_ExceptionWillBeThrown()
        {
            TemplateCollectionProviderFactory factory = new TemplateCollectionProviderFactory(_cache, Options.Create(_config));
            DefaultTemplateInfo templateInfo = new DefaultTemplateInfo(DataType.Hl7v2, _defaultTemplateImageReference, "WrongPath");
            Assert.Throws<DefaultTemplatesInitializeException>(() => factory.InitDefaultTemplates(templateInfo));
        }

        [Theory]
        [MemberData(nameof(GetDefaultImageReference))]
        public void GiveDefaultImageReference_WhenGetTemplateProviderWithEmptyToken_ADefaultTemplateProviderWillBeReturnedAsync(string imageReference)
        {
            TemplateCollectionProviderFactory factory = new TemplateCollectionProviderFactory(_cache, Options.Create(_config));
            Assert.NotNull(factory.CreateProvider(imageReference, string.Empty));
            Assert.NotNull(factory.CreateTemplateCollectionProvider(imageReference, string.Empty));
        }

        [Theory]
        [MemberData(nameof(GetDefaultTemplateTarGzFile))]

        public void GiveNewDefaultTemplateTarGzFile_WhenInitDefaultTemplate_DefaultTemplatesWillBeInit(string targzName, string imageReference, string templateFolder)
        {
            CreateTarGz(targzName, templateFolder);
            TemplateCollectionProviderFactory factory = new TemplateCollectionProviderFactory(_cache, Options.Create(_config));
            DefaultTemplateInfo templateInfo = new DefaultTemplateInfo(DataType.Hl7v2, imageReference, targzName);
            factory.InitDefaultTemplates(templateInfo);
            Assert.NotNull(factory.CreateProvider(imageReference, string.Empty));
        }

        private void CreateTarGz(string outputTarFilename, string sourceDirectory)
        {
            using FileStream fs = new FileStream(outputTarFilename, FileMode.Create, FileAccess.Write, FileShare.None);
            using Stream gzipStream = new GZipOutputStream(fs);
            using TarArchive tarArchive = TarArchive.CreateOutputTarArchive(gzipStream);
            AddDirectoryFilesToTar(tarArchive, sourceDirectory, true);
        }

        private void AddDirectoryFilesToTar(TarArchive tarArchive, string sourceDirectory, bool recurse)
        {
            if (recurse)
            {
                string[] directories = Directory.GetDirectories(sourceDirectory);
                foreach (string directory in directories)
                {
                    AddDirectoryFilesToTar(tarArchive, directory, recurse);
                }
            }

            string[] filenames = Directory.GetFiles(sourceDirectory);
            foreach (string filename in filenames)
            {
                TarEntry tarEntry = TarEntry.CreateEntryFromFile(filename);
                tarArchive.WriteEntry(tarEntry, true);
            }
        }
    }
}