using System.Collections.Generic;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;

namespace Rsk.IdentityServer.Migration.Writers
{
    public interface IApiResourceWriter
    {
        Task Write(IEnumerable<ApiResource> resources, IEnumerable<ApiScope> scopes);
    }
}