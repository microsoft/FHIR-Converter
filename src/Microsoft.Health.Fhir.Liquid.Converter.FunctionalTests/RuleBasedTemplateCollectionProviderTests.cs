// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Health.Fhir.Liquid.Converter.FunctionalTests
{
    public class RuleBasedTemplateCollectionProviderTests : BaseRuleBasedFunctionalTests, IClassFixture<TemplateCollectionProviderTestFixture>
    {
        private readonly TemplateCollectionProviderTestFixture _fixture;
        private readonly ITestOutputHelper _output;

        public RuleBasedTemplateCollectionProviderTests(ITestOutputHelper output, TemplateCollectionProviderTestFixture fixture)
            : base(output)
        {
            _output = output;
            _fixture = fixture;
        }

        [Theory]
        [MemberData(nameof(GetHL7V2RuleBasedTestCases))]
        [MemberData(nameof(GetCcdaRuleBasedTestCases))]
        public async Task GivenDataAndTemplateCollectionProvider_WhenConvertDataCalled_ThenValidateOnePatient(string templateName, string samplePath, DataType dataType)
        {
            await ConvertAndValidatePatientCount(_fixture.TemplateProvider, templateName, samplePath, dataType);
        }

        [Theory]
        [MemberData(nameof(GetCcdaRuleBasedTestCases))]
        public async Task GivenCCDAData_WhenConvertData_ThenValidateReferenceResourceId(string templateName, string samplePath, DataType dataType)
        {
            await ConvertAndValidateReferenceResourceId(_fixture.TemplateProvider, templateName, samplePath, dataType);
        }

        [Theory]
        [MemberData(nameof(GetHL7V2RuleBasedTestCases))]
        [MemberData(nameof(GetCcdaRuleBasedTestCases))]
        public async Task GivenDataAndTemplateCollectionProvider_WhenConvertDataCalled_ThenValidateNonemptyResource(string templateName, string samplePath, DataType dataType)
        {
            await ConvertAndValidateNonemptyResource(_fixture.TemplateProvider, templateName, samplePath, dataType);
        }

        [Theory]
        [MemberData(nameof(GetHL7V2RuleBasedTestCases))]
        [MemberData(nameof(GetCcdaRuleBasedTestCases))]
        public async Task GivenDataAndTemplateCollectionProvider_WhenConvertDataCalled_ThenValidateNonidenticalResources(string templateName, string samplePath, DataType dataType)
        {
            await ConvertAndValidateNonidenticalResources(_fixture.TemplateProvider, templateName, samplePath, dataType);
        }

        [Theory]
        [MemberData(nameof(GetHL7V2RuleBasedTestCases))]
        public async Task GivenDataAndTemplateCollectionProvider_WhenConvertDataCalled_ThenValidateValuesRevealInOrigin(string templateName, string samplePath, DataType dataType)
        {
            await ConvertAndValidateValuesRevealInOrigin(_fixture.TemplateProvider, templateName, samplePath, dataType);
        }

        [Theory]
        [MemberData(nameof(GetHL7V2RuleBasedTestCases))]
        [MemberData(nameof(GetCcdaRuleBasedTestCases))]
        public async Task GivenDataAndTemplateCollectionProvider_WhenConvertDataCalled_ThenValidatePassOfficialValidator(string templateName, string samplePath, DataType dataType)
        {
            await ConvertAndValidatePassOfficialValidator(_fixture.TemplateProvider, templateName, samplePath, dataType);
        }

        [Fact]
        public async Task GivenDataAndTemplateCollectionProvider_WhenConvertDataCalled_ThenValidateParserFunctionality()
        {
            await ConvertAndValidateParserFunctionality();
        }

        [Theory]
        [MemberData(nameof(GetHL7V2RuleBasedTestCases))]
        [MemberData(nameof(GetCcdaRuleBasedTestCases))]
        public async Task GivenDataAndTemplateCollectionProvider_WhenConvertDataCalled_ThenValidatePassFhirParser(string templateName, string samplePath, DataType dataType)
        {
            await ConvertAndValidatePassFhirParser(_fixture.TemplateProvider, templateName, samplePath, dataType);
        }
    }
}