// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Xunit;

namespace Microsoft.Health.Fhir.TemplateManagement.UnitTests
{
    public class ImageInfoTest
    {
        public static IEnumerable<object[]> GetValidImageReferenceInfo()
        {
            yield return new object[] { "testacr.azurecr.io/templateset:v1", "testacr.azurecr.io", "templateset", "v1", null, "v1" };
            yield return new object[] { "testacr.azurecr.io/templateset@sha256:e6dcff9eaf7604aa7a855e52b2cda22c5cfc5cadaa035892557c4ff19630b612", "testacr.azurecr.io", "templateset", "latest", "sha256:e6dcff9eaf7604aa7a855e52b2cda22c5cfc5cadaa035892557c4ff19630b612", "sha256:e6dcff9eaf7604aa7a855e52b2cda22c5cfc5cadaa035892557c4ff19630b612" };
            yield return new object[] { "testacr.azurecr.io/templateset", "testacr.azurecr.io", "templateset", "latest", null, "latest" };
        }

        public static IEnumerable<object[]> GetInValidImageReference()
        {
            yield return new object[] { "testacr.azurecr.io@v1" };
            yield return new object[] { "testacr.azurecr.io:templateset:v1" };
            yield return new object[] { "testacr.azurecr.io_v1" };
            yield return new object[] { "testacr.azurecr.io:v1" };
            yield return new object[] { "testacr.azurecr.io/" };
            yield return new object[] { "/testacr.azurecr.io" };
            yield return new object[] { "testacr.azurecr.io/name:" };
            yield return new object[] { "testacr.azurecr.io/:tag" };
            yield return new object[] { "testacr.azurecr.io/name@" };
            yield return new object[] { "testacr.azurecr.io/INVALID" };
            yield return new object[] { "testacr.azurecr.io/invalid_" };
            yield return new object[] { "testacr.azurecr.io/in*valid" };
            yield return new object[] { "testacr.azurecr.io/org/org/in*valid" };
            yield return new object[] { "testacr.azurecr.io/invalid____set" };
            yield return new object[] { "testacr.azurecr.io/invalid....set" };
            yield return new object[] { "testacr.azurecr.io/invalid._set" };
            yield return new object[] { "testacr.azurecr.io/_invalid" };
            yield return new object[] { "testacr.azurecr.io/Templateset:v1" };
        }

        public static IEnumerable<object[]> GetInValidImageReferenceWithCaseSensitive()
        {
            yield return new object[] { "testacr.azurecr.io/Templateset:v1" };
            yield return new object[] { "testacr.azurecr.io/TEMPLATESET:v1" };
        }

        public static IEnumerable<object[]> GetValidImageReference()
        {
            yield return new object[] { "testacr.azurecr.io/templateset:v1" };
            yield return new object[] { "testacr.azurecr.io/templateset@sha256:e6dcff9eaf7604aa7a855e52b2cda22c5cfc5cadaa035892557c4ff19630b612" };
            yield return new object[] { "testacr.azurecr.io/templateset" };
            yield return new object[] { "testacr.azurecr.io/org/templateset" };
            yield return new object[] { "testacr.azurecr.io/org/template-set" };
            yield return new object[] { "testacr.azurecr.io/org/template.set" };
            yield return new object[] { "testacr.azurecr.io/org/template__set" };
            yield return new object[] { "testacr.azurecr.io/org/template-----set" };
            yield return new object[] { "testacr.azurecr.io/org/template-set_set.set" };
            yield return new object[] { "Testacr.azurecr.io/templateset:v1" };
            yield return new object[] { "testacr.azurecr.io/templateset:V1" };
        }

        public static IEnumerable<object[]> GetValidImageReferenceWithCaseSensitive()
        {
            yield return new object[] { "Testacr.azurecr.io/templateset:v1" };
            yield return new object[] { "TESTACR.azurecr.io/templateset:v1" };
            yield return new object[] { "testacr.Azurecr.io/templateset:v1" };
            yield return new object[] { "testacr.azurecr.IO/templateset:v1" };
            yield return new object[] { "testacr.azurecr.io/templateset:V1" };
        }

        public static IEnumerable<object[]> GetUnDefaultTemplateReference()
        {
            yield return new object[] { "testacr.azurecr.io/templateset:v1" };
            yield return new object[] { "testacr.azurecr.io/templateset@sha256:e6dcff9eaf7604aa7a855e52b2cda22c5cfc5cadaa035892557c4ff19630b612" };
            yield return new object[] { "testacr.azurecr.io/templateset" };
        }

        public static IEnumerable<object[]> GetDefaultTemplateReference()
        {
            yield return new object[] { "microsofthealth/fhirconverter:default" };
            yield return new object[] { "microsofthealth/hl7v2templates:default" };
            yield return new object[] { "microsofthealth/ccdatemplates:default" };
            yield return new object[] { "microsofthealth/jsontemplates:default" };
            yield return new object[] { "microsofthealth/stu3tor4templates:default" };
        }

