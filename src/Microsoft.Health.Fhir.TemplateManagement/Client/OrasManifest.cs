// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.ContainerRegistry.Models;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.TemplateManagement.Client
{
    public class OrasManifest
    {
        /// <summary>
        /// Gets or sets media type for this Manifest
        /// </summary>
        [JsonProperty(PropertyName = "mediaType")]
        public string MediaType { get; set; }

        /// <summary>
        /// Gets or sets image config descriptor
        /// </summary>
        [JsonProperty(PropertyName = "config")]
        public Descriptor Config { get; set; }

        /// <summary>
        /// Gets or sets list of image layer information
        /// </summary>
        [JsonProperty(PropertyName = "layers")]
        public IList<Descriptor> Layers { get; set; }

        /// <summary>
        /// Gets or sets schema version
        /// </summary>
        [JsonProperty(PropertyName = "schemaVersion")]
        public int? SchemaVersion { get; set; }
    }
}
