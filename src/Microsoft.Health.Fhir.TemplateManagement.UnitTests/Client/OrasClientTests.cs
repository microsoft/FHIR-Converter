using Microsoft.Health.Fhir.TemplateManagement.Client;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Microsoft.Health.Fhir.TemplateManagement.UnitTests.Client
{
    public class OrasClientTests
    {
        public static IEnumerable<object[]> GetValidImageReference()
        {
            yield return new object[] { "OCIsowuacr.azurecr.io/templateoras:v1" };
            yield return new object[] { "OCIsowuacr.azurecr.io/templateoras:default" };
        }

        [Theory]
        [MemberData(nameof(GetValidImageReference))]
        public void GivenAValidImageReference_WhenPushImageUseOras_ImageWillBePushed(string imageReference)
        {
            Directory.CreateDirectory("TestData/.image/layers");
            File.Copy("TestData/TarGzFiles/baseLayer.tar.gz", "TestData/.image/layers/baseLayer.tar.gz", true);
            OrasClient orasClient = new OrasClient(imageReference, "TestData");
            var output = orasClient.PushImage();
            ClearFolder("TestData/.image/layers");
        }

        [Theory]
        [MemberData(nameof(GetValidImageReference))]
        public void GivenAValidImageReference_WhenPullImageUseOras_ImageWillBePulled(string imageReference)
        {
            ClearFolder("TestTemplates/.ImageLayers");
            OrasClient orasClient = new OrasClient(imageReference, "TestTemplates");
            var output = orasClient.PullImage();
        }

        private void ClearFolder(string directory)
        {
            if (!Directory.Exists(directory))
            {
                return;
            }

            DirectoryInfo folder = new DirectoryInfo(directory);
            folder.Delete(true);
        }
    }
}
