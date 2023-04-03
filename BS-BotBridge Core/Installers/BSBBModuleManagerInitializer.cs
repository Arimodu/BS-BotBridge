using BS_BotBridge_Core.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace BS_BotBridge_Core.Installers
{
    /// <summary>
    /// This is basically a hack to get IInitializable interface inside a bound, already existing, class
    /// </summary>
    internal class BSBBModuleManagerInitializer : IInitializable
    {
        private readonly BSBBModuleManager _manager;
        public BSBBModuleManagerInitializer(BSBBModuleManager manager)
        {
            _manager = manager;
        }

        public void Initialize()
        {
            _manager.Initialize();
        }
    }
}
