using IPA;
using IPA.Config.Stores;
using IPALogger = IPA.Logging.Logger;
using Config = IPA.Config.Config;
using SiraUtil.Zenject;
using BSBBCore.Configuration;
using BSBBCore.Installers;
using IPA.Loader;
using BSBBCore.Managers;
using System.Reflection;

namespace BSBBCore
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    [NoEnableDisable]
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

            zenject.Install<BSBBAppInstaller>(Location.App, conf.Generated<BSBBCoreConfig>(), _moduleManager);
            zenject.Install<BSBBMenuInstaller>(Location.Menu);
        }

        private object ConstructManager(object previous, ParameterInfo param, PluginMetadata meta)
        {
            return _moduleManager;
        }
    }
}
