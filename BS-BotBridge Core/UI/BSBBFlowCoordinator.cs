using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using BS_BotBridge_Core.Managers;
using HMUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace BS_BotBridge_Core.UI
{
    internal class BSBBFlowCoordinator : FlowCoordinator, IInitializable
    {
        protected BSBBViewController _bSBBViewController;
        private BSBBModuleManager _moduleManager;

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (firstActivation)
            {
                SetTitle("Bot Bridge Settings");
                showBackButton = true;
                ProvideInitialViewControllers(_bSBBViewController);
            }
        }

        [Inject]
        internal void InjectDependencies(BSBBViewController viewController, BSBBModuleManager moduleManager)
        {
            _bSBBViewController = viewController;
            _moduleManager = moduleManager;
        }

        public void Initialize()
        {
            _bSBBViewController.Buttons.Clear();

            var coreButton = new MenuButton("Core", "Connection settings", CoreButtonWasPressed);
            _bSBBViewController.Buttons.Add(coreButton);

            foreach (var module in _moduleManager.GetModules())
            {
                MenuButton button;

                var flowCoordinator = module.GetModuleFlowCoordinator();
                var displayText = module.GetDisplayName();
                if (flowCoordinator == null || displayText == null) continue;

                var hoverText = module.GetHoverText();
                if (hoverText == null)
                {
                    button = new MenuButton(displayText, () => PresentFlowCoordinator(flowCoordinator));
                    _bSBBViewController.Buttons.Add(button);
                    continue;
                }

                button = new MenuButton(displayText, hoverText, () => PresentFlowCoordinator(flowCoordinator));
                _bSBBViewController.Buttons.Add(button);
            }
        }

        private void CoreButtonWasPressed()
        {
            //PresentViewController();
        }

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            BeatSaberUI.MainFlowCoordinator.DismissFlowCoordinator(this);
        }
    }
}
