﻿using System.Collections.Generic;

namespace Plato.Internal.Shell.Models
{
    public class ShellDescriptor
    {

        
        public IList<ShellFeature> Modules { get; set; } = new List<ShellFeature>();

    }
}