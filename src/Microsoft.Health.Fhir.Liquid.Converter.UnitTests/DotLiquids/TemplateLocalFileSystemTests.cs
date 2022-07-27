// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Globalization;
using System.IO;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.DotLiquids;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Models.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.DotLiquids
{
    public class TemplateLocalFileSystemTests
    {
        [Fact]
        public void GivenAValidTemplateDirectory_WhenGetTemplate_CorrectResultsShouldBeReturned()
        {
            var templateLocalFileSystem = new TemplateLocalFileSystem(TestConstants.Hl7v2TemplateDirectory, DataType.Hl7v2);

            // Template exists
            Assert.NotNull(templateLocalFileSystem.GetTemplate("ADT_A01"));

            // Template does not exist
            Assert.Null(templateLocalFileSystem.GetTemplate("Foo"));
        }

        [Fact]
        public void GivenAValidTemplateDirectory_WhenGetTemplateWithContext_CorrectResultsShouldBeReturned()
        {
            var templateLocalFileSystem = new TemplateLocalFileSystem(TestConstants.Hl7v2TemplateDirectory, DataType.Hl7v2);
            var context = new Context(CultureInfo.InvariantCulture);

            // Template exists
            context["ADT_A01"] = "ADT_A01";
            Assert.NotNull(templateLocalFileSystem.GetTemplate(context, "ADT_A01"));

            // Template does not exist
            context["Foo"] = "Foo";
            Assert.Throws<RenderException>(() => templateLocalFileSystem.GetTemplate(context, "Foo"));
            Assert.Throws<RenderException>(() => templateLocalFileSystem.GetTemplate(context, "Bar"));
        }

        [Fact]
        public async void GivenAValidTemplateDirectory_WhenGetJsonSchemaTemplate_CorrectResultsShouldBeReturned()
        {
            var templateLocalFileSystem = new TemplateLocalFileSystem(Path.Join(TestConstants.TestTemplateDirectory, @"ValidValidateTemplates"), DataType.Hl7v2);
            var testSchemaPath = "Schemas/TestSchema.schema.json";

            var schemaTemplate = templateLocalFileSystem.GetTemplate(testSchemaPath);

            // Template exists
            Assert.NotNull(schemaTemplate);

            var jSchemaDocument = schemaTemplate.Root as JSchemaDocument;
            Assert.NotNull(jSchemaDocument);
            Assert.NotNull(jSchemaDocument.Schema);

            JsonSchema expectedJSchema = await JsonSchema.FromJsonAsync(File.ReadAllText(Path.Join(TestConstants.TestTemplateDirectory, @"ValidValidateTemplates", testSchemaPath)));
            Assert.True(JToken.DeepEquals(JToken.Parse(jSchemaDocument.Schema.ToJson()), JToken.Parse(expectedJSchema.ToJson())));
        }

        [Fact]
        public void GivenAValidTemplateDirectory_WhenReadTemplateWithContext_ExceptionShouldBeThrown()
        {
            var templateLocalFileSystem = new TemplateLocalFileSystem(TestConstants.Hl7v2TemplateDirectory, DataType.Hl7v2);
            var context = new Context(CultureInfo.InvariantCulture);
            context["ADT_A01"] = "ADT_A01";
            Assert.Throws<NotImplementedException>(() => templateLocalFileSystem.ReadTemplateFile(context, "hello"));
        }

        [Fact]
        public void GivenAValidTemplateDirectory_WhenGetInvalidJsonSchemaTemplate_CorrectResultsShouldBeReturned()
        {
            var templateLocalFileSystem = new TemplateLocalFileSystem(Path.Join(TestConstants.TestTemplateDirectory, @"InvalidValidateTemplates"), DataType.Hl7v2);
            var testSchemaPath = "Schemas/InvalidTestSchema.schema.json";
            var context = new Context(CultureInfo.InvariantCulture);
            Assert.Throws<TemplateLoadException>(() => templateLocalFileSystem.GetTemplate(testSchemaPath));
        }
    }
}
