using System.Collections.Generic;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;

namespace Rsk.IdentityServer.Migration.Writers
{
    public interface IClientWriter
    {
        Task Write(IEnumerable<Client> clients);
    }
}