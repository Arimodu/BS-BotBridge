using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using BSBBCore.UI;
using HMUI;
using IPA.Loader;
using SiraUtil.Zenject;
using System;
using Zenject;

namespace BSBBCore.Managers
{
    // Basically stolen from HSV
    internal class SettingsControllerManager : IInitializable, IDisposable
    {
        private readonly BSBBCoreFlowCoordinator _menuFlowCoordinator;

        private MenuButton _bbButton;

        public SettingsControllerManager(UBinder<Plugin, PluginMetadata> pluginMetadata, BSBBCoreFlowCoordinator flowCoordinator)
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
            if (_menuFlowCoordinator == null) return;
            // Yes, this might cause lag when opening the menu, but I have no idea how to do it differently because I know batshit about zenject
            if (!_menuFlowCoordinator.ButtonsInitialized) _menuFlowCoordinator.SetupButtons();
            BeatSaberUI.MainFlowCoordinator.PresentFlowCoordinatorOrAskForTutorial(_menuFlowCoordinator);
        }

        public void Dispose()
        {
            if (_bbButton == null) return;

            if (MenuButtons.IsSingletonAvailable && BSMLParser.IsSingletonAvailable)
            {
                MenuButtons.instance.UnregisterButton(_bbButton);
            }

            _bbButton = null;
        }
    }
}
