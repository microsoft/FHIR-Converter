using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;
using Microsoft.Health.Fhir.TemplateManagement.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Health.Fhir.TemplateManagement.Models
{
    public class OCIFileLayer : OCIArtifactLayer
    {
        public Dictionary<string, byte[]> FileContent { get; set; }

        public OverlayMetadata Meta { get; set; }

        public override int SequenceNumber { get; set; }

        public OverlayMetadata ReadMetaData()
        {
            if (FileContent.ContainsKey(Constants.OverlayMetadataFile))
            {
                try
                {
                    var metadata = JsonConvert.DeserializeObject<OverlayMetadata>(Encoding.UTF8.GetString(FileContent[Constants.OverlayMetadataFile]));
                    return metadata;
                }
                catch (Exception ex)
                {
                    throw new OverlayException(TemplateManagementErrorCode.SequenceNumberReadFailed, "Read overlayMeta.json file failed.", ex);
                }
            }
            else
            {
                return null;
            }
        }

        public int ReadSequenceNumber()
        {
            if (SequenceNumber != -1)
            {
                return SequenceNumber;
            }

            var metaData = ReadMetaData();
            SequenceNumber = metaData == null ? -1 : (metaData.SequenceNumber <= 0 ? -1 : metaData.SequenceNumber);
            return SequenceNumber;
        }

    }
}
