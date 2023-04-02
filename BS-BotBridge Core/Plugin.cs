using IPA;
using IPA.Config.Stores;
using IPALogger = IPA.Logging.Logger;
using Config = IPA.Config.Config;
using SiraUtil.Zenject;
using BS_BotBridge_Core.Configuration;
using BS_BotBridge_Core.Installers;
using IPA.Loader;

namespace BS_BotBridge_Core
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        [Init]
        public void Init(IPALogger logger, Config conf, PluginMetadata pluginMetadata, Zenjector zenject)
        {
            zenject.UseLogger(logger);
            zenject.UseMetadataBinder<Plugin>();

            zenject.Install<BSBBAppInstaller>(Location.App, conf.Generated<PluginConfig>());
            zenject.Install<BSBBMenuInstaller>(Location.Menu);
        }
    }
}
