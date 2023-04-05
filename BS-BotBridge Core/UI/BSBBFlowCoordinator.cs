using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using BS_BotBridge_Core.Managers;
using HMUI;
using Zenject;

namespace BS_BotBridge_Core.UI
{
    internal class BSBBFlowCoordinator : FlowCoordinator, IInitializable
    {
        protected BSBBViewController _bSBBViewController;
        protected BSBBCoreViewController _bSBBCoreViewController;
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
        internal void InjectDependencies(BSBBViewController viewController, BSBBCoreViewController coreViewController, BSBBModuleManager moduleManager)
        {
            _bSBBViewController = viewController;
            _moduleManager = moduleManager;
            _bSBBCoreViewController = coreViewController;
        }

        public void Initialize()
        {
            _bSBBViewController.Buttons.Clear();

            var coreButton = new MenuButton("Core", "Connection settings", CoreButtonWasPressed);
            _bSBBViewController.Buttons.Add(coreButton);

            foreach (var module in _moduleManager.GetModules())
            {
                MenuButton button;

                var flowCoordinator = module.FlowCoordinator;
                var displayText = module.DisplayName;
                if (flowCoordinator == null || displayText == null) continue;

                var hoverText = module.HoverText;
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

        private void CoreButtonWasPressed() => PresentViewController(_bSBBCoreViewController, animationDirection: ViewController.AnimationDirection.Vertical);

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            if (topViewController == _bSBBCoreViewController)
            {
                DismissViewController(topViewController);
                return;
            }
            BeatSaberUI.MainFlowCoordinator.DismissFlowCoordinator(this);
        }
    }
}
