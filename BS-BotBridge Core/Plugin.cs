using IPA;
using IPA.Config.Stores;
using BeatSaberMarkupLanguage.MenuButtons;
using System;
using System.Collections.Generic;
using UnityEngine;
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
        private Client _client;
        internal List<object> _moduleButtonsList = new List<object>();

        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }

        [Init]
        public void Init(IPALogger logger, Config conf, PluginMetadata pluginMetadata, Zenjector zenject)
        {
            zenject.UseLogger(logger);
            zenject.UseMetadataBinder<Plugin>();

            zenject.Install<BSBBAppInstaller>(Location.App, conf.Generated<PluginConfig>());
            zenject.Install<BSBBMenuInstaller>(Location.Menu);

            Instance = this;
            Log.Debug("BSBB Config loaded");
            _moduleButtonsList.Add(new MenuButton("Core", "Core settings, GO HERE FIRST!", OnCoreButtonPressed));
            _client = new Client(PluginConfig.Instance.ServerAddress, PluginConfig.Instance.ServerPort, Log);
            Log.Info("BS-BotBridge Core initialized.");
        }

        [OnStart]
        public void OnApplicationStart()
        {
            new GameObject("BS_BotBridge_CoreController").AddComponent<BS_BotBridge_CoreController>();
        }

        [OnExit]
        public void OnApplicationQuit()
        {
        }

        /// <summary>
        /// Registers a module to recieve data from the connection.
        /// </summary>
        /// <param name="name">A name to identify when packets are for this module. (This needs to be identical to what you send from the bot)</param>
        /// <param name="dataCallback">A callback for the server thread to call with data.</param>
        /// <param name="moduleButton">Optionally a menu button, if you have settings to change</param>
        public void RegisterModule(string name, Action<object> dataCallback, MenuButton moduleButton = null)
        {
            _client._moduleCallbacks.Add(name, dataCallback);
            if (moduleButton != null) _moduleButtonsList.Add(moduleButton);
        }

        // This is very ugly and horrible, but I have no idea how to get it from other code.
        // Shouldnt cause to much trouble in terms of efficiency I think
        public static List<object> GetModuleButtonsList()
        {
            return Instance._moduleButtonsList;
        }

        private void OnCoreButtonPressed()
        {
            Log.Info("Core button pressed");
        }
    }
}
