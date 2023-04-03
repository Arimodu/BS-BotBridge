using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using System.Collections.Generic;

namespace BS_BotBridge_Core.UI
{
    [HotReload(RelativePathToLayout = @"BSBBViewController.bsml")]
    [ViewDefinition("BS_BotBridge_Core.UI.BSBBViewController.bsml")]
    internal class BSBBViewController : BSMLAutomaticViewController
    {
        private readonly List<object> _buttons = new List<object>();

        [UIValue("buttons")]
        internal List<object> Buttons
        {
            get { return _buttons; }
        }
    }
}
