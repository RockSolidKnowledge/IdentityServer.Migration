using System.Collections.Generic;
using IdentityServer3.EntityFramework.Entities;

namespace Rsk.IdentityServer.Migration.Readers
{
    public interface IScopeReader
    {
        IList<Scope> Read();
    }
}