using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer3.Core.Models;

namespace Rsk.IdentityServer.Migration.Readers.Interfaces
{
    public interface IScopeReader
    {
        Task<IList<Scope>> Read();
    }
}