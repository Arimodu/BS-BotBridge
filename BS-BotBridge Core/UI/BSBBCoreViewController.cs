using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using BS_BotBridge_Core.Configuration;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using Zenject;

namespace BS_BotBridge_Core.UI
{
    [HotReload(RelativePathToLayout = @"BSBBCoreViewController.bsml")]
    [ViewDefinition("BS_BotBridge_Core.UI.BSBBCoreViewController.bsml")]
    internal class BSBBCoreViewController : BSMLAutomaticViewController
    {
        private BSBBCoreConfig _config;

        [Inject]
        public void InjectDependencies(BSBBCoreConfig config, SiraLog logger)
        {
            logger.Info("Injecting deps into " + typeof(BSBBCoreViewController).FullName);
            _config = config;
            ConnectionEnabled = _config.ConnectionEnalbed;
            Address = _config.ServerAddress;
            Port = _config.ServerPort.ToString();
            NotifyPropertyChanged(nameof(ConnectionEnabled));
            NotifyPropertyChanged(nameof(Address));
            NotifyPropertyChanged(nameof(Port));

            _config.OnChanged += Config_OnChanged;
        }

        private void Config_OnChanged()
        {
            ConnectionEnabled = _config.ConnectionEnalbed;
            Address = _config.ServerAddress;
            Port = _config.ServerPort.ToString();
            NotifyPropertyChanged(nameof(ConnectionEnabled));
            NotifyPropertyChanged(nameof(Address));
            NotifyPropertyChanged(nameof(Port));
        }

        private bool _connectionEnabled = false;
        public bool ConnectionEnabled
        {
            get { return _connectionEnabled; }
            set
            {
                if (value == _connectionEnabled) return;
                _connectionEnabled = value;
                NotifyPropertyChanged(nameof(ConnectionEnabled));
                _config.ConnectionEnalbed = value;
                _config.Changed();
            }
        }

        private string _address = "Config error";
        public string Address 
        { 
            get { return _address; }
            set
            {
                if (value == _address) return;
                _address = value;
                NotifyPropertyChanged(nameof(Address));
                _config.ServerAddress = value;
                _config.Changed();
            }
        }
        private string _port = "Config error";
        public string Port
        {
            get { return _port; }
            set
            {
                if (value == _port) return;
                _port = value;
                NotifyPropertyChanged(nameof(Port));
                if (!int.TryParse(value, out int port))
                    Port = _config.ServerPort.ToString();
                if (port == _config.ServerPort) return;
                _config.ServerPort = port;
                _config.Changed();
            }
        }
    }
}
