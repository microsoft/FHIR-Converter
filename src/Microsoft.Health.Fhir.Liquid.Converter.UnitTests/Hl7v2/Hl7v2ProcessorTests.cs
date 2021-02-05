﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Hl7v2;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.Hl7v2
{
    public class Hl7v2ProcessorTests
    {
        private const string TestData = @"MSH|^~\&|NIST Test Lab APP|NIST Lab Facility||NIST EHR Facility|20110531140551-0500||ORU^R01^ORU_R01|NIST-LRI-NG-002.00|T|2.5.1|||AL|NE|||||LRI_Common_Component^^2.16.840.1.113883.9.16^ISO~LRI_NG_Component^^2.16.840.1.113883.9.13^ISO~LRI_RU_Component^^2.16.840.1.113883.9.14^ISO
PID|1||PATID1234^^^NIST MPI^MR||Jones^William^A||19610615|M||2106-3^White^HL70005
ORC|RE|ORD666555^NIST EHR|R-991133^NIST Lab Filler|GORD874233^NIST EHR||||||||57422^Radon^Nicholas^^^^^^NIST-AA-1^L^^^NPI
OBR|1|ORD666555^NIST EHR|R-991133^NIST Lab Filler|57021-8^CBC W Auto Differential panel in Blood^LN^4456544^CBC^99USI^^^CBC W Auto Differential panel in Blood|||20110103143428-0800|||||||||57422^Radon^Nicholas^^^^^^NIST-AA-1^L^^^NPI||||||20110104170028-0800|||F|||10093^Deluca^Naddy^^^^^^NIST-AA-1^L^^^NPI|||||||||||||||||||||CC^Carbon Copy^HL70507
OBX|1|NM|26453-1^Erythrocytes [#/volume] in Blood^LN^^^^^^Erythrocytes [#/volume] in Blood||4.41|10*6/uL^million per microliter^UCUM|4.3 to 6.2|N|||F|||20110103143428-0800|||||20110103163428-0800||||Century Hospital^^^^^NIST-AA-1^XX^^^987|2070 Test Park^^Los Angeles^CA^90067^^B|2343242^Knowsalot^Phil^^^Dr.^^^NIST-AA-1^L^^^DN
OBX|2|NM|718-7^Hemoglobin [Mass/volume] in Blood^LN^^^^^^Hemoglobin [Mass/volume] in Blood||12.5|g/mL^grams per milliliter^UCUM|13 to 18|L|||F|||20110103143428-0800|||||20110103163428-0800||||Century Hospital^^^^^NIST-AA-1^XX^^^987|2070 Test Park^^Los Angeles^CA^90067^^B|2343242^Knowsalot^Phil^^^Dr.^^^NIST-AA-1^L^^^DN
OBX|3|NM|20570-8^Hematocrit [Volume Fraction] of Blood^LN^^^^^^Hematocrit [Volume Fraction] of Blood||41|%^percent^UCUM|40 to 52|N|||F|||20110103143428-0800|||||20110103163428-0800||||Century Hospital^^^^^NIST-AA-1^XX^^^987|2070 Test Park^^Los Angeles^CA^90067^^B|2343242^Knowsalot^Phil^^^Dr.^^^NIST-AA-1^L^^^DN
OBX|4|NM|26464-8^Leukocytes [#/volume] in Blood^LN^^^^^^Leukocytes [#/volume] in Blood||105600|{cells}/uL^cells per microliter^UCUM|4300 to 10800|HH|||F|||20110103143428-0800|||||20110103163428-0800||||Century Hospital^^^^^NIST-AA-1^XX^^^987|2070 Test Park^^Los Angeles^CA^90067^^B|2343242^Knowsalot^Phil^^^Dr.^^^NIST-AA-1^L^^^DN
OBX|5|NM|26515-7^Platelets [#/volume] in Blood^LN^^^^^^Platelets [#/volume] in Blood||210000|{cells}/uL^cells per microliter^UCUM|150000 to 350000|N|||F|||20110103143428-0800|||||20110103163428-0800||||Century Hospital^^^^^NIST-AA-1^XX^^^987|2070 Test Park^^Los Angeles^CA^90067^^B|2343242^Knowsalot^Phil^^^Dr.^^^NIST-AA-1^L^^^DN
OBX|6|NM|30428-7^Erythrocyte mean corpuscular volume [Entitic volume]^LN^^^^^^Erythrocyte mean corpuscular volume [Entitic volume]||91|fL^femtoliter^UCUM|80 to 95|N|||F|||20110103143428-0800|||||20110103163428-0800||||Century Hospital^^^^^NIST-AA-1^XX^^^987|2070 Test Park^^Los Angeles^CA^90067^^B|2343242^Knowsalot^Phil^^^Dr.^^^NIST-AA-1^L^^^DN
OBX|7|NM|28539-5^Erythrocyte mean corpuscular hemoglobin [Entitic mass]^LN^^^^^^Erythrocyte mean corpuscular hemoglobin [Entitic mass]||29|pg/{cell}^picograms per cell^UCUM|27 to 31|N|||F|||20110103143428-0800|||||20110103163428-0800||||Century Hospital^^^^^NIST-AA-1^XX^^^987|2070 Test Park^^Los Angeles^CA^90067^^B|2343242^Knowsalot^Phil^^^Dr.^^^NIST-AA-1^L^^^DN
OBX|8|NM|28540-3^Erythrocyte mean corpuscular hemoglobin concentration [Mass/volume]^LN^^^^^^Erythrocyte mean corpuscular hemoglobin concentration [Mass/volume]||32.4|g/dL^grams per deciliter^UCUM|32 to 36|N|||F|||20110103143428-0800|||||20110103163428-0800||||Century Hospital^^^^^NIST-AA-1^XX^^^987|2070 Test Park^^Los Angeles^CA^90067^^B|2343242^Knowsalot^Phil^^^Dr.^^^NIST-AA-1^L^^^DN
OBX|9|NM|30385-9^Erythrocyte distribution width [Ratio]^LN^^^^^^Erythrocyte distribution width [Ratio]||10.5|%^percent^UCUM|10.2 to 14.5|N|||F|||20110103143428-0800|||||20110103163428-0800||||Century Hospital^^^^^NIST-AA-1^XX^^^987|2070 Test Park^^Los Angeles^CA^90067^^B|2343242^Knowsalot^Phil^^^Dr.^^^NIST-AA-1^L^^^DN
OBX|10|NM|26444-0^Basophils [#/volume] in Blood^LN^^^^^^Basophils [#/volume] in Blood||0.1|10*3/uL^thousand per microliter^UCUM|0 to 0.3|N|||F|||20110103143428-0800|||||20110103163428-0800||||Century Hospital^^^^^NIST-AA-1^XX^^^987|2070 Test Park^^Los Angeles^CA^90067^^B|2343242^Knowsalot^Phil^^^Dr.^^^NIST-AA-1^L^^^DN
OBX|11|NM|30180-4^Basophils/100 leukocytes in Blood^LN^^^^^^Basophils/100 leukocytes in Blood||0.1|%^percent^UCUM|0 to 2|N|||F|||20110103143428-0800|||||20110103163428-0800||||Century Hospital^^^^^NIST-AA-1^XX^^^987|2070 Test Park^^Los Angeles^CA^90067^^B|2343242^Knowsalot^Phil^^^Dr.^^^NIST-AA-1^L^^^DN
OBX|12|NM|26484-6^Monocytes [#/volume] in Blood^LN^^^^^^Monocytes [#/volume] in Blood||3|10*3/uL^thousand per microliter^UCUM|0.0 to 13.0|N|||F|||20110103143428-0800|||||20110103163428-0800||||Century Hospital^^^^^NIST-AA-1^XX^^^987|2070 Test Park^^Los Angeles^CA^90067^^B|2343242^Knowsalot^Phil^^^Dr.^^^NIST-AA-1^L^^^DN
OBX|13|NM|26485-3^Monocytes/100 leukocytes in Blood^LN^^^^^^Monocytes/100 leukocytes in Blood||3|%^percent^UCUM|0 to 10|N|||F|||20110103143428-0800|||||20110103163428-0800||||Century Hospital^^^^^NIST-AA-1^XX^^^987|2070 Test Park^^Los Angeles^CA^90067^^B|2343242^Knowsalot^Phil^^^Dr.^^^NIST-AA-1^L^^^DN
OBX|14|NM|26449-9^Eosinophils [#/volume] in Blood^LN^^^^^^Eosinophils [#/volume] in Blood||2.1|10*3/uL^thousand per microliter^UCUM|0.0 to 0.45|HH|||F|||20110103143428-0800|||||20110103163428-0800||||Century Hospital^^^^^NIST-AA-1^XX^^^987|2070 Test Park^^Los Angeles^CA^90067^^B|2343242^Knowsalot^Phil^^^Dr.^^^NIST-AA-1^L^^^DN
OBX|15|NM|26450-7^Eosinophils/100 leukocytes in Blood^LN^^^^^^Eosinophils/100 leukocytes in Blood||2|%^percent^UCUM|0 to 6|N|||F|||20110103143428-0800|||||20110103163428-0800||||Century Hospital^^^^^NIST-AA-1^XX^^^987|2070 Test Park^^Los Angeles^CA^90067^^B|2343242^Knowsalot^Phil^^^Dr.^^^NIST-AA-1^L^^^DN
OBX|16|NM|26474-7^Lymphocytes [#/volume] in Blood^LN^^^^^^Lymphocytes [#/volume] in Blood||41.2|10*3/uL^thousand per microliter^UCUM|1.0 to 4.8|HH|||F|||20110103143428-0800|||||20110103163428-0800||||Century Hospital^^^^^NIST-AA-1^XX^^^987|2070 Test Park^^Los Angeles^CA^90067^^B|2343242^Knowsalot^Phil^^^Dr.^^^NIST-AA-1^L^^^DN
OBX|17|NM|26478-8^Lymphocytes/100 leukocytes in Blood^LN^^^^^^Lymphocytes/100 leukocytes in Blood||39|%^percent^UCUM|15.0 to 45.0|N|||F|||20110103143428-0800|||||20110103163428-0800||||Century Hospital^^^^^NIST-AA-1^XX^^^987|2070 Test Park^^Los Angeles^CA^90067^^B|2343242^Knowsalot^Phil^^^Dr.^^^NIST-AA-1^L^^^DN
OBX|18|NM|26499-4^Neutrophils [#/volume] in Blood^LN^^^^^^Neutrophils [#/volume] in Blood||58|10*3/uL^thousand per microliter^UCUM|1.5 to 7.0|HH|||F|||20110103143428-0800|||||20110103163428-0800||||Century Hospital^^^^^NIST-AA-1^XX^^^987|2070 Test Park^^Los Angeles^CA^90067^^B|2343242^Knowsalot^Phil^^^Dr.^^^NIST-AA-1^L^^^DN
OBX|19|NM|26511-6^Neutrophils/100 leukocytes in Blood^LN^^^^^^Neutrophils/100 leukocytes in Blood||55|%^percent^UCUM|50 to 73|N|||F|||20110103143428-0800|||||20110103163428-0800||||Century Hospital^^^^^NIST-AA-1^XX^^^987|2070 Test Park^^Los Angeles^CA^90067^^B|2343242^Knowsalot^Phil^^^Dr.^^^NIST-AA-1^L^^^DN
OBX|20|CWE|38892-6^Anisocytosis [Presence] in Blood^LN^^^^^^Anisocytosis [Presence] in Blood||260348001^Present ++ out of ++++^SCT^^^^^^Moderate Anisocytosis|||A|||F|||20110103143428-0800|||||20110103163428-0800||||Century Hospital^^^^^NIST-AA-1^XX^^^987|2070 Test Park^^Los Angeles^CA^90067^^B|2343242^Knowsalot^Phil^^^Dr.^^^NIST-AA-1^L^^^DN
OBX|21|CWE|30400-6^Hypochromia [Presence] in Blood^LN^^^^^^Hypochromia [Presence] in Blood||260415000^not detected^SCT^^^^^^None seen|||N|||F|||20110103143428-0800|||||20110103163428-0800||||Century Hospital^^^^^NIST-AA-1^XX^^^987|2070 Test Park^^Los Angeles^CA^90067^^B|2343242^Knowsalot^Phil^^^Dr.^^^NIST-AA-1^L^^^DN
OBX|22|CWE|30424-6^Macrocytes [Presence] in Blood^LN^^^^^^Macrocytes [Presence] in Blood||260415000^not detected^SCT^^^^^^None seen|||N|||F|||20110103143428-0800|||||20110103163428-0800||||Century Hospital^^^^^NIST-AA-1^XX^^^987|2070 Test Park^^Los Angeles^CA^90067^^B|2343242^Knowsalot^Phil^^^Dr.^^^NIST-AA-1^L^^^DN
OBX|23|CWE|30434-5^Microcytes [Presence] in Blood^LN^^^^^^Microcytes [Presence] in Blood||260415000^not detected^SCT^^^^^^None seen|||N|||F|||20110103143428-0800|||||20110103163428-0800||||Century Hospital^^^^^NIST-AA-1^XX^^^987|2070 Test Park^^Los Angeles^CA^90067^^B|2343242^Knowsalot^Phil^^^Dr.^^^NIST-AA-1^L^^^DN
OBX|24|CWE|779-9^Poikilocytosis [Presence] in Blood by Light microscopy^LN^^^^^^Poikilocytosis [Presence] in Blood by Light microscopy||260415000^not detected^SCT^^^^^^None seen|||N|||F|||20110103143428-0800|||||20110103163428-0800||||Century Hospital^^^^^NIST-AA-1^XX^^^987|2070 Test Park^^Los Angeles^CA^90067^^B|2343242^Knowsalot^Phil^^^Dr.^^^NIST-AA-1^L^^^DN
OBX|25|CWE|10378-8^Polychromasia [Presence] in Blood by Light microscopy^LN^^^^^^Polychromasia [Presence] in Blood by Light microscopy||260415000^not detected^SCT^^^^^^None seen|||N|||F|||20110103143428-0800|||||20110103163428-0800||||Century Hospital^^^^^NIST-AA-1^XX^^^987|2070 Test Park^^Los Angeles^CA^90067^^B|2343242^Knowsalot^Phil^^^Dr.^^^NIST-AA-1^L^^^DN
OBX|26|TX|6742-1^Erythrocyte morphology finding [Identifier] in Blood^LN^^^^^^Erythrocyte morphology finding [Identifier] in Blood||Many spherocytes present.|||A|||F|||20110103143428-0800|||||20110103163428-0800||||Century Hospital^^^^^NIST-AA-1^XX^^^987|2070 Test Park^^Los Angeles^CA^90067^^B|2343242^Knowsalot^Phil^^^Dr.^^^NIST-AA-1^L^^^DN
OBX|27|TX|11156-7^Leukocyte morphology finding [Identifier] in Blood^LN^^^^^^Leukocyte morphology finding [Identifier] in Blood||Reactive morphology in lymphoid cells.|||A|||F|||20110103143428-0800|||||20110103163428-0800||||Century Hospital^^^^^NIST-AA-1^XX^^^987|2070 Test Park^^Los Angeles^CA^90067^^B|2343242^Knowsalot^Phil^^^Dr.^^^NIST-AA-1^L^^^DN
OBX|28|TX|11125-2^Platelet morphology finding [Identifier] in Blood^LN^^^^^^Platelet morphology finding [Identifier] in Blood||Platelets show defective granulation.|||A|||F|||20110103143428-0800|||||20110103163428-0800||||Century Hospital^^^^^NIST-AA-1^XX^^^987|2070 Test Park^^Los Angeles^CA^90067^^B|2343242^Knowsalot^Phil^^^Dr.^^^NIST-AA-1^L^^^DN
SPM|1|||119297000^BLD^SCT^^^^^^Blood|||||||||||||20110103143428-0800
";

        [Fact]
        public void GivenAValidTemplateDirectory_WhenConvert_CorrectResultShouldBeReturned()
        {
            var processor = new Hl7v2Processor();
            var templateProvider = new Hl7v2TemplateProvider(Constants.Hl7v2TemplateDirectory);
            var result = processor.Convert(TestData, "ORU_R01", templateProvider);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void GivenAValidTemplateCollection_WhenConvert_CorrectResultShouldBeReturned()
        {
            var processor = new Hl7v2Processor();
            var templateCollection = new List<Dictionary<string, Template>>
            {
                new Dictionary<string, Template>
                {
                    { "TemplateName", Template.Parse(@"{""a"":""b""}") },
                },
            };

            var templateProvider = new Hl7v2TemplateProvider(templateCollection);
            var result = processor.Convert(TestData, "TemplateName", templateProvider);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void GivenInvalidTemplateProviderOrName_WhenConvert_ExceptionsShouldBeThrown()
        {
            var processor = new Hl7v2Processor();
            var templateCollection = new List<Dictionary<string, Template>>
            {
                new Dictionary<string, Template>
                {
                    { "TemplateName", Template.Parse(@"{""a"":""b""}") },
                },
            };

            var templateProvider = new Hl7v2TemplateProvider(templateCollection);

            // Null, empty or nonexistent root template
            var exception = Assert.Throws<RenderException>(() => processor.Convert(TestData, null, templateProvider));
            Assert.Equal(FhirConverterErrorCode.NullOrEmptyRootTemplate, exception.FhirConverterErrorCode);

            exception = Assert.Throws<RenderException>(() => processor.Convert(TestData, string.Empty, templateProvider));
            Assert.Equal(FhirConverterErrorCode.NullOrEmptyRootTemplate, exception.FhirConverterErrorCode);

            exception = Assert.Throws<RenderException>(() => processor.Convert(TestData, "NonExistentTemplateName", templateProvider));
            Assert.Equal(FhirConverterErrorCode.TemplateNotFound, exception.FhirConverterErrorCode);

            // Null TemplateProvider
            exception = Assert.Throws<RenderException>(() => processor.Convert(TestData, "TemplateName", null));
            Assert.Equal(FhirConverterErrorCode.NullTemplateProvider, exception.FhirConverterErrorCode);
        }

        [Fact]
        public void GivenProcessorSettings_WhenConvert_CorrectResultsShouldBeReturned()
        {
            // Null ProcessorSettings: no time out
            var processor = new Hl7v2Processor(null);
            var templateProvider = new Hl7v2TemplateProvider(Constants.Hl7v2TemplateDirectory);
            var result = processor.Convert(TestData, "ORU_R01", templateProvider);
            Assert.True(result.Length > 0);

            // Default ProcessorSettings: no time out
            processor = new Hl7v2Processor(new ProcessorSettings());
            result = processor.Convert(TestData, "ORU_R01", templateProvider);
            Assert.True(result.Length > 0);

            // Positive time out ProcessorSettings: exception thrown when time out
            var settings = new ProcessorSettings()
            {
                TimeOut = 1,
            };

            processor = new Hl7v2Processor(settings);
            var exception = Assert.Throws<RenderException>(() => processor.Convert(TestData, "ORU_R01", templateProvider));
            Assert.Equal(FhirConverterErrorCode.TimeoutError, exception.FhirConverterErrorCode);
            Assert.True(exception.InnerException is TimeoutException);

            // Negative time out ProcessorSettings: no time out
            settings = new ProcessorSettings()
            {
                TimeOut = -1,
            };

            processor = new Hl7v2Processor(settings);
            result = processor.Convert(TestData, "ORU_R01", templateProvider);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void GivenCancellationToken_WhenConvert_CorrectResultsShouldBeReturned()
        {
            var processor = new Hl7v2Processor();
            var templateProvider = new Hl7v2TemplateProvider(Constants.Hl7v2TemplateDirectory);
            var cts = new CancellationTokenSource();
            var result = processor.Convert(TestData, "ORU_R01", templateProvider, cts.Token);
            Assert.True(result.Length > 0);

            cts.Cancel();
            Assert.Throws<OperationCanceledException>(() => processor.Convert(TestData, "ORU_R01", templateProvider, cts.Token));
        }

        [Fact]
        public void GivenEscapedMessage_WhenConverting_ExpectedCharacterShouldBeReturned()
        {
            var hl7v2Processor = new Hl7v2Processor();
            var templateDirectory = Path.Join(AppDomain.CurrentDomain.BaseDirectory, Constants.TemplateDirectory, "Hl7v2");
            var inputContent = string.Join("\n", new List<string>
            {
                @"MSH|^~\&|FOO|BAR|FOO|BAR|20201225000000|FOO|ADT^A01|123456|P|2.3|||||||||||",
                @"PR1|1|FOO|FOO^ESCAPED ONE \T\ ESCAPED TWO^BAR|ESCAPED THREE \T\ ESCAPED FOUR|20201225000000||||||||||",
            });
            var result = JObject.Parse(hl7v2Processor.Convert(inputContent, "ADT_A01", new Hl7v2TemplateProvider(templateDirectory)));

            var texts = result.SelectTokens("$.entry[?(@.resource.resourceType == 'Procedure')].resource.code.text").Select(Convert.ToString);
            var expected = new List<string> { "ESCAPED ONE & ESCAPED TWO", "ESCAPED THREE & ESCAPED FOUR" };
            Assert.NotEmpty(texts.Intersect(expected));
        }
    }
}
