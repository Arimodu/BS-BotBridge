using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BS_BotBridge_Shared
{
    [Serializable]
    public class Packet
    {
        public string TargetModule { get; set; }
        public byte[] Data { get; set; }

        public Packet(string targetModule, byte[] data)
        {
            TargetModule = targetModule;
            Data = data;
        }
    }
}
