using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using HMUI;
using IPA.Loader;
using ModestTree;
using SiraUtil.Zenject;
using System;
using Zenject;

namespace BS_BotBridge_Core.UI
{
    // Basically stolen from HSV
    internal class SettingsControllerManager : IInitializable, IDisposable
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
            if (_menuFlowCoordinator == null) return;
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
