// --------------------------------------------------------------------------
// <copyright file="TemplateHostingConfiguration.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.TemplateManagement.Configurations
{
    public class TemplateHostingConfiguration
    {
        public static readonly string TemplateHosting = "TemplateHosting";

        public StorageAccountConfiguration StorageAccountConfiguration { get; set; }
    }
}
