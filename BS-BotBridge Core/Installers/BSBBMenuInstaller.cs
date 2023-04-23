using Zenject;
using BSBBCore.UI;
using BSBBCore.Managers;

namespace BSBBCore.Installers
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
