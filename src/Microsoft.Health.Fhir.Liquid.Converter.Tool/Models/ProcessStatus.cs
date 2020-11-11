// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Health.Fhir.Liquid.Converter.Tool.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ProcessStatus
    {
        [EnumMember(Value = "OK")]
        OK,
        [EnumMember(Value = "Fail")]
        Fail,
    }
}
