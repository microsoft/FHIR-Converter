// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.FilterTests
{
    public class ErrorFiltersTests
    {
        [Fact]
        public void GivenAMessage_WhenRaiseError_RenderExceptionThrown()
        {
            var exception = Assert.Throws<RenderException>(() => Filters.RaiseError("test error message"));
            Assert.Equal(FhirConverterErrorCode.TemplateRenderingError, exception.FhirConverterErrorCode);
            Assert.Equal("test error message", exception.Message);
        }

        [Fact]
        public void GivenNullMessage_WhenRaiseError_RenderExceptionThrown()
        {
            var exception = Assert.Throws<RenderException>(() => Filters.RaiseError(null));
            Assert.Equal(FhirConverterErrorCode.TemplateRenderingError, exception.FhirConverterErrorCode);
        }

        [Fact]
        public void GivenEmptyMessage_WhenRaiseError_RenderExceptionThrown()
        {
            var exception = Assert.Throws<RenderException>(() => Filters.RaiseError(string.Empty));
            Assert.Equal(FhirConverterErrorCode.TemplateRenderingError, exception.FhirConverterErrorCode);
        }
    }
}
