using BeatSaberMarkupLanguage;
using HMUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BS_BotBridge_Core.UI
{
    internal class BSBBFlowCoordinator : FlowCoordinator
    {
        protected ViewController _mainMenuViewController;


        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (addedToHierarchy)
            {
                SetTitle("Bot Bridge Settings");
                showBackButton = true;

                if (_mainMenuViewController == null)
                {
                    _mainMenuViewController = BeatSaberUI.CreateViewController<UI.MainMenuViewController>();
                }
                ProvideInitialViewControllers(_mainMenuViewController);
            }
        }

        protected override void BackButtonWasPressed(ViewController _)
        {
            BeatSaberUI.MainFlowCoordinator.DismissFlowCoordinator(this);
        }
    }
}
