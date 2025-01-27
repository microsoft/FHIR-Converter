using System.Collections.Generic;
using System.IO;
using DotLiquid;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests
{
    public class AddressTests : BaseECRLiquidTests
    {
        private static readonly string ECRPath = Path.Join(
            TestConstants.ECRTemplateDirectory, "DataType", "_Address.liquid"
        );

        [Fact]
        public void GivenNoAttributeReturnsEmpty()
        {
            ConvertCheckLiquidTemplate(ECRPath, new Dictionary<string, object>(), string.Empty);
        }

        [Fact]
        public void GivenSingleStreetAddresReturnsLines()
        {
            var attributes = new Dictionary<string, object>{
                {"Address", Hash.FromAnonymousObject(new { streetAddressLine = new { _ = "132 Main St" }})}
            };
            ConvertCheckLiquidTemplate(
                ECRPath, 
                attributes, 
                @"""use"": """", ""line"": [ ""132 Main St"", ], ""city"": """", ""state"": """", ""country"": """", ""postalCode"": """", ""district"": """", ""period"": { ""start"":"""", ""end"":"""", },");
        }

        [Fact]
        public void GivenArrayStreetAddresReturnsLines()
        {
            var attributes = new Dictionary<string, object>{
                {"Address", Hash.FromAnonymousObject(new { streetAddressLine = new List<object> {new {_ = "132 Main St"}, new { _ ="Unit 2"} }})}
            };
            ConvertCheckLiquidTemplate(
                ECRPath, 
                attributes, 
                @"""use"": """", ""line"": [ ""132 Main St"", ""Unit 2"", ], ""city"": """", ""state"": """", ""country"": """", ""postalCode"": """", ""district"": """", ""period"": { ""start"":"""", ""end"":"""", },");
        }

        [Fact]
        public void GivenCityReturnsCity()
        {
            var attributes = new Dictionary<string, object>{
                {"Address", Hash.FromAnonymousObject(new { city = new { _ = "Town" }})}
            };
            ConvertCheckLiquidTemplate(
                ECRPath, 
                attributes, 
                @"""use"": """", ""line"": [ ], ""city"": ""Town"", ""state"": """", ""country"": """", ""postalCode"": """", ""district"": """", ""period"": { ""start"":"""", ""end"":"""", },");
        }

        [Fact]
        public void GivenStateReturnsState()
        {
            var attributes = new Dictionary<string, object>{
                {"Address", Hash.FromAnonymousObject(new { state = new { _ = "State" }})}
            };
            ConvertCheckLiquidTemplate(
                ECRPath, 
                attributes, 
                @"""use"": """", ""line"": [ ], ""city"": """", ""state"": ""State"", ""country"": """", ""postalCode"": """", ""district"": """", ""period"": { ""start"":"""", ""end"":"""", },");
        }

        [Fact]
        public void GivenPostalCodeReturnsPostalCode()
        {
            var attributes = new Dictionary<string, object>{
                {"Address", Hash.FromAnonymousObject(new { postalCode = new { _ = "PostalCode" }})}
            };
            ConvertCheckLiquidTemplate(
                ECRPath, 
                attributes, 
                @"""use"": """", ""line"": [ ], ""city"": """", ""state"": """", ""country"": """", ""postalCode"": ""PostalCode"", ""district"": """", ""period"": { ""start"":"""", ""end"":"""", },");
        }

        [Fact]
        public void GivenCountyReturnsDistrict()
        {
            var attributes = new Dictionary<string, object>{
                {"Address", Hash.FromAnonymousObject(new { county = new { _ = "County" }})}
            };
            ConvertCheckLiquidTemplate(
                ECRPath, 
                attributes, 
                @"""use"": """", ""line"": [ ], ""city"": """", ""state"": """", ""country"": """", ""postalCode"": """", ""district"": ""County"", ""period"": { ""start"":"""", ""end"":"""", },");
        }

        [Fact]
        public void GivenCountrReturnsCountry()
        {
            var attributes = new Dictionary<string, object>{
                {"Address", Hash.FromAnonymousObject(new { country = new { _ = "Country" }})}
            };
            ConvertCheckLiquidTemplate(
                ECRPath, 
                attributes, 
                @"""use"": """", ""line"": [ ], ""city"": """", ""state"": """", ""country"": ""Country"", ""postalCode"": """", ""district"": """", ""period"": { ""start"":"""", ""end"":"""", },");
        }

        [Fact]
        public void GivenPeriodReturnsNothing()
        {
            var attributes = new Dictionary<string, object>{
                {"Address", Hash.FromAnonymousObject(new { useablePeriod = new { low = new { value = "20240313"}} })}
            };
            ConvertCheckLiquidTemplate(
                ECRPath, 
                attributes, 
                @"");
        }

        [Fact]
        public void GivenPeriodAndCountryReturnsBoth()
        {
            var attributes = new Dictionary<string, object>{
                {"Address", Hash.FromAnonymousObject(new { country = new {_ = "Country" }, useablePeriod = new { low = new { value = "20240313"}} })}
            };
            ConvertCheckLiquidTemplate(
                ECRPath, 
                attributes, 
                @"""use"": """", ""line"": [ ], ""city"": """", ""state"": """", ""country"": ""Country"", ""postalCode"": """", ""district"": """", ""period"": { ""start"":""2024-03-13"", ""end"":"""", },");
        }

    }
   
}
