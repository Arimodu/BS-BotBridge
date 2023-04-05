using BSBBLib.Interfaces;
using SiraUtil.Logging;
using System.Collections.Generic;
using System.Linq;
using Zenject;

namespace BS_BotBridge_Core.Managers
{
    public class BSBBModuleManager
    {
        private readonly Dictionary<string, IModule> _modules = new Dictionary<string, IModule>();
        private SiraLog _logger;
        private Client _client;

        [Inject]
        internal void InjectDependencies(Client client, SiraLog logger)
        {
            _client = client;
            _logger = logger;
        }

        public void Initialize()
        {
            _logger.Info("BSBB Initializing modules...");
            if (_modules.Count == 0)
            {
                _logger.Warn("No modules loaded, this mod does nothing on its own!");
                return;
            }

            foreach (var module in _modules.Values)
            {
                module.Initialize(_client);
            }
            _logger.Info("BSBB modules initialized");
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

        /// <summary>
        /// Gets an array of all registered modules
        /// </summary>
        /// <returns>An array of all registered modules</returns>
        public IModule[] GetModules() => _modules.Values.ToArray();
    }
}
