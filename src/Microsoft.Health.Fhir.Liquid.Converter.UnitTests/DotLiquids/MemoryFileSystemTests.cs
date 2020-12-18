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
    }
}
