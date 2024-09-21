// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Microsoft.Health.Fhir.Liquid.Converter.Fluid.Filters;
using Microsoft.Health.Fhir.Liquid.Converter.Fluid.Tags;

namespace Microsoft.Health.Fhir.Liquid.Converter.Fluid
{
    public static class FluidParserFactory
    {
        static FluidParserFactory()
        {
            TemplateOptions.Default.Filters.RegisterCollectionFilter()
                .RegisterDateFilter()
                .RegisterGeneralFilter()
                .RegisterMathFilter()
                .RegisterSectionFilter()
                .RegisterSegmentFilter()
                .RegisterStringFilter();

            TemplateOptions.Default.MemberAccessStrategy = new UnsafeMemberAccessStrategy()
            {
                IgnoreCasing = true,
            };

            // TemplateOptions.Default.FileProvider =
        }

        /// <summary>
        /// Creates an instances of <see cref="FluidParser"/> with customer filters registered.
        /// </summary>
        /// <returns>A new instance of <see cref="FluidParser"/></returns>
        public static FluidParser CreateParser()
        {
            var options = new FluidParserOptions { AllowFunctions = true };
            var parser = new FluidParser(options);

            parser.RegisterParserTag("evaluate", EvaluateTagParser.Instance, EvaluateHandler);

            return parser;
        }

        private static async ValueTask<Completion> EvaluateHandler(EvaluateTag tag, TextWriter textWriter, TextEncoder textEncoder, TemplateContext context)
        {
            await Task.Delay(0);

            return Completion.Normal;
        }
    }
}
