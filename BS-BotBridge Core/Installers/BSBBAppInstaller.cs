using BS_BotBridge_Core.Configuration;
using BS_BotBridge_Core.Managers;
using Zenject;

namespace BS_BotBridge_Core.Installers
{
    internal class BSBBAppInstaller : Installer
    {
        private readonly PluginConfig _config;
        private readonly BSBBModuleManager _moduleManager;

        internal BSBBAppInstaller(PluginConfig config, BSBBModuleManager moduleManager)
        {
            _config = config;
            _moduleManager = moduleManager; 
        }

        public override void InstallBindings()
        {
            Container.BindInstance(_config);
            Container.Bind<Client>().AsSingle();
            Container.BindInstance(_moduleManager).AsSingle();
            Container.QueueForInject(_moduleManager);
        }
    }
}
