using BS_BotBridge_Shared;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using Zenject;

namespace BS_BotBridge_Core.Managers
{
    internal class BSBBModuleManager
    {
        private readonly Dictionary<string, IModule> _modules = new Dictionary<string, IModule>();
        private SiraLog _logger;
        private Client _client;

        [Inject]
        public void InjectDependencies(Client client, SiraLog logger)
        {
            _client = client;
            _logger = logger;
            _client?.CreateCircularDependency(this); // Yes, this does exactly what it says it does. I dont care

            if (_modules.Count == 0) 
                _logger.Warn("No modules loaded, this mod does nothing on its own!");

            foreach (var module in _modules.Values)
            {
                module.Initialize(_client.Send);
            }
        }

        /// <summary>
        /// Registers a module to recieve data from the connection.
        /// </summary>
        /// <param name="name">A name to identify when packets are for this module. (This needs to be identical to what you send from the bot)</param>
        /// <param name="module">Your module class implementing the IModule interface</param>
        public void RegisterModule(string name, IModule module)
        {
            _modules.Add(name, module);
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
