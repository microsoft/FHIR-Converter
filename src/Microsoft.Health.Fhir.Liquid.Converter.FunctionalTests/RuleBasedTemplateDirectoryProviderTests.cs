// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Health.Fhir.Liquid.Converter.FunctionalTests
{
    public class RuleBasedTemplateDirectoryProviderTests : BaseRuleBasedFunctionalTests
    {
        private static readonly string _hl7TemplateFolder = Path.Combine(Constants.TemplateDirectory, "Hl7v2");
        private static readonly string _ccdaTemplateFolder = Path.Combine(Constants.TemplateDirectory, "Ccda");

        private static readonly ITemplateProvider _hl7TemplateProvider = new TemplateProvider(_hl7TemplateFolder, DataType.Hl7v2);
        private static readonly ITemplateProvider _ccdaTemplateProvider = new TemplateProvider(_ccdaTemplateFolder, DataType.Ccda);

        private static readonly Dictionary<DataType, ITemplateProvider> TemplateProviderDataTypeMap = new ()
        {
            { DataType.Hl7v2, _hl7TemplateProvider },
            { DataType.Ccda, _ccdaTemplateProvider },
        };

        private readonly ITestOutputHelper _output;

        public RuleBasedTemplateDirectoryProviderTests(ITestOutputHelper output)
            : base(output)
        {
            _output = output;
        }

        [Theory]
        [MemberData(nameof(GetHL7V2RuleBasedTestCases))]
        [MemberData(nameof(GetCcdaRuleBasedTestCases))]
        public async Task GivenDataAndTemplateDirectoryProvider_WhenConvertDataCalled_ThenValidateOnePatient(string templateName, string samplePath, DataType dataType)
        {
            await ConvertAndValidatePatientCount(TemplateProviderDataTypeMap[dataType], templateName, samplePath, dataType);
        }

        [Theory]
        [MemberData(nameof(GetCcdaRuleBasedTestCases))]
        public async Task GivenCCDAData_WhenConvertData_ThenValidateReferenceResourceId(string templateName, string samplePath, DataType dataType)
        {
            await ConvertAndValidateReferenceResourceId(TemplateProviderDataTypeMap[dataType], templateName, samplePath, dataType);
        }

        [Theory]
        [MemberData(nameof(GetHL7V2RuleBasedTestCases))]
        [MemberData(nameof(GetCcdaRuleBasedTestCases))]
        public async Task GivenDataAndTemplateDirectoryProvider_WhenConvertDataCalled_ThenValidateNonemptyResource(string templateName, string samplePath, DataType dataType)
        {
            await ConvertAndValidateNonemptyResource(TemplateProviderDataTypeMap[dataType], templateName, samplePath, dataType);
        }

        [Theory]
        [MemberData(nameof(GetHL7V2RuleBasedTestCases))]
        [MemberData(nameof(GetCcdaRuleBasedTestCases))]
        public async Task GivenDataAndTemplateDirectoryProvider_WhenConvertDataCalled_ThenValidateNonidenticalResources(string templateName, string samplePath, DataType dataType)
        {
            await ConvertAndValidateNonidenticalResources(TemplateProviderDataTypeMap[dataType], templateName, samplePath, dataType);
        }

        [Theory]
        [MemberData(nameof(GetHL7V2RuleBasedTestCases))]
        public async Task GivenDataAndTemplateDirectoryProvider_WhenConvertDataCalled_ThenValidateValuesRevealInOrigin(string templateName, string samplePath, DataType dataType)
        {
            await ConvertAndValidateValuesRevealInOrigin(TemplateProviderDataTypeMap[dataType], templateName, samplePath, dataType);
        }

        [Theory]
        [MemberData(nameof(GetHL7V2RuleBasedTestCases))]
        [MemberData(nameof(GetCcdaRuleBasedTestCases))]
        public async Task GivenDataAndTemplateDirectoryProvider_WhenConvertDataCalled_ThenValidatePassOfficialValidator(string templateName, string samplePath, DataType dataType)
        {
            await ConvertAndValidatePassOfficialValidator(TemplateProviderDataTypeMap[dataType], templateName, samplePath, dataType);
        }

        [Fact]
        public async Task GivenDataAndTemplateDirectoryProvider_WhenConvertDataCalled_ThenValidateParserFunctionality()
        {
            await ConvertAndValidateParserFunctionality();
        }

        [Theory]
        [MemberData(nameof(GetHL7V2RuleBasedTestCases))]
        [MemberData(nameof(GetCcdaRuleBasedTestCases))]
        public async Task GivenDataAndTemplateDirectoryProvider_WhenConvertDataCalled_ThenValidatePassFhirParser(string templateName, string samplePath, DataType dataType)
        {
            await ConvertAndValidatePassFhirParser(TemplateProviderDataTypeMap[dataType], templateName, samplePath, dataType);
        }
    }
}