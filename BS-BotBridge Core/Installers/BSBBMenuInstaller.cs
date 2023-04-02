using Zenject;
using BS_BotBridge_Core.UI;

namespace BS_BotBridge_Core.Installers
{
    internal sealed class BSBBMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<BSBBViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<BSBBFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
            Container.BindInterfacesTo<SettingsControllerManager>().AsSingle();
        }
    }
}
