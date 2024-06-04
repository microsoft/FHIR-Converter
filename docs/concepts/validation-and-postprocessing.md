# Resource validation and post-processing

The output of converter depends on the templates as well as the quality and richness of input messages. Therefore, it is important that you review and validate the Converter output before using those in production.

In general, you can use [HL7 FHIR validator](https://wiki.hl7.org/Using_the_FHIR_Validator) to validate a FHIR resource. You may be able to fix some of the conversion issues by appropriately changing the templates. For other issues, you may need to have a post-processing step in your pipeline.

In some cases, due to lack of field level data in the incoming messages, the Converter may produce resources without useful information or even without ID. You can use `Hl7.Fhir.R4` .NET library to filter such resources in your pipeline. Here is the sample code for such purpose.

```C#
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

public class PostProcessor
{
    private readonly FhirJsonParser _parser = new FhirJsonParser();

    public IEnumerable<Resource> FilterResources(IEnumerable<string> fhirResources)
    {
        return fhirResources
            .Select(fhirResource => _parser.Parse<Resource>(fhirResource))
            .Where(resource => !IsEmptyResource(resource))
            .Where(resource => !IsIdAbsentResource(resource));
    }

    public bool IsEmptyResource(Resource resource)
    {
        try
        {
            var fhirResource = resource.ToJObject();
            var properties = fhirResource.Properties().Select(property => property.Name);
            // an empty resource contains no properties other than "resourceType" and "id"
            return !properties
                .Where(property => !property.Equals("resourceType"))
                .Where(property => !property.Equals("id"))
                .Any();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            // deal with the exception...
        }

        return false;
    }

    public bool IsIdAbsentResource(Resource resource)
    {
        try
        {
            return string.IsNullOrWhiteSpace(resource.Id);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            // deal with the exception...
        }
        return false;
    }
}
```