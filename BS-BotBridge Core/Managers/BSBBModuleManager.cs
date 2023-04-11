using BSBBLib.Interfaces;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using Zenject;

namespace BS_BotBridge_Core.Managers
{
    public class BSBBModuleManager
    {
        private SiraLog _logger;
        private Client _client;
        public readonly Dictionary<string, IModule> Modules = new Dictionary<string, IModule>();

        /// <summary>
        /// Invoked when modules are initialized
        /// </summary>
        public event Action OnModulesInitialized;

        /// <summary>
        /// True if module initialization finished
        /// </summary>
        public bool ModuleInitFinished = false;

        [Inject]
        internal void InjectDependencies(Client client, SiraLog logger)
        {
            _client = client;
            _logger = logger;
        }

        public void Initialize()
        {
            _logger.Info("BSBB Initializing modules...");
            if (Modules.Count == 0)
            {
                _logger.Warn("No modules were registered, this mod does nothing on its own!");
                _logger.Warn("You can find more modules on the official BSBB website: https://bsbb.arimodu.com/modules");
                return;
            }

            foreach (var module in Modules.Values)
            {
                module.Initialize(_client);
            }
            OnModulesInitialized?.Invoke();
            ModuleInitFinished = true;
            _logger.Info("BSBB modules initialized");
        }

        /// <summary>
        /// Registers a module to recieve data from the connection.
        /// </summary>
        /// <param name="name">A name to identify when packets are for this module. (This needs to be identical to what you send from the bot)</param>
        /// <param name="module">Your module class implementing the IModule interface</param>
        public void RegisterModule(string name, IModule module)
        {
            Modules.Add(name, module);
        }

        /// <summary>
        /// Tries to find the module specified by Identifier string.
        /// </summary>
        /// <param name="identifier">String identifier unique for the requested module</param>
        /// <returns>IModule interface of requsted module or null if not found</returns>
        public IModule FindAndGetModule(string identifier)
        {
            return Modules.TryGetValue(identifier, out var module) ? module : null;
        }
    }
}
