// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Fhir.Liquid.Converter.Models.Hl7v2;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.FilterTests
{
    public class MathFiltersTests
    {
        [Fact]
        public void IsNaNTest()
        {
            Assert.True(Filters.IsNaN("2019.6"));
            Assert.False(Filters.IsNaN(true));
            Assert.False(Filters.IsNaN(string.Empty));
            Assert.False(Filters.IsNaN(new Hl7v2Data()));
        }

        [Fact]
        public void AbsTest()
        {
            Assert.Equal(2019.6, Filters.Abs(2019.6));
            Assert.Equal(2019.6, Filters.Abs(-2019.6));
            Assert.Equal(0, Filters.Abs(0));
        }

        [Fact]
        public void PowTest()
        {
            Assert.Equal(27, Filters.Pow(3, 3));
        }

        [Fact]
        public void RandomTest()
        {
            var result = Filters.Random(100);
            Assert.True(result < 100 && result >= 0);
        }

        [Fact]
        public void SignTest()
        {
            Assert.Equal(-1, Filters.Sign(-5));
            Assert.Equal(1, Filters.Sign(5));
            Assert.Equal(0, Filters.Sign(0));
        }

        [Fact]
        public void TruncateNumberTest()
        {
            Assert.Equal(-34, Filters.TruncateNumber(-34.53));
        }

        [Fact]
        public void DivideTest()
        {
            Assert.Equal(2.5, Filters.Divide(5, 2));
            Assert.True(double.IsInfinity(Filters.Divide(5, 0)));
        }
    }
}