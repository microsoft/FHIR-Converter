// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Health.Fhir.Liquid.Converter.Models
{
    public class DateTimeObject
    {
        public DateTimeOffset DateValue { get; set; }

        public bool HasTimeZone { get; set; } = true;

        public bool HasTime { get; set; } = true;

        public bool HasMilliSecond { get; set; } = true;

        public bool HasSecond { get; set; } = true;

        public bool HasMinute { get; set; } = true;

        public bool HasHour { get; set; } = true;

        public bool HasDay { get; set; } = true;

        public bool HasMonth { get; set; } = true;

        public bool HasYear { get; set; } = true;
    }
}
