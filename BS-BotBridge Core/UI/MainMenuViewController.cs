using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using System;
using System.Collections.Generic;


namespace BS_BotBridge_Core.UI
{
    [HotReload(RelativePathToLayout = @"MainMenuViewController.bsml")]
    [ViewDefinition("BS_BotBridge_Core.UI.MainMenuViewController.bsml")]
    internal class MainMenuViewController : BSMLAutomaticViewController
    {
        [UIValue("buttons")]
        private List<object> btns = Plugin.GetModuleButtonsList(); // "Theoretically", this shouldnt be called before the list is complete

        [UIAction("#post-parse")]
        internal void PostParse()
        {
        }
    }
}
