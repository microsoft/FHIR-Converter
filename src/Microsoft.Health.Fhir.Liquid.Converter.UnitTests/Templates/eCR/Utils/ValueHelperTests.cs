using System.Collections.Generic;
using System.IO;
using DotLiquid;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests
{
    public class ValueHelperTests : BaseECRLiquidTests
    {
        private static readonly string ECRPath = Path.Join(
            TestConstants.ECRTemplateDirectory, "Utils", "_ValueHelper.liquid"
        );

        [Fact]
        public void GivenNoAttributeReturnsEmpty()
        {
            ConvertCheckLiquidTemplate(ECRPath, new Dictionary<string, object>(), "\"valueString\": \"\",");
        }

        [Fact]
        public void GivenDecimalProperlyReturnsWithDecimal()
        {
            var attributes = new Dictionary<string, object>{
                {"value", Hash.FromAnonymousObject(new { value = ".29"})}
            };
            ConvertCheckLiquidTemplate(ECRPath, attributes, "\"valueQuantity\": { \"value\": 0.29, },");
        }

        [Fact]
        public void GivenIntProperlyReturnsInt()
        {
            var attributes = new Dictionary<string, object>{
                {"value", Hash.FromAnonymousObject(new { value = "300"})}
            };
            ConvertCheckLiquidTemplate(ECRPath, attributes, "\"valueQuantity\": { \"value\": 300, },");
        }

        [Fact]
        public void GivenDecimalProperlyReturnsDecimal()
        {
            var attributes = new Dictionary<string, object>{
                {"value", Hash.FromAnonymousObject(new { value = "300.00"})}
            };
            ConvertCheckLiquidTemplate(ECRPath, attributes, "\"valueQuantity\": { \"value\": 300.00, },");
        }

        [Fact]
        public void GivenNegativeValueProperlyReturnsNegativeValue()
        {
            var attributes = new Dictionary<string, object>{
                {"value", Hash.FromAnonymousObject(new { value = "-300.00"})}
            };
            ConvertCheckLiquidTemplate(ECRPath, attributes, "\"valueQuantity\": { \"value\": -300.00, },");
        }

        [Fact]
        public void GivenValueUnitProperlyReturnsWithValueUnit()
        {
            var attributes = new Dictionary<string, object>{
                {"value", Hash.FromAnonymousObject(new { value = ".29" , unit = "/d"})}
            };
            ConvertCheckLiquidTemplate(ECRPath, attributes, "\"valueQuantity\": { \"value\": 0.29, \"unit\":\"/d\", },");
        }
    }
}
