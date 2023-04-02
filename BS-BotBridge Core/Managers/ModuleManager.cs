using BS_BotBridge_Shared;
using System;
using System.Collections.Generic;
using Zenject;

namespace BS_BotBridge_Core.Managers
{
    internal class ModuleManager : IInitializable
    {
        private readonly Dictionary<string, IModule> _modules = new Dictionary<string, IModule>();
        private readonly Client _client;

        public ModuleManager(Client client) 
        {
            _client = client;
            _client.CreateCircularDependency(this); // Yes, this does exactly what it says it does. I dont care
        }

        public void Initialize()
        {
            //_modules.Add("core", );
        }

        /// <summary>
        /// Registers a module to recieve data from the connection.
        /// </summary>
        /// <param name="name">A name to identify when packets are for this module. (This needs to be identical to what you send from the bot)</param>
        /// <param name="module">Your module class implementing the IModule interface</param>
        public void RegisterModule(string name, IModule module)
        {
            _modules.Add(name, module);
            module.Initialize(_client.Send);
        }

        /// <summary>
        /// Tries to find the module specified by Identifier string.
        /// </summary>
        /// <param name="identifier">String identifier unique for the requested module</param>
        /// <returns>IModule interface of requsted module or null if not found</returns>
        public IModule GetModule(string identifier)
        {
            return _modules.TryGetValue(identifier, out var module) ? module : null;
        }
    }
}
