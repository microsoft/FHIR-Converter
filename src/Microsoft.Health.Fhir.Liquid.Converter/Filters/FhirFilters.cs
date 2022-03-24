// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Liquid.Converter
{
    /// <summary>
    /// Filters for conversion
    /// </summary>
    public partial class Filters
    {
        public static Dictionary<string, object> GetResource(object[] input, string resourceType)
        {
            foreach (Dictionary<string, object> resource in input)
            {
                var resourceDict = (Dictionary<string, object>)resource["resource"];
                if ((string)resourceDict["resourceType"] == resourceType) {
                    return resourceDict;
                }
            }

            return null;
        }

        public static Dictionary<string, object> GetByCode(object[] input, string code)
        {
            foreach (Dictionary<string, object> item in input)
            {
                if (item.ContainsKey("type"))
                {
                    Dictionary<string, object> iterCode = (Dictionary<string, object>)item["type"];
                    object[] codingArray = (object[])iterCode["coding"];
                    Dictionary<string, object> coding = (Dictionary<string, object>)codingArray[0];
                    if ((string)coding["code"] == code)
                    {
                        return item;
                    }
                }
            }

            return null;
        }

        public static Dictionary<string, object> GetByUrl(object[] input, string url)
        {
            foreach (Dictionary<string, object> item in input)
            {
                if (item.ContainsKey("url"))
                {
                    if ((string)item["url"] == url)
                    {
                        return item;
                    }
                }
            }

            return null;
        }
    }
}
