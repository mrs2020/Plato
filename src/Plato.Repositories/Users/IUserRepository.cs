﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plato.Repositories
{
    public interface IUserRepository<T> : IRepository<T> where T : class
    {
    }
}
