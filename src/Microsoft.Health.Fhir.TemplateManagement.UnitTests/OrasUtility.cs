// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Health.Fhir.TemplateManagement.Client;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Models;

namespace Microsoft.Health.Fhir.TemplateManagement.UnitTests
{
    public static class OrasUtility
    {
        private static readonly string _baseLayerTemplatePath = "TestData/TarGzFiles/layer1.tar.gz";
        private static readonly string _userLayerTemplatePath = "TestData/TarGzFiles/layer2.tar.gz";
        private const string _orasCacheEnvironmentVariableName = "ORAS_CACHE";
        private const string _defaultOrasCacheEnvironmentVariable = ".oras/cache";

        public static void InitOrasCache()
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(_orasCacheEnvironmentVariableName)))
            {
                Environment.SetEnvironmentVariable(_orasCacheEnvironmentVariableName, _defaultOrasCacheEnvironmentVariable);
            }
        }

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
