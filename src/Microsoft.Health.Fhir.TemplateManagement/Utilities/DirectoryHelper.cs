// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.IO;
using EnsureThat;

namespace Microsoft.Health.Fhir.TemplateManagement.Utilities
{
    public static class DirectoryHelper
    {
        public static void ClearFolder(string directory)
        {
            EnsureArg.IsNotNullOrEmpty(directory, nameof(directory));

            if (!Directory.Exists(directory))
            {
                return;
            }

            DirectoryInfo folder = new DirectoryInfo(directory);
            foreach (FileInfo file in folder.GetFiles())
            {
                file.Delete();
            }

            foreach (DirectoryInfo dir in folder.GetDirectories())
            {
                dir.Delete(true);
            }
        }
    }
}
