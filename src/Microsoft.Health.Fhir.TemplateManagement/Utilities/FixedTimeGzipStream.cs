// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using ICSharpCode.SharpZipLib.GZip;
using System;
using System.IO;

namespace Microsoft.Health.Fhir.TemplateManagement.Utilities
{
    public class FixedTimeGzipStream : GZipOutputStream
    {
        public FixedTimeGzipStream(Stream baseOutputStream)
            : base(baseOutputStream)
        {
        }

        private OutputState state_;

        private enum OutputState
        {
            Header,
            Footer,
            Finished,
            Closed
        }

        public override void Flush()
        {
            if (state_ == OutputState.Header)
            {
                WriteHeader();
            }

            base.Flush();
        }

        private void WriteHeader()
        {
            if (state_ == OutputState.Header)
            {
                state_ = OutputState.Footer;
                int num = (int)((DateTime.Now.Ticks - new DateTime(1970, 1, 1).Ticks) / 10000000);
                byte[] obj = new byte[10]
                {
                    31,
                    139,
                    8,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    255
                };
                obj[4] = (byte)num;
                obj[5] = (byte)(num >> 8);
                obj[6] = (byte)(num >> 16);
                obj[7] = (byte)(num >> 24);
                byte[] array = obj;
                baseOutputStream_.Write(array, 0, array.Length);
            }
        }
    }
}
