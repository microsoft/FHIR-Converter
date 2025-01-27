using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Utilities;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests
{
    public class BaseECRLiquidTests
    {
        /// <summary>
        /// Given a path to an eCR template, and attributes. Check that the rendered template
        /// matches the expected contents.
        /// </summary>
        /// <param name="templatePath">Path to the template being tested</param>
        /// <param name="attributes">Dictionary of attributes to hydrate the template</param>
        /// <param name="expectedContent">Serialized string that ought to be returned</param>
        protected static void ConvertCheckLiquidTemplate(string templatePath, Dictionary<string, object> attributes, string expectedContent)
        {
            var templateContent = File.ReadAllText(templatePath);
            var template = TemplateUtility.ParseLiquidTemplate(templatePath, templateContent);
            Assert.True(template.Root.NodeList.Count > 0);

            // Set up the context
            var templateProvider = new TemplateProvider(TestConstants.ECRTemplateDirectory, DataType.Ccda);
            var context = new Context(
                environments: new List<Hash>(),
                outerScope: new Hash(),
                registers: Hash.FromDictionary(new Dictionary<string, object>() { { "file_system", templateProvider.GetTemplateFileSystem() } }),
                errorsOutputMode: ErrorsOutputMode.Display,
                maxIterations: 0,
                formatProvider: CultureInfo.InvariantCulture,
                cancellationToken: CancellationToken.None);
            context.AddFilters(typeof(Filters));

            // Add the value sets to the context
            var codeContent = File.ReadAllText(Path.Join(TestConstants.ECRTemplateDirectory, "ValueSet", "ValueSet.json"));
            var codeMapping = TemplateUtility.ParseCodeMapping(codeContent);
            Console.WriteLine(codeMapping);
            if (codeMapping?.Root?.NodeList?.First() != null)
            {
                context["CodeMapping"] = codeMapping.Root.NodeList.First();
            }

            // Hydrate the context with the attributes passed to the function
            foreach (var keyValue in attributes)
            {
                context[keyValue.Key] = keyValue.Value;
            }

            // Render and strip out unhelpful whitespace (actual post-processing gets rid of this
            // at the end of the day anyway)
            var actualContent = template.Render(RenderParameters.FromContext(context, CultureInfo.InvariantCulture)).Trim().Replace("\n", " ").Replace("\t", string.Empty);
            actualContent = Filters.CleanStringFromTabs(actualContent);

            // Many are harmless, but can be helpful for debugging
            foreach (var err in template.Errors)
            {
                Console.WriteLine(err.Message);
            }

            Assert.Equal(expectedContent, actualContent);
        }
    }
}
