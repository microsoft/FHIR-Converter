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
            string outputFolder = "testdefault";
            var testManager = new OCIFileManager(imageReference, outputFolder);
            testManager.PullOCIImage();
            testManager.UnpackOCIImage();
            //Assert.Equal(818, Directory.EnumerateFiles(outputFolder, "*.*", SearchOption.AllDirectories).Count());
        }

        [Fact]
        public void GivenAnImageReferenceAndInputFolder_WhenPushOCIFiles_CorrectImageWillBePushed()
        {
            return;
            //DirectoryInfo folder = new DirectoryInfo("Snapshot");
            //folder.Delete(true);
            string imageReference = "OCIsowuacr.azurecr.io/templateoras:default";
            string inputFolder = "testdefault";
            var testManager = new OCIFileManager(imageReference, inputFolder);
            testManager.PackOCIImage();
            var output = testManager.PushOCIImage();
        }
    }
}
