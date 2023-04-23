using BS_BotBridge_Core.Network;
using BSBBCore.Configuration;
using BSBBCore.Managers;
using Zenject;

namespace BSBBCore.Installers
{
    internal class BSBBAppInstaller : Installer
    {
        private readonly BSBBCoreConfig _config;
        private readonly BSBBModuleManager _moduleManager;

        internal BSBBAppInstaller(BSBBCoreConfig config, BSBBModuleManager moduleManager)
        {
            _config = config;
            _moduleManager = moduleManager; 
        }

        public override void InstallBindings()
        {
            Container.BindInstance(_config).AsSingle();
            Container.BindInstance(_moduleManager).AsSingle();
            Container.BindInterfacesAndSelfTo<Client>().AsSingle();
            Container.QueueForInject(_moduleManager);
            Container.BindInterfacesAndSelfTo<BSBBModuleManagerInitializer>().AsSingle().NonLazy();
        }
    }
}
