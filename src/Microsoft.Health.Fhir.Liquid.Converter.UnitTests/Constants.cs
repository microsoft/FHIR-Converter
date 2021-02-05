﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.IO;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests
{
    public static class Constants
    {
        public static readonly string SampleDataDirectory = Path.Join("..", "..", "data", "SampleData");
        public static readonly string TemplateDirectory = Path.Join("..", "..", "data", "Templates");
        public static readonly string Hl7v2TemplateDirectory = Path.Join(TemplateDirectory, "Hl7v2");
        public static readonly string CdaTemplateDirectory = Path.Join(TemplateDirectory, "Cda");
    }
}
