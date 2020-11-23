// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using Microsoft.Azure.ContainerRegistry.Models;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Utilities;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Health.Fhir.TemplateManagement.UnitTests.Utilities
{
    public class ValidationUtilityTests
    {
        public static IEnumerable<object[]> GetConsistentBlobContentWithDigest()
        {
            yield return new object[] { "TestData/TarGzFiles/userV1.tar.gz", "sha256:ca1d8cff554ca9d7796500548c968f4985528af2676589de3a719048f266a3bc" };
            yield return new object[] { "TestData/TarGzFiles/userV2.tar.gz", "sha256:fac83d4cd496b0efd76db216568a53e1ff4f9571461deec8835cc4d2e171d063" };
        }

        public static IEnumerable<object[]> GetInConsistentBlobContentWithDigest()
        {
            yield return new object[] { "TestData/TarGzFiles/invalid1.tar.gz", "sha256:390c92b55d8ef9ee51621be7772defc56cedaa75ae0e05cf6612d26e44274cfa" };
            yield return new object[] { "TestData/TarGzFiles/invalid2.tar.gz", "sha256:63b3cd0c53f84c54233ee2643bd553cee7dcff30920024794e7d3d0c8fe14726" };
        }

        [Theory]
        [MemberData(nameof(GetInConsistentBlobContentWithDigest))]
        public void GivenInConsistentBlobContentWithDigest_WhenValidate_ExceptionWillBeThrown(string blobPath, string digest)
        {
            Assert.Throws<ImageValidationException>(() => ValidationUtility.ValidateOneBlob(File.ReadAllBytes(blobPath), digest));
        }

        [Theory]
        [MemberData(nameof(GetConsistentBlobContentWithDigest))]
        public void GivenConsistentBlobContentWithDigest_WhenValidate_NoExceptionWillBeThrown(string blobPath, string digest)
        {
            var ex = Record.Exception(() => ValidationUtility.ValidateOneBlob(File.ReadAllBytes(blobPath), digest));
            Assert.Null(ex);
        }

        [Fact]
        public void GivenAManifestWithDigestEmpty_WhenValidate_ExceptionWillBeThrown()
        {
            string invalidManifestName = "TestData/InvalidManifest/emptyDigestManifest";
            Assert.Throws<ImageValidationException>(() => ValidationUtility.ValidateManifest(JsonConvert.DeserializeObject<ManifestWrapper>(File.ReadAllText(invalidManifestName))));
        }

        [Fact]
        public void GivenANullManifestObject_WhenValidate_ExceptionWillBeThrown()
        {
            Assert.Throws<ImageValidationException>(() => ValidationUtility.ValidateManifest(null));
        }

        [Fact]
        public void GivenAValidManifestObject_WhenValidate_NoExceptionShouldBeThrown()
        {
            string validManifestName = "TestData/ExpectedManifest/manifestv1";
            var ex = Record.Exception(() => ValidationUtility.ValidateManifest(JsonConvert.DeserializeObject<ManifestWrapper>(File.ReadAllText(validManifestName))));
            Assert.Null(ex);
        }
    }
}
