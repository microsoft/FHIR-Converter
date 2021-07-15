// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Health.Fhir.Liquid.Converter.Json
{
    public class DictionaryContractResolver : DefaultContractResolver
    {
        protected override JsonContract CreateContract(Type objectType)
        {
            var contract = base.CreateContract(objectType);
            contract.Converter = new DictionaryJsonConverter();
            return contract;
        }
    }
}