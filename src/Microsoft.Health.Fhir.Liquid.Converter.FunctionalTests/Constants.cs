// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.IO;

namespace Microsoft.Health.Fhir.Liquid.Converter.FunctionalTests
{
    public static class Constants
    {
        public static readonly string TemplateDirectory = Path.Join("..", "..", "data", "Templates");
        public static readonly string SampleDataDirectory = Path.Join("..", "..", "data", "SampleData");
        public static readonly string ExpectedDataFolder = Path.Join("TestData", "Expected");
    }
}
