using System.Collections.Generic;
using Client = IdentityServer3.EntityFramework.Entities.Client;

namespace Rsk.IdentityServer.Migration.Readers
{
    public interface IClientReader
    {
        IList<Client> Read();
    }
}