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

            TemplateOptions.Default.MemberAccessStrategy = UnsafeMemberAccessStrategy.Instance;

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

            // {% evaluate patientId using 'ID/Patient' PID: firstSegments.PID, type: 'First' -%}
            // {% assign fullPatientId = patientId | prepend: 'Patient/' -%}
            // {% evaluate messageHeaderId using 'ID/MessageHeader' MSH: firstSegments.MSH -%}

            parser.RegisterExpressionTag("evaluate", GenericExpressionHandler);
            return parser;
        }

        private static ValueTask<Completion> GenericExpressionHandler(Expression expression, TextWriter textWriter, TextEncoder textEncoder, TemplateContext context)
        {
            return new ValueTask<Completion>(Completion.Normal);
        }
    }
}
