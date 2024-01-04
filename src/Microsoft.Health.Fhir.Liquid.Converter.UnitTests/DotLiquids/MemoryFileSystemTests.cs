// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.DotLiquids;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Utilities;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.DotLiquids
{
    public class MemoryFileSystemTests
    {
        [Fact]
        public void GivenAValidTemplateCollection_WhenGetTemplate_CorrectResultShouldBeReturned()
        {
            var templateCollection = new List<Dictionary<string, Template>>
            {
                new Dictionary<string, Template>
                {
                    { "template1", Template.Parse("hello world") },
                },
            };

            var memoryFileSystem = new MemoryFileSystem(templateCollection);
            Assert.Equal("hello world", memoryFileSystem.GetTemplate("template1").Render());
            Assert.Null(memoryFileSystem.GetTemplate("template2"));
            Assert.Null(memoryFileSystem.GetTemplate(null));
            Assert.Null(memoryFileSystem.GetTemplate(string.Empty));
        }

        [Fact]
        public void GivenTwoValidTemplateCollection_WhenGetTemplate_CorrectResultShouldBeReturned()
        {
            var templateCollection = new List<Dictionary<string, Template>>
            {
                new Dictionary<string, Template>
                {
                    // customer delete folder/template1
                    { "folder/template1", null },

                    // customer update template2
                    { "template2", Template.Parse("template2 updated in customized layer") },

                    // customer add template4
                    { "template4", Template.Parse("template4 added in customized layer") },
                },
                new Dictionary<string, Template>
                {
                    { "folder/template1", Template.Parse("folder/template1 added in base layer") },
                    { "template2", Template.Parse("template2 added in base layer") },
                    { "template3", Template.Parse("template3 added in base layer") },
                },
            };

            var memoryFileSystem = new MemoryFileSystem(templateCollection);
            Assert.Null(memoryFileSystem.GetTemplate("template1"));
            Assert.Equal("template2 updated in customized layer", memoryFileSystem.GetTemplate("template2").Render());
            Assert.Equal("template3 added in base layer", memoryFileSystem.GetTemplate("template3").Render());
            Assert.Equal("template4 added in customized layer", memoryFileSystem.GetTemplate("template4").Render());
        }

        [Fact]
        public void GivenAValidTemplateCollection_WhenGetTemplateWithContext_CorrectResultShouldBeReturned()
        {
            var templateCollection = new List<Dictionary<string, Template>>
            {
                new Dictionary<string, Template>
                {
                    { "template1", Template.Parse("hello world") },
                },
            };

            var memoryFileSystem = new MemoryFileSystem(templateCollection);
            var context = new Context(CultureInfo.InvariantCulture);
            context["template1"] = "template1";
            context["template2"] = "template2";
            Assert.Equal("hello world", memoryFileSystem.GetTemplate(context, "template1").Render());
            Assert.Throws<RenderException>(() => memoryFileSystem.GetTemplate(context, "template2"));
            Assert.Throws<RenderException>(() => memoryFileSystem.GetTemplate(context, "template3"));
            Assert.Throws<RenderException>(() => memoryFileSystem.GetTemplate(context, null));
            Assert.Throws<RenderException>(() => memoryFileSystem.GetTemplate(context, string.Empty));
        }

        [Fact]
        public void GivenTwoValidtTemplateCollection_WhenGetTemplateWithContext_CorrectResultShouldBeReturned()
        {
            var templateCollection = new List<Dictionary<string, Template>>
            {
                new Dictionary<string, Template>
                {
                    // customer delete folder/template1
                    { "folder/template1", null },

                    // customer update template2
                    { "template2", Template.Parse("template2 updated in customized layer") },

                    // customer add template4
                    { "template4", Template.Parse("template4 added in customized layer") },
                },
                new Dictionary<string, Template>
                {
                    { "folder/template1", Template.Parse("folder/template1 added in base layer") },
                    { "template2", Template.Parse("template2 added in base layer") },
                    { "template3", Template.Parse("template3 added in base layer") },
                },
            };

            var memoryFileSystem = new MemoryFileSystem(templateCollection);
            var context = new Context(CultureInfo.InvariantCulture);
            context["'folder/template1'"] = "folder/template1";
            context["template2"] = "template2";
            context["template3"] = "template3";
            context["template4"] = "template4";

            Assert.Throws<RenderException>(() => memoryFileSystem.GetTemplate(context, "'folder/template1'"));
            Assert.Equal("template2 updated in customized layer", memoryFileSystem.GetTemplate(context, "template2").Render());
            Assert.Equal("template3 added in base layer", memoryFileSystem.GetTemplate(context, "template3").Render());
            Assert.Equal("template4 added in customized layer", memoryFileSystem.GetTemplate(context, "template4").Render());
        }

        [Fact]
        public void GivenAValidTemplateCollection_WhenReadTemplateWithContext_ExceptionShouldBeThrown()
        {
            var templateCollection = new List<Dictionary<string, Template>>
            {
                new Dictionary<string, Template>
                {
                    { "hello", Template.Parse("world") },
                },
            };

            var memoryFileSystem = new MemoryFileSystem(templateCollection);
            var context = new Context(CultureInfo.InvariantCulture);
            context["hello"] = "hello";
            Assert.Throws<NotImplementedException>(() => memoryFileSystem.ReadTemplateFile(context, "hello"));
        }

        [Theory]
        [InlineData(null, null, null)]
        [InlineData("", null, null)]
        [InlineData("invalidtemplate", null, null)]
        [InlineData("template0", null, "template0")]
        [InlineData("template0", "TC0", null)]
        [InlineData("template1", null, null)]
        [InlineData("TC1/template1", null, "TC1/template1")]
        [InlineData("template1", "TC1", "TC1/template1")]
        [InlineData("template1/subtemplate1", null, null)]
        [InlineData("TC1/template1/subtemplate1", null, "TC1/template1/subtemplate1")]
        [InlineData("template1/subtemplate1", "TC1", "TC1/template1/subtemplate1")]
        [InlineData("template2/subtemplate1", "TC1", "TC1/template2/subtemplate1")]
        [InlineData("template1", "TC2", "TC2/template1")]
        [InlineData("template2/subtemplate1", "TC2", "TC2/template2/subtemplate1")]
        public void GivenValidTemplateCollection_WhenGetTemplateWithRootTemplatePath_CorrectResultShouldBeReturned(string templateName, string rootTemplateParentPath, string expectedTemplate)
        {
            var templateCollection = GetMockTemplateCollection();
            var memoryFileSystem = new MemoryFileSystem(templateCollection);

            var template = memoryFileSystem.GetTemplate(templateName, rootTemplateParentPath);
            Assert.Equal(expectedTemplate, template?.Render());
        }

        [Theory]
        [InlineData(null, null, null)]
        [InlineData("", null, null)]
        [InlineData("invalidtemplate", null, null)]
        [InlineData("template0", null, "template0")]
        [InlineData("template0", "TC0", null)]
        [InlineData("template1", null, null)]
        [InlineData("'TC1/template1'", null, "TC1/template1")]
        [InlineData("template1", "TC1", "TC1/template1")]
        [InlineData("template1/subtemplate1", null, null)]
        [InlineData("'TC1/template1/subtemplate1'", null, "TC1/template1/subtemplate1")]
        [InlineData("'template1/subtemplate1'", "TC1", "TC1/template1/subtemplate1")]
        [InlineData("'template2/subtemplate1'", "TC1", "TC1/template2/subtemplate1")]
        [InlineData("template1", "TC2", "TC2/template1")]
        [InlineData("'template2/subtemplate1'", "TC2", "TC2/template2/subtemplate1")]
        public void GivenValidTemplateCollection_WhenGetTemplateWithRootTemplatePathAndContext_CorrectResultShouldBeReturned(string templateName, string rootTemplateParentPath, string expectedTemplate)
        {
            var templateCollection = GetMockTemplateCollection();
            var memoryFileSystem = new MemoryFileSystem(templateCollection);

            var context = new Context(CultureInfo.InvariantCulture);
            context[TemplateUtility.RootTemplateParentPathScope] = rootTemplateParentPath;

            if (!string.IsNullOrWhiteSpace(templateName))
            {
                context[templateName] = templateName;
            }

            if (string.IsNullOrEmpty(expectedTemplate))
            {
                var exception = Assert.Throws<RenderException>(() => memoryFileSystem.GetTemplate(context, templateName));
                Assert.Equal(FhirConverterErrorCode.TemplateNotFound, exception.FhirConverterErrorCode);
            }
            else
            {
                var template = memoryFileSystem.GetTemplate(context, templateName);
                Assert.Equal(expectedTemplate, template.Render());
            }
        }

        private List<Dictionary<string, Template>> GetMockTemplateCollection()
        {
            return new List<Dictionary<string, Template>>
            {
                new ()
                {
                    { "template0", Template.Parse("template0") },
                    { "TC1/template1", Template.Parse("TC1/template1") },
                    { "TC1/template1/subtemplate1", Template.Parse("TC1/template1/subtemplate1") },
                    { "TC1/template1/subtemplate2", Template.Parse("TC1/template1/subtemplate2") },
                    { "TC1/template2/subtemplate1", Template.Parse("TC1/template2/subtemplate1") },
                    { "TC2/template1", Template.Parse("TC2/template1") },
                    { "TC2/template1/subtemplate1", Template.Parse("TC2/template1/subtemplate1") },
                    { "TC2/template1/subtemplate2", Template.Parse("TC2/template1/subtemplate2") },
                    { "TC2/template2/subtemplate1", Template.Parse("TC2/template2/subtemplate1") },
                },
            };
        }
    }
}
