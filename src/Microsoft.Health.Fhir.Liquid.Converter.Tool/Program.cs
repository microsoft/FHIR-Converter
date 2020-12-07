// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using Microsoft.Health.Fhir.Liquid.Converter.Tool.Models;

namespace Microsoft.Health.Fhir.Liquid.Converter.Tool
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var parseResult = Parser.Default.ParseArguments<ConverterOptions, PullTemplateOptions, PushTemplateOptions>(args);
            int resultCode = 0;
            parseResult.WithParsed<ConverterOptions>(options => ConverterLogicHandler.Convert(options));
            await parseResult.WithParsedAsync<PullTemplateOptions>(async options => resultCode = await TemplateManagementLogicHandler.PullAsync(options));
            await parseResult.WithParsedAsync<PushTemplateOptions>(async options => resultCode = await TemplateManagementLogicHandler.PushAsync(options));
            parseResult.WithNotParsed((errors) => resultCode = HandleOptionsParseError(parseResult));
            return resultCode;
        }

        private static int HandleOptionsParseError(ParserResult<object> parseResult)
        {
            var usageText = HelpText.RenderUsageText(parseResult);
            Console.WriteLine(usageText);
            return -1;
        }
    }
}
