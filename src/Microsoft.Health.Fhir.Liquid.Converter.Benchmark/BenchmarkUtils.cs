// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.Liquid.Converter.Benchmark
{
    public static class BenchmarkUtils
    {
        public static readonly string DataDirectory = GetDataDirectory();
        public static readonly string TemplateDirectory = Path.Join(DataDirectory, "Templates");
        public static readonly string Hl7v2TemplateDirectory = Path.Join(TemplateDirectory, "Hl7v2");
        public static readonly string SampleDataDirectory = Path.Join(DataDirectory, "SampleData");

        private static readonly Dictionary<string, (string template, string datafile)> Hl7v2TemplatesAndData = GetHl7v2TemplatesAndDataFilenames();

        private const string DataDirectoryName = "data";

        private static string GetDataDirectory()
        {
            var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());

            while (currentDir != null)
            {
                var directory = currentDir.GetDirectories().FirstOrDefault(dir => dir.Name.Equals(DataDirectoryName));

                if (directory != default)
                {
                    return directory.FullName;
                }

                currentDir = currentDir.Parent;
            }

            throw new DirectoryNotFoundException($"Could not find directory named '{DataDirectoryName}' in the path.");
        }

        public static IDictionary<string, (string template, string datafile)> GetHl7v2TemplatesAndDataFiles()
        {
            return Hl7v2TemplatesAndData;
        }

        private static Dictionary<string, (string template, string datafile)> GetHl7v2TemplatesAndDataFilenames()
        {
            return new (string template, string datafile)[]
            {
                (@"ADT_A01", @"ADT-A01-01.hl7"),
                (@"ADT_A01", @"ADT-A01-02.hl7"),
                (@"ADT_A02", @"ADT-A02-01.hl7"),
                (@"ADT_A02", @"ADT-A02-02.hl7"),
                (@"ADT_A03", @"ADT-A03-01.hl7"),
                (@"ADT_A03", @"ADT-A03-02.hl7"),
                (@"ADT_A04", @"ADT-A04-01.hl7"),
                (@"ADT_A04", @"ADT-A04-02.hl7"),
                (@"ADT_A05", @"ADT-A05-01.hl7"),
                (@"ADT_A05", @"ADT-A05-02.hl7"),
                (@"ADT_A06", @"ADT-A06-01.hl7"),
                (@"ADT_A06", @"ADT-A06-02.hl7"),
                (@"ADT_A07", @"ADT-A07-01.hl7"),
                (@"ADT_A07", @"ADT-A07-02.hl7"),
                (@"ADT_A08", @"ADT-A08-01.hl7"),
                (@"ADT_A08", @"ADT-A08-02.hl7"),
                (@"ADT_A09", @"ADT-A09-01.hl7"),
                (@"ADT_A09", @"ADT-A09-02.hl7"),
                (@"ADT_A10", @"ADT-A10-01.hl7"),
                (@"ADT_A10", @"ADT-A10-02.hl7"),
                (@"ADT_A11", @"ADT-A11-01.hl7"),
                (@"ADT_A11", @"ADT-A11-02.hl7"),
                (@"ADT_A13", @"ADT-A13-01.hl7"),
                (@"ADT_A13", @"ADT-A13-02.hl7"),
                (@"ADT_A14", @"ADT-A14-01.hl7"),
                (@"ADT_A14", @"ADT-A14-02.hl7"),
                (@"ADT_A15", @"ADT-A15-01.hl7"),
                (@"ADT_A15", @"ADT-A15-02.hl7"),
                (@"ADT_A16", @"ADT-A16-01.hl7"),
                (@"ADT_A16", @"ADT-A16-02.hl7"),
                (@"ADT_A25", @"ADT-A25-01.hl7"),
                (@"ADT_A25", @"ADT-A25-02.hl7"),
                (@"ADT_A26", @"ADT-A26-01.hl7"),
                (@"ADT_A26", @"ADT-A26-02.hl7"),
                (@"ADT_A27", @"ADT-A27-01.hl7"),
                (@"ADT_A27", @"ADT-A27-02.hl7"),
                (@"ADT_A28", @"ADT-A28-01.hl7"),
                (@"ADT_A28", @"ADT-A28-02.hl7"),
                (@"ADT_A29", @"ADT-A29-01.hl7"),
                (@"ADT_A29", @"ADT-A29-02.hl7"),
                (@"ADT_A31", @"ADT-A31-01.hl7"),
                (@"ADT_A31", @"ADT-A31-02.hl7"),
                (@"ADT_A40", @"ADT-A40-01.hl7"),
                (@"ADT_A40", @"ADT-A40-02.hl7"),
                (@"ADT_A41", @"ADT-A41-01.hl7"),
                (@"ADT_A41", @"ADT-A41-02.hl7"),
                (@"ADT_A45", @"ADT-A45-01.hl7"),
                (@"ADT_A45", @"ADT-A45-02.hl7"),
                (@"ADT_A47", @"ADT-A47-01.hl7"),
                (@"ADT_A47", @"ADT-A47-02.hl7"),
                (@"ADT_A60", @"ADT-A60-01.hl7"),
                (@"ADT_A60", @"ADT-A60-02.hl7"),
                (@"SIU_S12", @"SIU-S12-01.hl7"),
                (@"SIU_S12", @"SIU-S12-02.hl7"),
                (@"SIU_S13", @"SIU-S13-01.hl7"),
                (@"SIU_S13", @"SIU-S13-02.hl7"),
                (@"SIU_S14", @"SIU-S14-01.hl7"),
                (@"SIU_S14", @"SIU-S14-02.hl7"),
                (@"SIU_S15", @"SIU-S15-01.hl7"),
                (@"SIU_S15", @"SIU-S15-02.hl7"),
                (@"SIU_S16", @"SIU-S16-01.hl7"),
                (@"SIU_S16", @"SIU-S16-02.hl7"),
                (@"SIU_S17", @"SIU-S17-01.hl7"),
                (@"SIU_S17", @"SIU-S17-02.hl7"),
                (@"SIU_S26", @"SIU-S26-01.hl7"),
                (@"SIU_S26", @"SIU-S26-02.hl7"),
                (@"ORU_R01", @"ORU-R01-01.hl7"),
                (@"ORM_O01", @"ORM-O01-01.hl7"),
                (@"ORM_O01", @"ORM-O01-02.hl7"),
                (@"ORM_O01", @"ORM-O01-03.hl7"),
                (@"ORM_O01", @"ORM-O01-04.hl7"),
                (@"ORM_O01", @"ORM-O01-05.hl7"),
                (@"ORM_O01", @"ORM-O01-06.hl7"),
                (@"MDM_T01", @"MDM-T01-01.hl7"),
                (@"MDM_T01", @"MDM-T01-02.hl7"),
                (@"MDM_T02", @"MDM-T02-01.hl7"),
                (@"MDM_T02", @"MDM-T02-02.hl7"),
                (@"MDM_T02", @"MDM-T02-03.hl7"),
                (@"MDM_T05", @"MDM-T05-01.hl7"),
                (@"MDM_T05", @"MDM-T05-02.hl7"),
                (@"MDM_T06", @"MDM-T06-01.hl7"),
                (@"MDM_T06", @"MDM-T06-02.hl7"),
                (@"MDM_T09", @"MDM-T09-01.hl7"),
                (@"MDM_T09", @"MDM-T09-02.hl7"),
                (@"MDM_T10", @"MDM-T10-01.hl7"),
                (@"MDM_T10", @"MDM-T10-02.hl7"),
                (@"RDE_O11", @"RDE-O11-01.hl7"),
                (@"RDE_O11", @"RDE-O11-02.hl7"),
                (@"RDE_O25", @"RDE-O25-01.hl7"),
                (@"RDE_O25", @"RDE-O25-02.hl7"),
                (@"RDS_O13", @"RDS-O13-01.hl7"),
                (@"RDS_O13", @"RDS-O13-02.hl7"),
                (@"OML_O21", @"OML-O21-01.hl7"),
                (@"OML_O21", @"OML-O21-02.hl7"),
                (@"OML_O21", @"OML-O21-03.hl7"),
                (@"OUL_R22", @"OUL-R22-01.hl7"),
                (@"OUL_R22", @"OUL-R22-02.hl7"),
                (@"OUL_R23", @"OUL-R23-01.hl7"),
                (@"OUL_R23", @"OUL-R23-02.hl7"),
                (@"OUL_R24", @"OUL-R24-01.hl7"),
                (@"OUL_R24", @"OUL-R24-02.hl7"),
                (@"VXU_V04", @"VXU-V04-01.hl7"),
                (@"VXU_V04", @"VXU-V04-02.hl7"),
                (@"BAR_P01", @"BAR-P01-01.hl7"),
                (@"BAR_P01", @"BAR-P01-02.hl7"),
                (@"BAR_P02", @"BAR-P02-01.hl7"),
                (@"BAR_P02", @"BAR-P02-02.hl7"),
                (@"BAR_P12", @"BAR-P12-01.hl7"),
                (@"BAR_P12", @"BAR-P12-02.hl7"),
                (@"DFT_P03", @"DFT-P03-01.hl7"),
                (@"DFT_P03", @"DFT-P03-02.hl7"),
                (@"DFT_P11", @"DFT-P11-01.hl7"),
                (@"DFT_P11", @"DFT-P11-02.hl7"),
                (@"OMG_O19", @"OMG-O19-01.hl7"),
                (@"OMG_O19", @"OMG-O19-02.hl7"),
                (@"REF_I12", @"REF-I12-01.hl7"),
                (@"REF_I12", @"REF-I12-02.hl7"),
                (@"REF_I14", @"REF-I14-01.hl7"),
                (@"REF_I14", @"REF-I14-02.hl7"),
                (@"ADT_A01", @"ADT01-23.hl7"),
                (@"ADT_A01", @"ADT01-28.hl7"),
                (@"ADT_A04", @"ADT04-23.hl7"),
                (@"ADT_A04", @"ADT04-251.hl7"),
                (@"ADT_A04", @"ADT04-28.hl7"),
                (@"OML_O21", @"MDHHS-OML-O21-1.hl7"),
                (@"OML_O21", @"MDHHS-OML-O21-2.hl7"),
                (@"ORU_R01", @"LAB-ORU-1.hl7"),
                (@"ORU_R01", @"LAB-ORU-2.hl7"),
                (@"ORU_R01", @"LRI_2.0-NG_CBC_Typ_Message.hl7"),
                (@"ORU_R01", @"ORU-R01-RMGEAD.hl7"),
                (@"VXU_V04", @"IZ_1_1.1_Admin_Child_Max_Message.hl7"),
                (@"VXU_V04", @"VXU.hl7"),
            }.Select((v, i) => new KeyValuePair<string, (string template, string datafile)>($"Test {v.template} {i}", (v.template, Path.Join(SampleDataDirectory, "Hl7v2", v.datafile))))
            .ToDictionary(k => k.Key, v => v.Value);
        }
    }
}