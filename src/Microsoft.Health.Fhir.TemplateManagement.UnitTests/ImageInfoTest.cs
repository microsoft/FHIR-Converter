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

        public static IEnumerable<object[]> GetInValidImageReferenceInfo()
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
        }

        public static IEnumerable<object[]> GetValidImageReference()
        {
            yield return new object[] { "testacr.azurecr.io/templateset:v1" };
            yield return new object[] { "testacr.azurecr.io/templateset@sha256:e6dcff9eaf7604aa7a855e52b2cda22c5cfc5cadaa035892557c4ff19630b612" };
            yield return new object[] { "testacr.azurecr.io/templateset" };
        }

        [Theory]
        [MemberData(nameof(GetValidImageReferenceInfo))]
        public void GivenAValidImageReference_WhenConctructImageInfo_ACorrectImageInfoShouldBeConstructed(string imageReference, string registry, string imageName, string tag, string digest, string label)
        {
            ImageInfo imageInfo = ImageInfo.CreateFromImageReference(imageReference);
            Assert.Equal(registry, imageInfo.Registry);
            Assert.Equal(imageName, imageInfo.ImageName);
            Assert.Equal(tag, imageInfo.Tag);
            Assert.Equal(digest, imageInfo.Digest);
            Assert.Equal(label, imageInfo.Label);
        }

        [Theory]
        [MemberData(nameof(GetInValidImageReferenceInfo))]
        public void GivenAnInValidImageReference_WhenConctructImageInfo_ExceptionShouldBeThrown(string imageReference)
        {
            Assert.Throws<ImageReferenceException>(() => ImageInfo.CreateFromImageReference(imageReference));
        }

        [Theory]
        [MemberData(nameof(GetInValidImageReferenceInfo))]
        public void GivenAnInValidImageReference_WhenCheckImageReference_IsInvalidResultWillBeReturned(string imageReference)
        {
            Assert.False(ImageInfo.IsValidImageReference(imageReference));
        }

        [Theory]
        [MemberData(nameof(GetValidImageReference))]
        public void GivenAValidImageReference_WhenCheckImageReference_IsValidResultWillBeReturned(string imageReference)
        {
            Assert.True(ImageInfo.IsValidImageReference(imageReference));
        }

        [Fact]
        public void GivenDefaultTemplateReference_WhenCheckIsDefaultImageReference_TrueResultWillBeReturned()
        {
            var imageReference = "MicrosoftHealth/FhirConverter:default";
            Assert.True(ImageInfo.IsDefaultTemplateImageReference(imageReference));
        }

        [Fact]
        public void GivenUnDefaultTemplateReference_WhenCheckIsDefaultImageReference_TrueResultWillBeReturned()
        {
            var imageReference = "test";
            Assert.False(ImageInfo.IsDefaultTemplateImageReference(imageReference));
        }
    }
}
