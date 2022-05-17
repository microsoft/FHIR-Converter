// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.Models
{
    public struct ComplexObjectFilterUtilityTestCase
    {
        public object[] Input;
        public string Path;
        public string[] Values;
        public object[] Expected;

        public ComplexObjectFilterUtilityTestCase(object[] input, string path, string[] values, object[] expected)
        {
            Input = input;
            Path = path;
            Values = values;
            Expected = expected;
        }
    }
}