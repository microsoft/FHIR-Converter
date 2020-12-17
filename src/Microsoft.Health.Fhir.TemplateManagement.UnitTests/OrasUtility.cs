// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Diagnostics;
using System.IO;

namespace Microsoft.Health.Fhir.TemplateManagement.UnitTests
{
    public static class OrasUtility
    {
        public static string OrasExecution(string command, string orasFileName)
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo(orasFileName),
            };

            process.StartInfo.Arguments = command;
            process.StartInfo.RedirectStandardError = true;
            process.EnableRaisingEvents = true;
            process.Start();

            StreamReader errStreamReader = process.StandardError;
            process.WaitForExit(30000);
            if (process.HasExited)
            {
                var error = errStreamReader.ReadToEnd();
                return error;
            }
            else
            {
                return "oras timeout.";
            }
        }

        public static void AddFilePermissionInLinuxSystem(string fileName)
        {
            var command = $"chmod +x {fileName}";
            var escapedArgs = command.Replace("\"", "\\\"");

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                },
            };

            process.Start();
            process.WaitForExit();
        }
    }
}
