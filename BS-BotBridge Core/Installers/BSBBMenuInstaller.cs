using Zenject;
using BS_BotBridge_Core.UI;
using BS_BotBridge_Core.Managers;

namespace BS_BotBridge_Core.Installers
{
    internal sealed class BSBBMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<BSBBMainViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<BSBBCoreViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<BSBBCoreFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
            Container.BindInterfacesTo<SettingsControllerManager>().AsSingle();
        }
    }
}
