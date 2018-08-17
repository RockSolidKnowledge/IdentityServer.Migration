﻿using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer3.Core.Models;

namespace Rsk.IdentityServer.Migration.Readers
{
    public interface IClientReader
    {
        Task<IList<Client>> Read();
    }
}