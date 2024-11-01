// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Health.Fhir.Liquid.Converter.Tool.Models;

namespace Microsoft.Health.Fhir.Liquid.Converter.Tool
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var parseResult = Parser.Default.ParseArguments<ConverterOptions, PullTemplateOptions, PushTemplateOptions>(args);
            try
            {
                parseResult.WithParsed<ConverterOptions>(ConverterLogicHandler.Convert);
                await parseResult.WithParsedAsync<PullTemplateOptions>(TemplateManagementLogicHandler.PullAsync);
                await parseResult.WithParsedAsync<PushTemplateOptions>(TemplateManagementLogicHandler.PushAsync);
                parseResult.WithNotParsed(HandleOptionsParseError);
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Ex: {ex} StackTrace: '{Environment.StackTrace}'");
                Console.Error.WriteLine($"HERE Process failed: {ex.Message}");
                return -1;
            }
        }

        private static void HandleOptionsParseError(IEnumerable<Error> errors)
        {
            if (!errors.IsHelp() && !errors.IsVersion())
            {
                throw new InputParameterException(@"The input option is invalid.");
            }
        }
    }
}
