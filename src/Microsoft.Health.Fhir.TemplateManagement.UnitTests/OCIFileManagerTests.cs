using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace Microsoft.Health.Fhir.TemplateManagement.UnitTests
{
    public class OCIFileManagerTests
    {
        [Fact]
        public void GivenAnImageReferenceAndOutputFolder_WhenPullOCIFiles_CorrectFilesWillBeWrittenToFolder()
        {
            return;
            string imageReference = "OCIsowuacr.azurecr.io/templateoras:default";
            string outputFolder = "TestData/testOCIFileManager";
            var testManager = new OCIFileManager(imageReference, outputFolder);
            testManager.PullOCIImageAsync();
            testManager.UnpackOCIImage();
            //Assert.Equal(818, Directory.EnumerateFiles(outputFolder, "*.*", SearchOption.AllDirectories).Count());
        }

        [Fact]
        public void GivenAnImageReferenceAndInputFolder_WhenPushOCIFiles_CorrectImageWillBePushed()
        {
            return;
            string imageReference = "OCIsowuacr.azurecr.io/templateoras:default";
            string inputFolder = "TestData/UserFolder";
            var testManager = new OCIFileManager(imageReference, inputFolder);
            testManager.PackOCIImage(true);
            testManager.PushOCIImageAsync();
        }
    }
}
