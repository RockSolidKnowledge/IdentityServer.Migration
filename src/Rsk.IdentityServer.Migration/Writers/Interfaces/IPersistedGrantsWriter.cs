using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer3.EntityFramework.Entities;

namespace Rsk.IdentityServer.Migration.Writers.Interfaces
{
    public interface IPersistedGrantsWriter
    {
        Task Write(IList<Token> tokens, IList<Consent> consents);
    }
}