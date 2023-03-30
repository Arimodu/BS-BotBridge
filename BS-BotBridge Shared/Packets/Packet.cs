using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BS_BotBridge_Shared.Packets
{
    [Serializable]
    public class Packet
    {
        public string TargetModule { get; set; }
        public object Data { get; set; }

        public Packet(string targetModule, object data)
        {
            TargetModule = targetModule;
            Data = data;
        }
    }
}
