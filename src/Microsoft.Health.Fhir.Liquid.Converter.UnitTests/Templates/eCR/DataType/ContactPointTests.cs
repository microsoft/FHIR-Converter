using System.Collections.Generic;
using System.IO;
using DotLiquid;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests
{
    public class ContactPointTests : BaseECRLiquidTests
    {
        private static readonly string ECRPath = Path.Join(
            TestConstants.ECRTemplateDirectory, "DataType", "_ContactPoint.liquid"
        );

        [Fact]
        public void GivenNoAttributeReturnsEmpty()
        {
            ConvertCheckLiquidTemplate(ECRPath, new Dictionary<string, object>(), string.Empty);
        }

        [Fact]
        public void GivenTelValueReturnsPhone()
        {
            var attributes = new Dictionary<string, object>{
                {"ContactPoint", Hash.FromAnonymousObject(new { value = "tel:123" })}
            };
            ConvertCheckLiquidTemplate(
                ECRPath, 
                attributes, 
                @"""system"":""phone"", ""value"": ""123"", ""use"": """",");
        }

        [Fact]
        public void GivenTelValuAndUseReturnsPhone()
        {
            var attributes = new Dictionary<string, object>{
                {"ContactPoint", Hash.FromAnonymousObject(new { value = "tel:123", use="H" })}
            };
            ConvertCheckLiquidTemplate(
                ECRPath, 
                attributes, 
                @"""system"":""phone"", ""value"": ""123"", ""use"": ""home"",");
        }

        [Fact]
        public void GivenTelValuAndPagerUseReturnsPager()
        {
            var attributes = new Dictionary<string, object>{
                {"ContactPoint", Hash.FromAnonymousObject(new { value = "tel:123", use="PG" })}
            };
            ConvertCheckLiquidTemplate(
                ECRPath, 
                attributes, 
                @"""system"":""pager"", ""value"": ""123"",");
        }

        [Fact]
        public void GivenMailtoReturnsEmail()
        {
            var attributes = new Dictionary<string, object>{
                {"ContactPoint", Hash.FromAnonymousObject(new { value = "mailto:abc@me.com", use="WP" })}
            };
            ConvertCheckLiquidTemplate(
                ECRPath, 
                attributes, 
                @"""system"":""email"", ""value"": ""abc@me.com"", ""use"": ""work"",");
        }

        [Fact]
        public void GivenFaxoReturnsFax()
        {
            var attributes = new Dictionary<string, object>{
                {"ContactPoint", Hash.FromAnonymousObject(new { value = "fax:123", use="WP" })}
            };
            ConvertCheckLiquidTemplate(
                ECRPath, 
                attributes, 
                @"""system"":""fax"", ""value"": ""123"", ""use"": ""work"",");
        }
    }
   
}
