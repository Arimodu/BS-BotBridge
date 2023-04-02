using IPA;
using IPA.Config.Stores;
using IPALogger = IPA.Logging.Logger;
using Config = IPA.Config.Config;
using SiraUtil.Zenject;
using BS_BotBridge_Core.Configuration;
using BS_BotBridge_Core.Installers;
using IPA.Loader;
using BS_BotBridge_Core.Managers;
using System.Reflection;

namespace BS_BotBridge_Core
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        private BSBBModuleManager _moduleManager;

        [Init]
        public void Init(IPALogger logger, Config conf, Zenjector zenject)
        {
            // Add ourselves to IPA for modules to initialize with us
            _moduleManager = new BSBBModuleManager();
            PluginInitInjector.AddInjector(typeof(BSBBModuleManager), ConstructManager);

            zenject.UseLogger(logger);
            zenject.UseMetadataBinder<Plugin>();

            zenject.Install<BSBBAppInstaller>(Location.App, conf.Generated<PluginConfig>(), _moduleManager);
            zenject.Install<BSBBMenuInstaller>(Location.Menu);
        }

        private object ConstructManager(object previous, ParameterInfo param, PluginMetadata meta)
        {
            return _moduleManager;
        }
    }
}
