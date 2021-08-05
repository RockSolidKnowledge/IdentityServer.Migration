using System.Collections.Generic;
using System.Threading.Tasks;
using Client = IdentityServer3.EntityFramework.Entities.Client;

namespace Rsk.IdentityServer.Migration.Readers
{
    public interface IClientReader
    {
        Task<List<Client>> Read();
    }
}