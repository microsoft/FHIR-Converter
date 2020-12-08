// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.IO;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Hl7v2;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.Hl7v2
{
    public class Hl7v2TemplateProviderTests
    {
        [Fact]
        public void GivenATemplateDirectory_WhenLoadTemplates_CorrectResultsShouldBeReturned()
        {
            var templateProvider = new Hl7v2TemplateProvider(Constants.Hl7v2TemplateDirectory);
            Assert.True(templateProvider.GetTemplate("ADT_A01").Root.NodeList.Count > 0);

            Assert.Throws<ConverterInitializeException>(() => templateProvider.LoadTemplates(null));
            Assert.Throws<ConverterInitializeException>(() => templateProvider.LoadTemplates(string.Empty));
            Assert.Throws<ConverterInitializeException>(() => templateProvider.LoadTemplates(Path.Join("a", "b", "c")));

            var exception = Assert.Throws<ConverterInitializeException>(() => templateProvider.LoadTemplates(@"TestTemplates"));
            Assert.Equal(FhirConverterErrorCode.TemplateLoadingError, exception.FhirConverterErrorCode);
            var innerException = exception.InnerException as FhirConverterException;
            Assert.Equal(FhirConverterErrorCode.TemplateSyntaxError, innerException.FhirConverterErrorCode);
        }
    }
}
