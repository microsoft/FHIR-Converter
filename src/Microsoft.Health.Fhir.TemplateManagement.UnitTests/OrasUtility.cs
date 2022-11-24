using Microsoft.Health.Fhir.TemplateManagement.Client;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.TemplateManagement.UnitTests
{
    public static class OrasUtility
    {
        private static readonly string _baseLayerTemplatePath = "TestData/TarGzFiles/layer1.tar.gz";
        private static readonly string _userLayerTemplatePath = "TestData/TarGzFiles/layer2.tar.gz";

        public static async Task<string> PushOneLayerImageAsync(string testOneLayerImageReference)
        {
            string command = $"push {testOneLayerImageReference} {_baseLayerTemplatePath}";
            try
            {
                var output = await OrasClient.OrasExecutionAsync(command, Directory.GetCurrentDirectory());
                var digest = GetImageDigest(output);
                return digest.Value;
            }
            catch
            {
                return null;
            }
        }

        public static async Task<string> PushMultiLayersImageAsync(string testMultiLayersImageReference)
        {
            string command = $"push {testMultiLayersImageReference} {_baseLayerTemplatePath} {_userLayerTemplatePath}";
            try
            {
                var output = await OrasClient.OrasExecutionAsync(command, Directory.GetCurrentDirectory());
                var digest = GetImageDigest(output);
                return digest.Value;
            }
            catch
            {
                return null;
            }
        }

        private static Digest GetImageDigest(string input)
        {
            var digests = Digest.GetDigest(input);
            if (digests.Count == 0)
            {
                throw new OciClientException(TemplateManagementErrorCode.OrasProcessFailed, "Failed to parse image digest.");
            }

            return digests[0];
        }
    }
}
