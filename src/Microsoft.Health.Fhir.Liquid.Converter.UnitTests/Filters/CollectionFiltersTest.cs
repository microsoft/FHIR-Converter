// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.FilterTests
{
    public class CollectionFiltersTest
    {
        [Fact]
        public void ToArrayTests()
        {
            Assert.Empty(Filters.ToArray(null));
            Assert.Single(Filters.ToArray(1));
            Assert.Equal(2, Filters.ToArray(new List<string> { null, string.Empty }).Count);
        }

        [Fact]
        public void ConcatTests()
        {
            Assert.Empty(Filters.Concat(null, null));
            Assert.Single(Filters.Concat(new List<object> { string.Empty, null }, new List<object>()));
        }
    }
}
