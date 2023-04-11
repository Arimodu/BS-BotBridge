using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using System.Collections.Generic;

namespace BS_BotBridge_Core.UI
{
    [HotReload(RelativePathToLayout = @"BSBBMainViewController.bsml")]
    [ViewDefinition("BS_BotBridge_Core.UI.BSBBMainViewController.bsml")]
    internal class BSBBMainViewController : BSMLAutomaticViewController
    {
        private readonly List<object> _buttons = new List<object>();

        [UIValue("buttons")]
        internal List<object> Buttons
        {
            get { return _buttons; }
        }

        internal void NotifyButtonCollectionReloaded()
        {
            NotifyPropertyChanged(nameof(Buttons));
        }
    }
}
