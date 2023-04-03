using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using System;
using System.Collections.Generic;


namespace BS_BotBridge_Core.UI
{
    [HotReload(RelativePathToLayout = @"BSBBCoreViewController.bsml")]
    [ViewDefinition("BS_BotBridge_Core.UI.BSBBCoreViewController.bsml")]
    internal class BSBBCoreViewController : BSMLAutomaticViewController
    {
        private string yourTextField = "Hello World";
        public string YourTextProperty
        {
            get { return yourTextField; }
            set
            {
                if (yourTextField == value) return;
                yourTextField = value;
                NotifyPropertyChanged();
            }
        }

        [UIAction("#post-parse")]
        internal void PostParse()
        {
            // Code to run after BSML finishes
        }
    }
}
