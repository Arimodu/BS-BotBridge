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
        protected ViewController _bSBBViewController;


        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (addedToHierarchy)
            {
                SetTitle("Bot Bridge Settings");
                showBackButton = true;

                if (_bSBBViewController == null)
                {
                    _bSBBViewController = BeatSaberUI.CreateViewController<UI.BSBBViewController>();
                }
                ProvideInitialViewControllers(_bSBBViewController);
            }
        }

        protected override void BackButtonWasPressed(ViewController _)
        {
            BeatSaberUI.MainFlowCoordinator.DismissFlowCoordinator(this);
        }
    }
}
