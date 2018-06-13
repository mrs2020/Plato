﻿using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Plato.Internal.Modules.Abstractions;

namespace Plato.Internal.Modules
{

    public class TypedModuleProvider : ITypedModuleProvider
    {

        private readonly ConcurrentDictionary<Type, IModuleEntry> _modules
            = new ConcurrentDictionary<Type, IModuleEntry>();

        private readonly IModuleManager _moduleManager;

        public TypedModuleProvider(
            IModuleManager moduleManager)
        {
            _moduleManager = moduleManager;
        }
        
        public async Task<IModuleEntry> GetModuleForDependency(Type dependency)
        {

            await BuildTypedProvider();

            if (_modules.TryGetValue(dependency, out var module))
            {
                return module;
            }

            throw new InvalidOperationException($"Could not resolve module for type {dependency.Name}");
        }

        async Task BuildTypedProvider()
        {
            var moduleEntries = await _moduleManager.LoadModulesAsync();
            foreach (var moduleEntry in moduleEntries)
            {
                // Get all valid types from module
                var types = moduleEntry.Assmeblies.SelectMany(assembly =>
                    assembly.ExportedTypes.Where(IsComponentType));
                foreach (var type in types)
                {
                    TryAdd(type, moduleEntry);
                }
            }
        }

        bool IsComponentType(Type type)
        {
            if (type == null)
                return false;
            return type.IsClass && !type.IsAbstract && type.IsPublic;
        }

        void TryAdd(Type type, IModuleEntry module)
        {
            _modules.TryAdd(type, module);
        }

    }
}