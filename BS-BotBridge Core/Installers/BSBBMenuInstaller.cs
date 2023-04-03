﻿using Zenject;
using BS_BotBridge_Core.UI;
using BS_BotBridge_Core.Managers;
using BS_BotBridge_Core.Configuration;

namespace BS_BotBridge_Core.Installers
{
    internal sealed class BSBBMenuInstaller : Installer
    {
        private BSBBCoreConfig _config;
        public BSBBMenuInstaller(BSBBCoreConfig config) 
        { 
            _config = config;
        }

        public override void InstallBindings()
        {
            Container.Bind<BSBBViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<BSBBCoreViewController>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<BSBBFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
            Container.BindInterfacesTo<SettingsControllerManager>().AsSingle();
        }
    }
}
