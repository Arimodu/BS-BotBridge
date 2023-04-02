using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using System;
using System.Collections.Generic;


namespace BS_BotBridge_Core.UI
{
    [HotReload(RelativePathToLayout = @"BSBBViewController.bsml")]
    [ViewDefinition("BS_BotBridge_Core.UI.BSBBViewController.bsml")]
    internal class BSBBViewController : BSMLAutomaticViewController
    {
        [UIValue("buttons")]
        //private List<object> btns = Plugin.GetModuleButtonsList(); // "Theoretically", this shouldnt be called before the list is complete

        [UIAction("#post-parse")]
        internal void PostParse()
        {
        }
    }
}
