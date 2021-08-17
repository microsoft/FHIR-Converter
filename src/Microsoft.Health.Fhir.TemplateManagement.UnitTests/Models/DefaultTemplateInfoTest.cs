// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Xunit;

namespace Microsoft.Health.Fhir.TemplateManagement.UnitTests
{
    public class DefaultTemplateInfoTest
    {
        public static IEnumerable<object[]> GetValidDefaultTemplateInfo()
        {
            yield return new object[] { DataType.Hl7v2, "microsofthealth/fhirconverter:default", "Hl7v2DefaultTemplates.tar.gz" };
            yield return new object[] { DataType.Hl7v2, "microsofthealth/hl7v2templates:default", "Hl7v2DefaultTemplates.tar.gz" };
            yield return new object[] { DataType.Ccda, "microsofthealth/ccdatemplates:default", "CcdaDefaultTemplates.tar.gz" };
            yield return new object[] { DataType.Json, "microsofthealth/jsontemplates:default", "JsonDefaultTemplates.tar.gz" };
        }

        public static IEnumerable<object[]> GetSupportedImageReference()
        {
            yield return new object[] { "microsofthealth/fhirconverter:default" };
            yield return new object[] { "microsofthealth/hl7v2templates:default" };
            yield return new object[] { "microsofthealth/ccdatemplates:default" };
            yield return new object[] { "microsofthealth/jsontemplates:default" };
        }

        public static IEnumerable<object[]> GetSupportedImageReferenceWithCaseInsensitive()
        {
            yield return new object[] { "Microsofthealth/fhirconverter:default" };
            yield return new object[] { "microsofthealth/Fhirconverter:default" };
            yield return new object[] { "microsofthealth/fhirconverter:Default" };
            yield return new object[] { "MICROSOFTHEALTH/hl7v2templates:default" };
            yield return new object[] { "microsofthealth/ccdatemplates:DEFAULT" };
            yield return new object[] { "microsofthealth/JSONtemplates:DEFAULT" };
        }

        public static IEnumerable<object[]> GetUnSupportedImageReference()
        {
            yield return new object[] { "microsofthealth/fhirconverter" };
            yield return new object[] { "microsofthealth/fhirconverter:tag" };
            yield return new object[] { "microsofthealth/fhirconverter_:default" };
            yield return new object[] { "microsofthealth/invalidtemplates:default" };
        }

        [Theory]
        [MemberData(nameof(GetValidDefaultTemplateInfo))]
        public void GivenAValidDefaultTemplateInfo_WhenConstructDefaultTemplateInfo_ACorrectDefaultTemplateInfoShouldBeConstructed(DataType dataType, string imageReference, string templatePath)
        {
            DefaultTemplateInfo defaultTemplateInfo = new DefaultTemplateInfo(dataType, imageReference, templatePath);
            Assert.Equal(dataType, defaultTemplateInfo.DataType);
            Assert.Equal(imageReference, defaultTemplateInfo.ImageReference);
            Assert.Equal(templatePath, defaultTemplateInfo.TemplatePath);
        }

        [Theory]
        [MemberData(nameof(GetValidDefaultTemplateInfo))]
        public void GivenADefaultTemplateInfo_WhenGetDefaultTemplateMap_TheContentShouldBeTheSame(DataType dataType, string imageReference, string templatePath)
        {
            var defaultTemplateInfo = DefaultTemplateInfo.DefaultTemplateMap.GetValueOrDefault(imageReference);
            Assert.Equal(dataType, defaultTemplateInfo.DataType);
            Assert.Equal(imageReference, defaultTemplateInfo.ImageReference);
            Assert.Equal(templatePath, defaultTemplateInfo.TemplatePath);
        }

        [Theory]
        [MemberData(nameof(GetSupportedImageReference))]
        [MemberData(nameof(GetSupportedImageReferenceWithCaseInsensitive))]
        public void GivenAnSupportedImageReference_WhenCheckIsContainsKey_IsFalseWillBeReturned(string imageReference)
        {
            var defaultTemplateMap = DefaultTemplateInfo.DefaultTemplateMap;
            Assert.True(defaultTemplateMap.ContainsKey(imageReference));
        }

        [Theory]
        [MemberData(nameof(GetUnSupportedImageReference))]
        public void GivenAnUnSupportedImageReference_WhenCheckIsContainsKey_IsFalseWillBeReturned(string imageReference)
        {
            var defaultTemplateMap = DefaultTemplateInfo.DefaultTemplateMap;
            Assert.False(defaultTemplateMap.ContainsKey(imageReference));
        }

        [Theory]
        [MemberData(nameof(GetUnSupportedImageReference))]
        public void GivenAnUnSupportedImageReference_WhenGetValue_NullWillBeReturned(string imageReference)
        {
            var defaultTemplateMap = DefaultTemplateInfo.DefaultTemplateMap;
            Assert.Null(defaultTemplateMap.GetValueOrDefault(imageReference));
        }
    }
}
