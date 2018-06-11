﻿using Plato.Internal.Shell.Models;

namespace Plato.Internal.Hosting
{
    public interface IPlatoHost
    {
        void Initialize();

        ShellContext GetOrCreateShellContext(ShellSettings settings);

        void UpdateShellSettings(ShellSettings settings);

        ShellContext CreateShellContext(ShellSettings settings);

    }
}
