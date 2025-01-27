using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests
{
    public class PatientProficiencyTests : BaseECRLiquidTests
    {
        private static readonly string ECRPath = Path.Join(
            TestConstants.ECRTemplateDirectory, "Extension", "_PatientProficiency.liquid");

        [Fact]
        public void GivenNoAttributeReturnsEmpty()
        {
            ConvertCheckLiquidTemplate(ECRPath, new Dictionary<string, object>(), string.Empty);
        }

        [Fact]
        public void GivenModeCodeAndProficiencyLevelCodeReturnsBothInPatientProficiency()
        {
            var attributes = new Dictionary<string, object>
            {
                {
                    "modeCode", new Dictionary<string, string>
                    {
                        { "code", "esp" },
                    }
                },
                {
                    "proficiencyLevelCode", new Dictionary<string, string>
                    {
                        { "code", "e" },
                    }
                },
            };

            ConvertCheckLiquidTemplate(
                ECRPath,
                attributes,
                @"""url"": ""http://hl7.org/fhir/StructureDefinition/patient-proficiency"", ""extension"": [ { ""url"": ""type"", ""valueCoding"": { ""system"": ""http://terminology.hl7.org/CodeSystem/v3-LanguageAbilityMode"", ""code"": ""ESP"", ""display"": ""Expressed spoken"", }, }, { ""url"": ""level"", ""valueCoding"": { ""system"": ""http://terminology.hl7.org/CodeSystem/v3-LanguageAbilityProficiency"", ""code"": ""E"", ""display"": ""Excellent"", }, }, ],");
        }

        [Fact]
        public void GivenModeCodeReturnsModeCodeInPatientProficiency()
        {
            var attributes = new Dictionary<string, object>
            {
                {
                    "modeCode", new Dictionary<string, string>
                    {
                        { "code", "esp" },
                    }
                },
            };

            ConvertCheckLiquidTemplate(
                ECRPath,
                attributes,
                @"""url"": ""http://hl7.org/fhir/StructureDefinition/patient-proficiency"", ""extension"": [ { ""url"": ""type"", ""valueCoding"": { ""system"": ""http://terminology.hl7.org/CodeSystem/v3-LanguageAbilityMode"", ""code"": ""ESP"", ""display"": ""Expressed spoken"", }, }, ],");
        }

        [Fact]
        public void GivenProficiencyLevelCodeReturnsProficiencyLevelCodeInPatientProficiency()
        {
            var attributes = new Dictionary<string, object>
            {
                {
                    "proficiencyLevelCode", new Dictionary<string, string>
                    {
                        { "code", "e" },
                    }
                },
            };

            ConvertCheckLiquidTemplate(
                ECRPath,
                attributes,
                @"""url"": ""http://hl7.org/fhir/StructureDefinition/patient-proficiency"", ""extension"": [ { ""url"": ""level"", ""valueCoding"": { ""system"": ""http://terminology.hl7.org/CodeSystem/v3-LanguageAbilityProficiency"", ""code"": ""E"", ""display"": ""Excellent"", }, }, ],");
        }

        [Fact]
        public void GivenInvalidProficiencyLevelCodeReturnsNothing()
        {
            var attributes = new Dictionary<string, object>
            {
                {
                    "proficiencyLevelCode", new Dictionary<string, string>
                    {
                        { "code", "na" },
                    }
                },
            };

            ConvertCheckLiquidTemplate(
                ECRPath,
                attributes,
                string.Empty);
        }

        [Fact]
        public void GivenInvalidModeCodeReturnsNothing()
        {
            var attributes = new Dictionary<string, object>
        {
            {
                "modeCode", new Dictionary<string, string>
                {
                    { "code", "na" },
                }
            },
        };

            ConvertCheckLiquidTemplate(
                ECRPath,
                attributes,
                string.Empty);
        }
    }
}
