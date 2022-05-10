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
        public string Value;
        public object[] Expected;

        public ComplexObjectFilterUtilityTestCase(object[] input, string path, string value, object[] expected)
        {
            Input = input;
            Path = path;
            Value = value;
            Expected = expected;
        }
    }
}