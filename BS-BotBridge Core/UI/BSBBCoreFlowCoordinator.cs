using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using BSBBCore.Managers;
using BSBBCore.Interfaces;
using HMUI;
using SiraUtil.Logging;
using System;
using Zenject;

namespace BSBBCore.UI
{
    internal class BSBBCoreFlowCoordinator : FlowCoordinator
    {
        protected BSBBMainViewController _bSBBMainViewController;
        protected BSBBCoreViewController _bSBBCoreViewController;
        private SiraLog _logger;
        private BSBBModuleManager _moduleManager;
        private IModule DisplayedModule;

        public bool ButtonsInitialized = false;

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (firstActivation)
            {
                SetTitle("Bot Bridge Settings");
                ProvideInitialViewControllers(_bSBBMainViewController);
            }

            showBackButton = true;
        }

        [Inject]
        internal void InjectDependencies(SiraLog logger, BSBBMainViewController viewController, BSBBCoreViewController coreViewController, BSBBModuleManager moduleManager)
        {
            _logger = logger;
            _bSBBMainViewController = viewController;
            _moduleManager = moduleManager;
            _bSBBCoreViewController = coreViewController;
        }

        internal void SetupButtons()
        {
            _logger.Info("Setting up buttons...");
            _bSBBMainViewController.Buttons.Clear();

            var coreButton = new MenuButton("Core", "Connection settings", CoreButtonWasPressed);
            _bSBBMainViewController.Buttons.Add(coreButton);
            _logger.Info("Set up Core button");

            foreach (var module in _moduleManager.Modules.Values)
            {
                MenuButton button;

                if (module.ViewController == null || module.DisplayName == null) continue;

                if (module.HoverText == null) button = new MenuButton(module.DisplayName, () => PresentModuleViewControllers(module));
                else button = new MenuButton(module.DisplayName, module.HoverText, () => PresentModuleViewControllers(module));

                _bSBBMainViewController.Buttons.Add(button);
                _logger.Info($"Set up {module.DisplayName} button");
            }
            ButtonsInitialized = true;
            _bSBBMainViewController.NotifyButtonCollectionReloaded();
            _logger.Info("Button setup finished");
        }

        private void PresentModuleViewControllers(IModule module)
        {
            DisplayedModule = module;
            PresentViewController(module.ViewController);
            if (module.LeftViewController != null) SetLeftScreenViewController(module.LeftViewController, ViewController.AnimationType.In);
            if (module.RightViewController != null) SetRightScreenViewController(module.RightViewController, ViewController.AnimationType.In);
            if (module.TopViewController != null) SetTopScreenViewController(module.TopViewController, ViewController.AnimationType.In);
            if (module.BottomViewController != null) SetBottomScreenViewController(module.BottomViewController, ViewController.AnimationType.In);
        }

        private void DismissModuleViewControllers(IModule module)
        {
            DisplayedModule = null;
            DismissViewController(module.ViewController);
            if (module.LeftViewController != null) DismissViewController(module.LeftViewController);
            if (module.RightViewController != null) DismissViewController(module.RightViewController);
            if (module.TopViewController != null) DismissViewController(module.TopViewController);
            if (module.BottomViewController != null) DismissViewController(module.BottomViewController);
        }

        private void CoreButtonWasPressed() => PresentViewController(_bSBBCoreViewController, animationDirection: ViewController.AnimationDirection.Vertical);

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            if (DisplayedModule != null)
            {
                DismissModuleViewControllers(DisplayedModule);
                return;
            }

            if (topViewController == _bSBBCoreViewController)
            {
                DismissViewController(topViewController);
                return;
            }

            BeatSaberUI.MainFlowCoordinator.DismissFlowCoordinator(this);
        }
    }
}
