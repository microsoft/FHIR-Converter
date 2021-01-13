// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.Cda;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.FilterTests
{
    public class SectionFiltersTests
    {
        [Fact]
        public void GetFirstCdaSectionsTests()
        {
            const string sectionNameContent = "Problems";

            // Empty data
            Assert.Empty(Filters.GetFirstCdaSections(new Hash(), sectionNameContent));

            // Empty section name content
            var data = LoadTestData() as Dictionary<string, object>;
            var msg = data?.GetValueOrDefault("msg") as IDictionary<string, object>;
            Assert.Empty(Filters.GetFirstCdaSections(Hash.FromDictionary(msg), string.Empty));

            // Valid data and section name content
            var sections = Filters.GetFirstCdaSections(Hash.FromDictionary(msg), sectionNameContent);
            Assert.Single(sections);
            Assert.Equal(5, ((Dictionary<string, object>)sections[sectionNameContent]).Count);

            // Null data or section name content
            Assert.Throws<NullReferenceException>(() => Filters.GetFirstCdaSections(null, sectionNameContent));
            Assert.Throws<NullReferenceException>(() => Filters.GetFirstCdaSections(new Hash(), null));
        }

        [Fact]
        public void GetCdaSectionListsTests()
        {
            const string sectionNameContent = "Problems";

            // Empty data
            Assert.Empty(Filters.GetCdaSectionLists(new Hash(), sectionNameContent));

            // Empty section name content
            var data = LoadTestData() as Dictionary<string, object>;
            var msg = data?.GetValueOrDefault("msg") as IDictionary<string, object>;
            Assert.Empty(Filters.GetCdaSectionLists(Hash.FromDictionary(msg), string.Empty));

            // Valid data and section name content
            var sectionLists = Filters.GetCdaSectionLists(Hash.FromDictionary(msg), sectionNameContent);
            Assert.Single(sectionLists);

            var sections = (List<object>)sectionLists[sectionNameContent];
            Assert.Single(sections);
            Assert.Equal(5, ((Dictionary<string, object>)sections[0]).Count);

            // Null data or section name content
            Assert.Throws<NullReferenceException>(() => Filters.GetCdaSectionLists(null, sectionNameContent));
            Assert.Throws<NullReferenceException>(() => Filters.GetCdaSectionLists(new Hash(), null));
        }

        [Fact]
        public void GetFirstCdaSectionsByTemplateIdTests()
        {
            const string templateIdContent = "2.16.840.1.113883.10.20.22.2.6.1";

            // Empty data
            Assert.Empty(Filters.GetFirstCdaSectionsByTemplateId(new Hash(), templateIdContent));

            // Empty template id content
            var data = LoadTestData() as Dictionary<string, object>;
            var msg = data?.GetValueOrDefault("msg") as IDictionary<string, object>;
            Assert.Empty(Filters.GetFirstCdaSectionsByTemplateId(Hash.FromDictionary(msg), string.Empty));

            // Valid data and template id content
            var sections = Filters.GetFirstCdaSectionsByTemplateId(Hash.FromDictionary(msg), templateIdContent);
            Assert.Single(sections);
            Assert.Equal(5, ((Dictionary<string, object>)sections["2_16_840_1_113883_10_20_22_2_6_1"]).Count);

            // Null data or template id content
            Assert.Throws<NullReferenceException>(() => Filters.GetFirstCdaSectionsByTemplateId(null, templateIdContent));
            Assert.Throws<NullReferenceException>(() => Filters.GetFirstCdaSectionsByTemplateId(new Hash(), null));
        }

        private static IDictionary<string, object> LoadTestData()
        {
            var parser = new CdaDataParser();
            var dataContent = File.ReadAllText(Path.Join("..", "..", "data", "SampleData", "Cda", "170.314B2_Amb_CCD.cda"));
            return parser.Parse(dataContent);
        }
    }
}
