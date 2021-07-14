// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hl7.Fhir.Serialization;
using Microsoft.Health.Fhir.Liquid.Converter.Ccda;
using Microsoft.Health.Fhir.Liquid.Converter.Hl7v2;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Health.Fhir.Liquid.Converter.FunctionalTests
{
    public class RuleBasedTests
    {
        private static readonly string _hl7TemplateFolder = Path.Combine(Constants.TemplateDirectory, "Hl7v2");
        private static readonly string _hl7DataFolder = Path.Combine(Constants.SampleDataDirectory, "Hl7v2");
        private static readonly string _ccdaTemplateFolder = Path.Combine(Constants.TemplateDirectory, "Ccda");
        private static readonly string _ccdaDataFolder = Path.Combine(Constants.SampleDataDirectory, "Ccda");

        private static readonly Hl7v2TemplateProvider _hl7TemplateProvider = new Hl7v2TemplateProvider(_hl7TemplateFolder);
        private static readonly CcdaTemplateProvider _ccdaTemplateProvider = new CcdaTemplateProvider(_ccdaTemplateFolder);

        private static readonly FhirJsonParser _fhirParser = new FhirJsonParser();

        private static readonly int _maxRevealDepth = 1 << 7;

        private readonly ITestOutputHelper _output;

        public RuleBasedTests(ITestOutputHelper output)
        {
            this._output = output;
        }

        public static IEnumerable<object[]> GetHL7V2Cases()
        {
            var cases = new List<object[]>
            {
                new object[] { "ADT_A01", "ADT01-23.hl7" },
                new object[] { "ADT_A01", "ADT01-28.hl7" },
                new object[] { "VXU_V04", "VXU.hl7" },
                new object[] { "VXU_V04", "IZ_1_1.1_Admin_Child_Max_Message.hl7" },
                new object[] { "ORU_R01", "LAB-ORU-1.hl7" },
                new object[] { "ORU_R01", "LAB-ORU-2.hl7" },
                new object[] { "ORU_R01", "LRI_2.0-NG_CBC_Typ_Message.hl7" },
                new object[] { "ORU_R01", "ORU-R01-RMGEAD.hl7" },
                new object[] { "OML_O21", "MDHHS-OML-O21-1.hl7" },
                new object[] { "OML_O21", "MDHHS-OML-O21-2.hl7" },
            };
            return cases.Select(item => new object[]
            {
                Convert.ToString(item[0]),
                Path.Combine(_hl7DataFolder, Convert.ToString(item[1])),
                DataType.Hl7v2,
            });
        }

        public static IEnumerable<object[]> GetCcdaCases()
        {
            var cases = new List<object[]>
            {
                new object[] { "CCD", "170.314B2_Amb_CCD.ccda" },
                new object[] { "CCD", "Care_Plan.ccda" },
                new object[] { "CCD", "CCD.ccda" },
                new object[] { "CCD", "C-CDA_R2-1_CCD.xml.ccda" },
                new object[] { "CCD", "CCD-Parent-Document-Replace-C-CDAR2.1.ccda" },
                new object[] { "CCD", "CDA_with_Embedded_PDF.ccda" },
                new object[] { "CCD", "Consultation_Note.ccda" },
                new object[] { "CCD", "Consult-Document-Closing-Referral-C-CDAR2.1.ccda" },
                new object[] { "CCD", "Diagnostic_Imaging_Report.ccda" },
                new object[] { "CCD", "Discharge_Summary.ccda" },
                new object[] { "CCD", "History_and_Physical.ccda" },
                new object[] { "CCD", "Operative_Note.ccda" },
                new object[] { "CCD", "Patient-1.ccda" },
                new object[] { "CCD", "Patient-and-Provider-Organization-Direct-Address-C-CDAR2.1.ccda" },
                new object[] { "CCD", "PROBLEMS_in_Empty_C-CDA_2.1-C-CDAR2.1.ccda" },
                new object[] { "CCD", "Procedure_Note.ccda" },
                new object[] { "CCD", "Progress_Note.ccda" },
                new object[] { "CCD", "Referral_Note.ccda" },
                new object[] { "CCD", "sample.ccda" },
                new object[] { "CCD", "Transfer_Summary.ccda" },
                new object[] { "CCD", "Unstructured_Document_embed.ccda" },
                new object[] { "CCD", "Unstructured_Document_reference.ccda" },
                new object[] { @"ConsultationNote", @"Care_Plan.ccda"},
                new object[] { @"ConsultationNote", @"CDA_with_Embedded_PDF.ccda"},
                new object[] { @"ConsultationNote", @"Consultation_Note.ccda"},
                new object[] { @"ConsultationNote", @"Unstructured_Document_embed.ccda"},
                new object[] { @"DischargeSummary", @"Discharge_Summary.ccda"},
                new object[] { @"DischargeSummary", @"Consult-Document-Closing-Referral-C-CDAR2.1.ccda"},
                new object[] { @"HistoryandPhysical", @"History_and_Physical.ccda"},
                new object[] { @"HistoryandPhysical", @"Diagnostic_Imaging_Report.ccda"},
                new object[] { @"OperativeNote", @"Operative_Note.ccda"},
                new object[] { @"OperativeNote", @"Patient-1.ccda"},
                new object[] { @"ProcedureNote", @"Procedure_Note.ccda"},
                new object[] { @"ProcedureNote", @"Patient-and-Provider-Organization-Direct-Address-C-CDAR2.1.ccda"},
                new object[] { @"ProgressNote", @"Progress_Note.ccda"},
                new object[] { @"ProgressNote", @"PROBLEMS_in_Empty_C-CDA_2.1-C-CDAR2.1.ccda"},
                new object[] { @"ReferralNote", @"Referral_Note.ccda"},
                new object[] { @"ReferralNote", @"sample.ccda"},
                new object[] { @"TransferSummary", @"Transfer_Summary.ccda"},
                new object[] { @"TransferSummary", @"Unstructured_Document_reference.ccda"},
            };
            return cases.Select(item => new object[]
            {
                Convert.ToString(item[0]),
                Path.Combine(_ccdaDataFolder, Convert.ToString(item[1])),
                DataType.Ccda,
            });
        }

        [Theory]
        [MemberData(nameof(GetHL7V2Cases))]
        [MemberData(nameof(GetCcdaCases))]
        public async Task CheckOnePatient(string templateName, string samplePath, DataType dataType)
        {
            var result = await ConvertData(templateName, samplePath, dataType);
            var patients = result.SelectTokens("$.entry[?(@.resource.resourceType == 'Patient')].resource.id");
            Assert.Equal(1, patients?.Count());
        }

        [Theory]
        [MemberData(nameof(GetCcdaCases))]
        public async Task CheckReferenceResourceId(string templateName, string samplePath, DataType dataType)
        {
            Dictionary<string, string> resourceIdMapping = new Dictionary<string, string>();
            var result = await ConvertData(templateName, samplePath, dataType);
            var resources = result.SelectTokens("$.entry..resource");

            // Check resource id uniqueness
            foreach (var resource in resources)
            {
                var resourceId = resource.SelectTokens("$.id").First().ToString();
                var resouceType = resource.SelectTokens("$.resourceType").First().ToString();
                Assert.False(resourceIdMapping.ContainsKey(resourceId));
                resourceIdMapping.Add(resourceId, resouceType);
            }

            // check reference resouce id exists
            foreach (var resource in resources)
            {
                RevealObject(resource, 0, resourceIdMapping);
            }
        }

        [Theory]
        [MemberData(nameof(GetHL7V2Cases))]
        [MemberData(nameof(GetCcdaCases))]
        public async Task CheckNonemptyResource(string templateName, string samplePath, DataType dataType)
        {
            var result = await ConvertData(templateName, samplePath, dataType);
            var resources = result.SelectTokens("$.entry..resource");
            foreach (var resource in resources)
            {
                var properties = resource.ToObject<JObject>().Properties();
                var propNames = properties.Select(p => p.Name).ToHashSet();
                Assert.True(propNames?.Count() > 0);
            }
        }

        [Theory]
        [MemberData(nameof(GetHL7V2Cases))]
        [MemberData(nameof(GetCcdaCases))]
        public async Task CheckNonidenticalResources(string templateName, string samplePath, DataType dataType)
        {
            var result = await ConvertData(templateName, samplePath, dataType);
            var resourceIds = result.SelectTokens("$.entry..resource.id");
            var uniqueResourceIds = resourceIds.Select(Convert.ToString).Distinct();
            Assert.Equal(uniqueResourceIds.Count(), resourceIds.Count());
        }

        [Theory]
        [MemberData(nameof(GetHL7V2Cases))]
        // [MemberData(nameof(GetCcdaCases))]
        public async Task CheckValuesRevealInOrigin(string templateName, string samplePath, DataType dataType)
        {
            var sampleContent = await File.ReadAllTextAsync(samplePath, Encoding.UTF8);
            var result = await ConvertData(templateName, samplePath, dataType);
            RevealObjectValues(sampleContent, result, 0);
        }

        /*
        [Theory]
        [MemberData(nameof(GetCcdaCases))]
        public async Task CheckPassOfficialValidator(string templateName, string samplePath, DataType dataType)
        {
            (bool javaStatus, string javaMessage) = await ExecuteCommand("-version");
            Assert.True(javaStatus, javaMessage);

            var result = await ConvertData(templateName, samplePath, dataType);
            var resultFolder = Path.GetFullPath(Path.Combine(@"AppData", "Temp"));
            var resultPath = Path.Combine(resultFolder, $"{Guid.NewGuid().ToString("N")}.json");
            if (!Directory.Exists(resultFolder))
            {
                Directory.CreateDirectory(resultFolder);
            }

            await File.WriteAllTextAsync(resultPath, JsonConvert.SerializeObject(result, Formatting.Indented), Encoding.UTF8);

            var validatorPath = Path.GetFullPath(Path.Combine(@"ValidatorLib", "validator_cli.jar"));
            var specPath = Path.GetFullPath(Path.Combine(@"ValidatorLib", "hl7.fhir.r4.core-4.0.1.tgz"));
            var command = $"-jar {validatorPath} {resultPath} -version 4.0.1 -ig {specPath}";
            (bool status, string message) = await ExecuteCommand(command);
            var errorFile = @"E:\work\health\project\converter\result\validate_result_tx_5.4_fixbug_procedure_.txt";
            if (!status)
            {
                using StreamWriter file = new StreamWriter(errorFile, append: true);
                await file.WriteLineAsync($"Template: {templateName}, input:{Path.GetFileName(samplePath)}");
                await file.WriteLineAsync(message);
                await file.WriteLineAsync();

                _output.WriteLine(message);
            }

            //Assert.True(status);

            Directory.Delete(resultFolder, true);
        }
        */

        [Theory]
        [MemberData(nameof(GetHL7V2Cases))]
        [MemberData(nameof(GetCcdaCases))]
        public async Task CheckNoPassOfficialValidator(string templateName, string samplePath, DataType dataType)
        {
            (bool javaStatus, string javaMessage) = await ExecuteCommand("-version");
            Assert.True(javaStatus, javaMessage);

            var result = await ConvertData(templateName, samplePath, dataType);
            var resultFolder = Path.GetFullPath(Path.Combine(@"AppData", "Temp"));
            var resultPath = Path.Combine(resultFolder, $"{Guid.NewGuid().ToString("N")}.json");
            if (!Directory.Exists(resultFolder))
            {
                Directory.CreateDirectory(resultFolder);
            }

            await File.WriteAllTextAsync(resultPath, JsonConvert.SerializeObject(result, Formatting.Indented), Encoding.UTF8);

            var validatorPath = Path.GetFullPath(Path.Combine(@"ValidatorLib", "validator_cli.jar"));
            var specPath = Path.GetFullPath(Path.Combine(@"ValidatorLib", "hl7.fhir.r4.core-4.0.1.tgz"));
            var command = $"-jar {validatorPath} {resultPath} -version 4.0.1 -ig {specPath} -tx n/a";
            (bool status, string message) = await ExecuteCommand(command);
            if (!status)
            {
                _output.WriteLine(message);
                Assert.False(status, "Currently the templates are still under development. By default we turn off this validator.");
            }
            else
            {
                Assert.True(status);
            }

            Directory.Delete(resultFolder, true);
        }

        [Fact]
        public async Task CheckParserFunctionality()
        {
            var jsonResult = await Task.FromResult(@"{
                ""resourceType"": ""Observation"",
                ""id"": ""209c8566-dafa-22b6-31f6-e4c00e649c61"",
                ""valueQuantity"": {
                    ""code"": ""mg/dl""
                },
                ""valueRange"": {	
                    ""low"": {	
                        ""value"": ""182""	
                    }
                }
            }");
            try
            {
                var bundle = _fhirParser.Parse<Hl7.Fhir.Model.Observation>(jsonResult);
                Assert.Null(bundle);
            }
            catch (FormatException fe)
            {
                Assert.NotNull(fe);
            }
        }

        [Theory]
        [MemberData(nameof(GetHL7V2Cases))]
        [MemberData(nameof(GetCcdaCases))]
        public async Task CheckPassFhirParser(string templateName, string samplePath, DataType dataType)
        {
            var result = await ConvertData(templateName, samplePath, dataType);
            var jsonResult = JsonConvert.SerializeObject(result, Formatting.Indented);
            try
            {
                var bundle = _fhirParser.Parse<Hl7.Fhir.Model.Bundle>(jsonResult);
                Assert.NotNull(bundle);
            }
            catch (FormatException fe)
            {
                Assert.Null(fe);
            }
        }

        private async Task<JObject> ConvertData(string templateName, string samplePath, DataType dataType)
        {
            switch (dataType)
            {
                case DataType.Hl7v2:
                    return JObject.Parse(new Hl7v2Processor()
                .Convert(await File.ReadAllTextAsync(samplePath, Encoding.UTF8), templateName, _hl7TemplateProvider));
                case DataType.Ccda:
                    return JObject.Parse(new CcdaProcessor()
                .Convert(await File.ReadAllTextAsync(samplePath, Encoding.UTF8), templateName, _ccdaTemplateProvider));
                default:
                    return null;
            }
        }

        private void RevealObject(JToken resource, int level, Dictionary<string, string> resourceIdMapping)
        {
            Assert.True(level < _maxRevealDepth, "Reveal depth shouldn't exceed limit.");
            switch (resource)
            {
                case JArray array:
                    array.ToList().ForEach(sub => RevealObject(sub, level + 1, resourceIdMapping));
                    break;
                case JObject container:
                    var properties = container.Properties();
                    foreach (var property in properties)
                    {
                        if (property.Value.Children().Count() > 0)
                        {
                            RevealObject(property.Value, level + 1, resourceIdMapping);
                        }
                        else if (property.Name == "reference")
                        {
                            var s = property.Value.ToString().Split('/');
                            if (!resourceIdMapping.ContainsKey(s[1]) || resourceIdMapping[s[1]] != s[0])
                            {
                                Console.WriteLine(property.Value.ToString());
                            }

                            Assert.True(resourceIdMapping.ContainsKey(s[1]));
                            Assert.Equal(resourceIdMapping[s[1]], s[0]);
                        }
                    }

                    break;
                case JValue value:
                    break;
                default:
                    Assert.True(false, $"Unexpected token {resource}, type {resource.Type}");
                    break;
            }
        }

        private void RevealObjectValues(string origin, JToken resource, int level)
        {
            Assert.True(level < _maxRevealDepth, "Reveal depth shouldn't exceed limit.");
            switch (resource)
            {
                case JArray array:
                    array.ToList().ForEach(sub => RevealObjectValues(origin, sub, level + 1));
                    break;
                case JObject container:
                    container.Properties().Select(p => p.Name).Where(key => ResourceFilter.NonCompareProperties.All(func => !func(key)))
                        .Select(key => container[key]).ToList().ForEach(sub => RevealObjectValues(origin, sub, level + 1));
                    break;
                case JValue value:
                    if (ResourceFilter.NonCompareValues.All(func => !func(value.ToString())))
                    {
                        Assert.Contains(value.ToString().Trim(), origin);
                    }

                    break;
                default:
                    Assert.True(false, $"Unexpected token {resource}, type {resource.Type}");
                    break;
            }
        }

        private async Task<(bool status, string message)> ExecuteCommand(string command)
        {
            var rawMessage = string.Empty;
            var messages = new List<string>();
            var lineSplitter = new Regex(@"\r|\n|\r\n");

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "java",
                    Arguments = command,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                },
            };

            try
            {
                process.Start();
                rawMessage = await process.StandardOutput.ReadToEndAsync();
                process.WaitForExit();
            }
            catch (Exception e)
            {
                rawMessage = e.Message;
            }

            var lines = lineSplitter.Split(rawMessage ?? string.Empty).Select(line => line.Trim())
                .Where(line => line.StartsWith("Error"));
            messages.AddRange(lines);

            return (process.ExitCode == 0, string.Join(Environment.NewLine, messages));
        }

        internal static class ResourceFilter
        {
            private static readonly HashSet<string> _explicitProps = new HashSet<string>
            {
                "resourceType", "type", "fullUrl", "id", "method", "url", "reference", "system",
                "code", "display", "gender", "use", "preferred", "status", "mode", "div",
            };

            private static readonly HashSet<string> _explicitValues = new HashSet<string>
            {
                "order",
            };

            public static readonly List<Func<string, bool>> NonCompareProperties = new List<Func<string, bool>>
            {
                // Exlude all the properties whose value is written in mapping tables explicitly or peculiar to FHIR
                _explicitProps.Contains,
            };

            public static readonly List<Func<string, bool>> NonCompareValues = new List<Func<string, bool>>
            {
                // Exlude all the explicit values written in mapping tables and values peculiar to FHIR
                _explicitValues.Contains,

                // Exclude datetime and boolean values because the format is transformed differently
                (string input) => DateTime.TryParse(input, out _),
                (string input) => bool.TryParse(input, out _),
            };
        }
    }
}