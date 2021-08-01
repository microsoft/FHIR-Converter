// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Fhir.Liquid.Converter.DotLiquids;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.DotLiquids
{
    public class SafeListTests
    {
        [Fact]
        public void GivenASafeList_WhenAccessed_CorrectResultShouldBeReturned()
        {
            // SafeList can return default when index is out of range
            var safeList = new SafeList<string>();
            Assert.Null(safeList[0]);
            Assert.False(safeList.ContainsKey(0));
            Assert.Equal(0, safeList.Count);

            // SafeList can add value with Add
            safeList.Add("a");
            Assert.Equal("a", safeList[0]);
            Assert.True(safeList.ContainsKey(0));
            Assert.Equal(1, safeList.Count);

            // SafeList can add value with index
            safeList[1] = "b";
            Assert.Equal("b", safeList[1]);
            Assert.True(safeList.ContainsKey(1));
            Assert.Equal(2, safeList.Count);

            // SafeList can return default when key is not an integer
            safeList["abc"] = "c";
            Assert.Null(safeList["abc"]);
            Assert.False(safeList.ContainsKey("abc"));
            Assert.Equal(2, safeList.Count);
        }
    }
}
