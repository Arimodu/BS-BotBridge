using BS_BotBridge_Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace BS_BotBridge_Core.Installers
{
    internal class BSBBAppInstaller : Installer
    {
        private readonly PluginConfig _config;

        internal BSBBAppInstaller(PluginConfig config)
        {
            _config = config;
        }

        public override void InstallBindings()
        {
            Container.BindInstance(_config);
        }
    }
}
