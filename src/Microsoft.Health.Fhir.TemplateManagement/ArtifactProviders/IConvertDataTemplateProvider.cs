using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DotLiquid;

namespace Microsoft.Health.Fhir.TemplateManagement.ArtifactProviders
{
    public interface IConvertDataTemplateProvider
    {
        public Task<List<Dictionary<string, Template>>> GetTemplateCollectionAsync(CancellationToken cancellationToken);
    }
}