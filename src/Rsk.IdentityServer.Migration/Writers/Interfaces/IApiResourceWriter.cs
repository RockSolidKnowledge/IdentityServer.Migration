﻿using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Models;

namespace Rsk.IdentityServer.Migration.Writers
{
    public interface IApiResourceWriter
    {
        Task Write(IEnumerable<ApiResource> resources);
    }
}