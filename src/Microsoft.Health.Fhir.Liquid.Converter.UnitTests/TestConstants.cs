// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.IO;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests
{
    public static class TestConstants
    {
        public static readonly string SampleDataDirectory = Path.Join("..", "..", "data", "SampleData");
        public static readonly string TemplateDirectory = Path.Join("..", "..", "data", "Templates");
        public static readonly string Hl7v2TemplateDirectory = Path.Join(TemplateDirectory, "Hl7v2");
        public static readonly string CcdaTemplateDirectory = Path.Join(TemplateDirectory, "Ccda");
        public static readonly string JsonTemplateDirectory = Path.Join(TemplateDirectory, "Json");
        public static readonly string FhirStu3TemplateDirectory = Path.Join(TemplateDirectory, "Stu3ToR4");
        public static readonly string TestTemplateDirectory = "TestData/TestTemplates";
        public static readonly string ExpectedDirectory = "TestData/Expected/";
    }
}
