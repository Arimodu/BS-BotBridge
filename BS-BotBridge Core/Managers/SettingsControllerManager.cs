using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using HMUI;
using IPA.Loader;
using ModestTree;
using SiraUtil.Zenject;

namespace BS_BotBridge_Core.UI
{
    internal class SettingsControllerManager
    {
        private FlowCoordinator _menuFlowCoordinator;

        private MenuButton _bbButton;

        public SettingsControllerManager(UBinder<Plugin, PluginMetadata> pluginMetadata, BSBBFlowCoordinator flowCoordinator)
        {
            _menuFlowCoordinator = flowCoordinator;

            _bbButton = new MenuButton("BotBridge", "Make it your own!", OnMenuButtonPressed, true); ;
        }

        public void Initialize()
        {
            MenuButtons.instance.RegisterButton(_bbButton);
        }

        private void OnMenuButtonPressed()
        {
            if (_menuFlowCoordinator == null)
            {
                _menuFlowCoordinator = BeatSaberUI.CreateFlowCoordinator<UI.BSBBFlowCoordinator>();
            }
            BeatSaberUI.MainFlowCoordinator.PresentFlowCoordinatorOrAskForTutorial(_menuFlowCoordinator);
        }
    }
}
