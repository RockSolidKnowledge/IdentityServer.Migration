﻿using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Models;

namespace Rsk.IdentityServer.Migration.Writers.Interfaces
{
    public interface IIdentityResourceWriter
    {
        Task Write(IEnumerable<IdentityResource> resources);
    }
}