        public static IEnumerable<object[]> GetDefaultTemplateReferenceWithCaseInsensitive()
        {
            yield return new object[] { "Microsofthealth/fhirconverter:default" };
            yield return new object[] { "microsofthealth/Fhirconverter:default" };
            yield return new object[] { "microsofthealth/fhirconverter:Default" };
            yield return new object[] { "MICROSOFTHEALTH/FHIRCONVERTER:DEFAULT" };
            yield return new object[] { "MicrosoftHealth/Hl7v2Templates:default" };
            yield return new object[] { "microsoftHealth/CcdaTemplates:default" };
            yield return new object[] { "microsoftHealth/JSONTemplates:default" };
            yield return new object[] { "microsoftHealth/STu3tor4Templates:default" };
        }

        public static IEnumerable<object[]> GetInvalidDefaultTemplateReference()
        {
            yield return new object[] { "MicrosoftHealth/FhirConverter:tag" };
            yield return new object[] { "MicrosoftHealth" };
            yield return new object[] { "MicrosoftHealth/FhirConverter" };
            yield return new object[] { "FhirConverter:tag" };
            yield return new object[] { "MicrosoftHealth//FhirConverter:default" };
            yield return new object[] { "icrosoftHealth/FhirConverter:default" };
            yield return new object[] { "MicrosoftHealth/FhirConverter@default" };
            yield return new object[] { "MicrosoftHealth/Hl7v2Templates" };
            yield return new object[] { "MicrosoftHealth/Hl7v2Templates@default" };
            yield return new object[] { "MicrosoftHealth/templates:default" };
            yield return new object[] { "test" };
        }

        [Theory]
        [MemberData(nameof(GetValidImageReferenceInfo))]
        public void GivenAValidImageReference_WhenConstructImageInfo_ACorrectImageInfoShouldBeConstructed(string imageReference, string registry, string imageName, string tag, string digest, string label)
        {
            ImageInfo imageInfo = ImageInfo.CreateFromImageReference(imageReference);
            Assert.Equal(registry, imageInfo.Registry);
            Assert.Equal(imageName, imageInfo.ImageName);
            Assert.Equal(tag, imageInfo.Tag);
            Assert.Equal(digest, imageInfo.Digest);
            Assert.Equal(label, imageInfo.Label);
        }

        [Theory]
        [MemberData(nameof(GetInValidImageReference))]
        [MemberData(nameof(GetInValidImageReferenceWithCaseSensitive))]
        public void GivenAnInValidImageReference_WhenConstructImageInfo_ExceptionShouldBeThrown(string imageReference)
        {
            Assert.Throws<ImageReferenceException>(() => ImageInfo.CreateFromImageReference(imageReference));
        }

        [Theory]
        [MemberData(nameof(GetInValidImageReference))]
        [MemberData(nameof(GetInValidImageReferenceWithCaseSensitive))]
        public void GivenAnInValidImageReference_WhenCheckImageReference_IsInvalidResultWillBeReturned(string imageReference)
        {
            Assert.False(ImageInfo.IsValidImageReference(imageReference));
        }

        [Theory]
        [MemberData(nameof(GetValidImageReference))]
        [MemberData(nameof(GetValidImageReferenceWithCaseSensitive))]
        public void GivenAValidImageReference_WhenCheckImageReference_IsValidResultWillBeReturned(string imageReference)
        {
            Assert.True(ImageInfo.IsValidImageReference(imageReference));
        }

        [Theory]
        [MemberData(nameof(GetDefaultTemplateReference))]
        [MemberData(nameof(GetDefaultTemplateReferenceWithCaseInsensitive))]
        public void GivenADefaultTemplateReferenceAsImageReference_WhenCheckIsDefaultTemplate_IsTrueResultWillBeReturned(string imageReference)
        {
            Assert.True(ImageInfo.IsDefaultTemplateImageReference(imageReference));
        }

        [Theory]
        [MemberData(nameof(GetDefaultTemplateReference))]
        [MemberData(nameof(GetDefaultTemplateReferenceWithCaseInsensitive))]
        public void GivenADefaultTemplateReferenceAsImageReference_WhenCreateImageInfoAndCheckIsDefaultTemplate_IsTrueResultWillBeReturned(string imageReference)
        {
            var imageInfo = ImageInfo.CreateFromImageReference(imageReference);
            Assert.True(imageInfo.IsDefaultTemplate());
        }

        [Theory]
        [MemberData(nameof(GetUnDefaultTemplateReference))]
        public void GivenAUnDefaultTemplateReferenceAsImageReference_WhenCheckIsDefaultTemplate_IsFalseResultWillBeReturned(string imageReference)
        {
            var imageInfo = ImageInfo.CreateFromImageReference(imageReference);
            Assert.False(imageInfo.IsDefaultTemplate());
        }

        [Theory]
        [MemberData(nameof(GetInvalidDefaultTemplateReference))]
        public void GivenInvalidDefaultTemplateReference_WhenCheckIsDefaultImageReference_IsFalseResultWillBeReturned(string imageReference)
        {
            Assert.False(ImageInfo.IsDefaultTemplateImageReference(imageReference));
        }
    }
}
