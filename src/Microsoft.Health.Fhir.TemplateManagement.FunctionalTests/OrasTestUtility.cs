// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Health.Fhir.TemplateManagement.Client;

namespace Microsoft.Health.Fhir.TemplateManagement.FunctionalTests
{
    public static class OrasTestUtility
    {
        public const string _containerRegistryServer = "localhost:5000";
        public const string _testOneLayerWithValidSequenceNumberImageReference = _containerRegistryServer + "/templatetest:onelayer_valid_sequence";
        public const string _testOneLayerWithoutSequenceNumberImageReference = _containerRegistryServer + "/templatetest:onelayer_without_sequence";
        public const string _testOneLayerWithInValidSequenceNumberImageReference = _containerRegistryServer + "/templatetest:onelayer_invalid_sequence";
        public const string _testMultiLayersWithValidSequenceNumberImageReference = _containerRegistryServer + "/templatetest:multilayers_valid_sequence";
        public const string _testMultiLayersWithInValidSequenceNumberImageReference = _containerRegistryServer + "/templatetest:multilayers_invalid_sequence";
        public const string _testInvalidCompressedImageReference = _containerRegistryServer + "/templatetest:invalid_image";

        private static readonly string _testTarGzPath = Path.Join("TestData", "TarGzFiles");
        private static string _baseLayerTemplatePath = Path.Join(_testTarGzPath, "layerbase.tar.gz");
        private static string _userLayerTemplatePath = Path.Join(_testTarGzPath, "layer2.tar.gz");
        private static string _emptySequenceNumberLayerPath = Path.Join(_testTarGzPath, "userV1.tar.gz");
        private static string _invalidCompressedImageLayerPath = Path.Join(_testTarGzPath, "invalid1.tar.gz");

        public static async Task<bool> PushOneLayerWithValidSequenceNumberAsync()
        {
            string command = $"push {_testOneLayerWithValidSequenceNumberImageReference} {_baseLayerTemplatePath}";
            return await ExecuteOrasCommandAsync(command);
        }

        public static async Task<bool> PushOneLayerWithoutSequenceNumberAsync()
        {
            string command = $"push {_testOneLayerWithoutSequenceNumberImageReference} {_emptySequenceNumberLayerPath}";
            return await ExecuteOrasCommandAsync(command);
        }

        public static async Task<bool> PushOneLayerWithInvalidSequenceNumberAsync()
        {
            string command = $"push {_testOneLayerWithInValidSequenceNumberImageReference} {_userLayerTemplatePath}";
            return await ExecuteOrasCommandAsync(command);
        }

        public static async Task<bool> PushMultiLayersWithValidSequenceNumberAsync()
        {
            string command = $"push {_testMultiLayersWithValidSequenceNumberImageReference} {_baseLayerTemplatePath} {_userLayerTemplatePath}";
            return await ExecuteOrasCommandAsync(command);
        }

        public static async Task<bool> PushMultiLayersWithInValidSequenceNumberAsync()
        {
            string command = $"push {_testMultiLayersWithInValidSequenceNumberImageReference} {_baseLayerTemplatePath} {_emptySequenceNumberLayerPath}";
            return await ExecuteOrasCommandAsync(command);
        }

        public static async Task<bool> PushInvalidCompressedImageAsync()
        {
            string command = $"push {_testInvalidCompressedImageReference} {_invalidCompressedImageLayerPath}";
            return await ExecuteOrasCommandAsync(command);
        }

        private static async Task<bool> ExecuteOrasCommandAsync(string command)
        {
            try
            {
                await OrasClient.OrasExecutionAsync(command, Directory.GetCurrentDirectory());
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
