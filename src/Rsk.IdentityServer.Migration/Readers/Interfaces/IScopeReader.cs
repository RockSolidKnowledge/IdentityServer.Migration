using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer3.EntityFramework.Entities;

namespace Rsk.IdentityServer.Migration.Readers
{
    public interface IScopeReader
    {
        Task<List<Scope>> Read();
    }
}