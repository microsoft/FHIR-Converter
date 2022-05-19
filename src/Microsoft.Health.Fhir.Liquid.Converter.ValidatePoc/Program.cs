using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Models.Json;
using Microsoft.Health.Fhir.Liquid.Converter.Processors;
using Microsoft.Health.Fhir.TemplateManagement;
using Microsoft.Health.Fhir.TemplateManagement.ArtifactProviders;
using Microsoft.Health.Fhir.TemplateManagement.Client;
using Microsoft.Health.Fhir.TemplateManagement.Models;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Liquid.Converter.ValidatePoc
{
    class Program
    {
        static void Main(string[] args)
        {
            string templateDirectory = @".\data\Templates";
            string rootTemplate = @"ExamplePatient";

            string sampleDataPath = @".\data\SampleData\ExamplePatient.json";
            var inputData = File.ReadAllText(sampleDataPath);

            ExecuteWithLocalSystem(rootTemplate, templateDirectory, inputData);
            // ExecuteWithMemorySystem(rootTemplate, inputData);
        }

        private static void ExecuteWithLocalSystem(string rootTemplate, string templateDirectory, string inputData)
        {
            var processor = new JsonProcessor();
            var templateProvider = new TemplateProvider(templateDirectory, DataType.Json);

            // Convert the input data with validate tag template
            var traceInfo = new JsonTraceInfo();
            var result = processor.Convert(inputData, rootTemplate, templateProvider, traceInfo);

            foreach (var validateSchema in traceInfo.ValidateSchemas)
            {
                Console.WriteLine(validateSchema.ToString());
            }

            Console.WriteLine(result);
        }

        private static void ExecuteWithMemorySystem(string rootTemplate, string inputData)
        {
            var pocConfig = JsonConvert.DeserializeObject<PocConfiguration>(File.ReadAllText("config.json"));

            IMemoryCache cache = new MemoryCache(new MemoryCacheOptions());
            string registry = pocConfig.Registry;
            string templateName = pocConfig.Template;
            string tag = pocConfig.Tag;

            string imageReference = string.Format("{0}/{1}:{2}", registry, templateName, tag);

            TemplateCollectionConfiguration collectionConfig = new TemplateCollectionConfiguration();
            
            string registryUsername = pocConfig.RegistryUsername;
            string registryPassword = pocConfig.RegistryPassword;
            string token = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes($"{registryUsername}:{registryPassword}"));

            ImageInfo imageInfo = ImageInfo.CreateFromImageReference(imageReference);

            var client = new AcrClient(imageInfo.Registry, token);
            var templateCollectionProvider = new TemplateCollectionProvider(imageInfo, client, cache, collectionConfig);
            var templateCollection = templateCollectionProvider.GetTemplateCollectionAsync().Result;

            foreach (var item in templateCollection)
            {
                Console.WriteLine("templateCollection: " + string.Join(",", item.Keys));
            }

            // Convert the input data with validate tag template
            var processor = new JsonProcessor();
            var traceInfo = new JsonTraceInfo();
            var templateProvider = new TemplateProvider(templateCollection);
            var result = processor.Convert(inputData, rootTemplate, templateProvider, traceInfo);

            foreach (var validateSchema in traceInfo.ValidateSchemas)
            {
                Console.WriteLine(validateSchema.ToString());
            }

            Console.WriteLine(result);
        }
    }
}
