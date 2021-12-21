// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Fhir.Liquid.Converter.Utilities;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.Utilities
{
    public class Hl7v2DataUtilityTests
    {
        [Fact]
        public void GivenValidMessage_WhenSplitMessageToSegments_CorrectResultShouldBeReturned()
        {
            string[] testMessage =
            {
                "\r\nMSH|^~\\&|test\r\nPID|test\r\n\r\nPD1|test\r\nPD1|test\r\n",
                "\rMSH|^~\\&|test\rPID|test\r\rPD1|test\rPD1|test\r",
                "\nMSH|^~\\&|test\nPID|test\n\nPD1|test\nPD1|test\n",
                "\nMSH|^~\\&|test\r\nPID|test\r\r\nPD1|test\r\n\nPD1|test\r\n",
            };

            foreach (string message in testMessage)
            {
                string[] segments = Hl7v2DataUtility.SplitMessageToSegments(message);
                Assert.Equal(4, segments.Length);
                Assert.Equal("MSH|^~\\&|test", segments[0]);
                Assert.Equal("PID|test", segments[1]);
                Assert.Equal("PD1|test", segments[2]);
                Assert.Equal("PD1|test", segments[3]);
            }

            Assert.Empty(Hl7v2DataUtility.SplitMessageToSegments(string.Empty));

            // message could not be null
            Assert.Throws<NullReferenceException>(() => Hl7v2DataUtility.SplitMessageToSegments(null));
        }
    }
}
